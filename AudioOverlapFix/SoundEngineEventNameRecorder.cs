using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AudioOverlapFix
{
#if DEBUG
    static class SoundEngineEventNameRecorder
    {
        static readonly Dictionary<uint, string> _eventIDToName = new Dictionary<uint, string>();

        public static string GetEventDisplayString(uint eventID)
        {
            return TryGetEventName(eventID, out string name) ? name : eventID.ToString();
        }

        public static bool TryGetEventName(uint eventID, out string name)
        {
            return _eventIDToName.TryGetValue(eventID, out name);
        }

        public static void Init()
        {
            MethodInfo AkSoundEngine_GetIDFromString = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.GetIDFromString(default));
            if (AkSoundEngine_GetIDFromString is not null)
            {
                new Hook(AkSoundEngine_GetIDFromString, GetIDFromString_recordEventName);
            }
            else
            {
                Log.Warning("Unable to find AkSoundEngine method: static uint GetIDFromString(string in_pszString)");
            }

            const BindingFlags METHOD_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance;
            foreach (MethodInfo method in typeof(AkSoundEngine).GetMethods(METHOD_FLAGS))
            {
                if (!method.HasMethodBody())
                {
                    Log.Debug($"{method.FullDescription()} has no body");
                    continue;
                }
                
                ParameterInfo eventNameParam = Array.Find(method.GetParameters(), p => p.ParameterType == typeof(string) && p.Name == "in_pszEventName");
                if (eventNameParam is null)
                    continue;

                int eventNameArgIndex = eventNameParam.Position;
                if (!method.IsStatic)
                    eventNameArgIndex++;

                new ILHook(method, il =>
                {
                    new ILCursor(il).Emit(OpCodes.Ldarg, eventNameArgIndex).EmitDelegate(RecordEventName);
                });

                Log.Debug($"Added record event name hook to {method.FullDescription()}");
            }
        }

        delegate uint orig_AkSoundEngine_GetIDFromString(string in_pszString);
        static uint GetIDFromString_recordEventName(orig_AkSoundEngine_GetIDFromString orig, string in_pszString)
        {
            uint eventID = orig(in_pszString);

            if (eventID > 0U)
            {
                if (_eventIDToName.TryGetValue(eventID, out string recordedName))
                {
                    if (!string.Equals(recordedName, in_pszString, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Warning($"Duplicate event ID recorded ({eventID}): {nameof(recordedName)}={recordedName} {nameof(in_pszString)}={in_pszString}");
                    }
                }
                else
                {
                    _eventIDToName.Add(eventID, in_pszString);
                }
            }
            else
            {
                Log.Warning($"Invalid event? in_pszString={in_pszString}");
            }

            return eventID;
        }

        public static void RecordEventName(string name)
        {
            if (AkSoundEngine.IsInitialized())
            {
                AkSoundEngine.GetIDFromString(name);
            }
        }
    }
#endif
}
