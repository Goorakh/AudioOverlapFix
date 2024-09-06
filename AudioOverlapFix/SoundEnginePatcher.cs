using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace AudioOverlapFix
{
    public static class SoundEnginePatcher
    {
        public delegate void OverridePostEventDelegate(uint eventID, ref bool post);

        static event OverridePostEventDelegate _overridePostEvent;
        public static event OverridePostEventDelegate OverridePostEvent
        {
            add
            {
                _overridePostEvent += value;
                tryApplyPatches();
            }
            remove
            {
                _overridePostEvent -= value;
            }
        }

        static bool shouldPostEvent(uint eventID)
        {
            bool post = true;
            _overridePostEvent?.Invoke(eventID, ref post);
            return post;
        }

        static bool shouldPostEvent(string eventName)
        {
            return shouldPostEvent(AkSoundEngine.GetIDFromString(eventName));
        }

        static uint tryPostEvent(uint playingID, bool shouldPost)
        {
            if (!shouldPost && playingID > 0)
            {
#if DEBUG
                Log.Debug($"Stopping event ID {SoundEngineEventNameRecorder.GetEventDisplayString(AkSoundEngine.GetEventIDFromPlayingID(playingID))} (playingID={playingID})");
#endif

                AkSoundEngine.StopPlayingID(playingID);
            }

            return playingID;
        }

        static bool _hasAppliedPatches = false;
        static void tryApplyPatches()
        {
            if (_hasAppliedPatches)
                return;

            foreach (MethodInfo soundEngineMethod in typeof(AkSoundEngine).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!string.Equals(soundEngineMethod.Name, "PostEvent"))
                    continue;

                if (soundEngineMethod.ReturnType != typeof(uint))
                {
                    Log.Warning($"Unhandled return type '{soundEngineMethod.ReturnType.FullDescription()}'");
                    continue;
                }

                ParameterInfo[] parameters = soundEngineMethod.GetParameters();

                ParameterInfo eventIdOrNameParameter = null;
                foreach (ParameterInfo parameterInfo in parameters)
                {
                    if (parameterInfo.ParameterType == typeof(uint))
                    {
                        if (string.Equals(parameterInfo.Name, "in_eventID", StringComparison.OrdinalIgnoreCase))
                        {
                            eventIdOrNameParameter = parameterInfo;
                            break;
                        }
                    }
                    else if (parameterInfo.ParameterType == typeof(string))
                    {
                        if (string.Equals(parameterInfo.Name, "in_pszEventName", StringComparison.OrdinalIgnoreCase))
                        {
                            eventIdOrNameParameter = parameterInfo;
                            break;
                        }
                    }
                }

                if (eventIdOrNameParameter == null)
                {
                    Log.Warning($"Failed to find eventId or eventName parameter on method {soundEngineMethod.FullDescription()}");
                    continue;
                }

                new ILHook(soundEngineMethod, tryPostEventManipulator);

                void tryPostEventManipulator(ILContext il)
                {
                    ILCursor c = new ILCursor(il);

                    while (c.TryGotoNext(MoveType.Before, x => x.MatchRet()))
                    {
                        c.Emit(OpCodes.Ldarg, eventIdOrNameParameter.Position);
                        if (eventIdOrNameParameter.ParameterType == typeof(string))
                        {
                            c.EmitDelegate<Func<string, bool>>(shouldPostEvent);
                        }
                        else if (eventIdOrNameParameter.ParameterType == typeof(uint))
                        {
                            c.EmitDelegate<Func<uint, bool>>(shouldPostEvent);
                        }
                        else
                        {
                            throw new NotImplementedException($"Event parameter type '{eventIdOrNameParameter.ParameterType.FullDescription()}' is not implemented");
                        }

                        c.EmitDelegate(tryPostEvent);

                        c.Index++;
                    }
                }
            }

            // 8

            _hasAppliedPatches = true;
        }
    }
}
