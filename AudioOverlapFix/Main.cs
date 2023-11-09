using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AudioOverlapFix
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "AudioOverlapFix";
        public const string PluginVersion = "1.1.0";

        internal static Main Instance { get; private set; }

        internal static ConfigEntry<float> DuplicateSoundCooldown;
        internal static ConfigEntry<bool> ExcludeMithrixPizzaSound;

        static readonly HashSet<TimeStampedSoundEvent> _trackedSoundEvents = new HashSet<TimeStampedSoundEvent>(TimeStampedSoundEvent.EventIDComparer);

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            Instance = this;

            DuplicateSoundCooldown = Config.Bind("General", "Sound Cooldown", 0f, new ConfigDescription("How many seconds to keep track of sounds and prevent it from playing again. Set to 0 to only prevent duplicate sounds within the same frame"));

            ExcludeMithrixPizzaSound = Config.Bind("General", "Exclude Mithrix Pizza Attack", true, new ConfigDescription("Excludes Mithrix's pizza attack sound from the mod. The sound was seemingly designed with overlap in mind and will be very low volume if this is turned off."));

            if (RiskOfOptionsCompat.Active)
            {
                RiskOfOptionsCompat.Init();
            }

            SoundEnginePatcher.OverridePostEvent += SoundEnginePatcher_OverridePostEvent;

#if DEBUG
            SoundEngineEventNameRecorder.Init();
#endif

            SoundEventLibrary.Init();

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        void LateUpdate()
        {
            if (DuplicateSoundCooldown != null)
            {
                float soundCooldown = DuplicateSoundCooldown.Value;
                _trackedSoundEvents.RemoveWhere(evnt => evnt.TimeSince > soundCooldown);
            }
        }

        void OnDestroy()
        {
            SoundEnginePatcher.OverridePostEvent -= SoundEnginePatcher_OverridePostEvent;
            _trackedSoundEvents.Clear();
        }

        static bool excludeSound(uint eventID)
        {
            if (SoundEventLibrary.IsInitialized)
            {
                if (ExcludeMithrixPizzaSound.Value && eventID == SoundEventLibrary.Play_moonBrother_blueWall_explode)
                    return true;
            }

            return false;
        }

        static bool tryPlaySound(uint eventID)
        {
            return excludeSound(eventID) || _trackedSoundEvents.Add(new TimeStampedSoundEvent(eventID, Time.unscaledTime));
        }

        static void SoundEnginePatcher_OverridePostEvent(uint eventID, ref bool post)
        {
            if (post && !tryPlaySound(eventID))
            {
                post = false;
            }
        }
    }
}
