using HarmonyLib;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using UnityEngine;

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

        static bool _hasAppliedPatches = false;
        static void tryApplyPatches()
        {
            if (_hasAppliedPatches)
                return;

            // static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources, uint in_PlayingID)
            {
                MethodInfo method = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostEvent(default(uint), default, default, default, default, default, default, default));
                if (method is not null)
                {
                    new Hook(method, (Func<uint, GameObject, uint, AkCallbackManager.EventCallback, object, uint, AkExternalSourceInfoArray, uint, uint> orig, uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources, uint in_PlayingID) =>
                    {
                        if (!shouldPostEvent(in_eventID))
                            return 0U;

                        return orig(in_eventID, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals, in_pExternalSources, in_PlayingID);
                    });
                }
                else
                {
                    Log.Warning("Unable to find AkSoundEngine method: static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources, uint in_PlayingID)");
                }
            }

            // static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources)
            {
                MethodInfo method = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostEvent(default(uint), default, default, default, default, default, default));
                if (method is not null)
                {
                    new Hook(method, (Func<uint, GameObject, uint, AkCallbackManager.EventCallback, object, uint, AkExternalSourceInfoArray, uint> orig, uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources) =>
                    {
                        if (!shouldPostEvent(in_eventID))
                            return 0U;

                        return orig(in_eventID, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals, in_pExternalSources);
                    });
                }
                else
                {
                    Log.Warning("Unable to find AkSoundEngine method: static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources)");
                }
            }

            // static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie)
            {
                MethodInfo method = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostEvent(default(uint), default, default, default, default));
                if (method is not null)
                {
                    new Hook(method, (Func<uint, GameObject, uint, AkCallbackManager.EventCallback, object, uint> orig, uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie) =>
                    {
                        if (!shouldPostEvent(in_eventID))
                            return 0U;

                        return orig(in_eventID, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie);
                    });
                }
                else
                {
                    Log.Warning("Unable to find AkSoundEngine method: static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie)");
                }
            }

            // static uint PostEvent(uint in_eventID, GameObject in_gameObjectID)
            {
                MethodInfo method = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostEvent(default(uint), default));
                if (method is not null)
                {
                    new Hook(method, (Func<uint, GameObject, uint> orig, uint in_eventID, GameObject in_gameObjectID) =>
                    {
                        if (!shouldPostEvent(in_eventID))
                            return 0U;

                        return orig(in_eventID, in_gameObjectID);
                    });
                }
                else
                {
                    Log.Warning("Unable to find AkSoundEngine method: static uint PostEvent(uint in_eventID, GameObject in_gameObjectID)");
                }
            }

            // static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources, uint in_PlayingID)
            {
                MethodInfo method = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostEvent(default(string), default, default, default, default, default, default, default));
                if (method is not null)
                {
                    new Hook(method, (Func<string, GameObject, uint, AkCallbackManager.EventCallback, object, uint, AkExternalSourceInfoArray, uint, uint> orig, string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources, uint in_PlayingID) =>
                    {
                        if (!shouldPostEvent(in_pszEventName))
                            return 0U;

                        return orig(in_pszEventName, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals, in_pExternalSources, in_PlayingID);
                    });
                }
                else
                {
                    Log.Warning("Unable to find AkSoundEngine method: static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources, uint in_PlayingID)");
                }
            }

            // static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources)
            {
                MethodInfo method = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostEvent(default(string), default, default, default, default, default, default));
                if (method is not null)
                {
                    new Hook(method, (Func<string, GameObject, uint, AkCallbackManager.EventCallback, object, uint, AkExternalSourceInfoArray, uint> orig, string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources) =>
                    {
                        if (!shouldPostEvent(in_pszEventName))
                            return 0U;

                        return orig(in_pszEventName, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals, in_pExternalSources);
                    });
                }
                else
                {
                    Log.Warning("Unable to find AkSoundEngine method: static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfoArray in_pExternalSources, uint in_PlayingID)");
                }
            }

            // static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie)
            {
                MethodInfo method = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostEvent(default(string), default, default, default, default));
                if (method is not null)
                {
                    new Hook(method, (Func<string, GameObject, uint, AkCallbackManager.EventCallback, object, uint> orig, string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie) =>
                    {
                        if (!shouldPostEvent(in_pszEventName))
                            return 0U;

                        return orig(in_pszEventName, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie);
                    });
                }
                else
                {
                    Log.Warning("Unable to find AkSoundEngine method: static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie)");
                }
            }

            // static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID)
            {
                MethodInfo method = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostEvent(default(string), default));
                if (method is not null)
                {
                    new Hook(method, (Func<string, GameObject, uint> orig, string in_pszEventName, GameObject in_gameObjectID) =>
                    {
                        if (!shouldPostEvent(in_pszEventName))
                            return 0U;

                        return orig(in_pszEventName, in_gameObjectID);
                    });
                }
                else
                {
                    Log.Warning("Unable to find AkSoundEngine method: static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID)");
                }
            }

            _hasAppliedPatches = true;
        }
    }
}
