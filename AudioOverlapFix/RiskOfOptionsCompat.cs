using BepInEx.Bootstrap;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Linq;
using System.Security;
using UnityEngine;

namespace AudioOverlapFix
{
    static class RiskOfOptionsCompat
    {
        public static bool Active => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public static void Init()
        {
            const string GUID = Main.PluginGUID;
            const string NAME = "Audio Overlap Fix";

            ModSettingsManager.SetModDescription($"Mod options for {NAME}", GUID, NAME);

            ModSettingsManager.AddOption(new StepSliderOption(Main.DuplicateSoundCooldown, new StepSliderConfig
            {
                formatString = "{0:F2}s",
                min = 0f,
                max = 2f,
                increment = 0.01f
            }), GUID, NAME);

            ModSettingsManager.AddOption(new CheckBoxOption(Main.ExcludeMithrixPizzaSound, new CheckBoxConfig()), GUID, NAME);

            FileInfo iconFile = findPluginIconFile();
            if (iconFile != null && iconFile.Exists)
            {
                using FileStream fileStream = iconFile.OpenRead();

                byte[] fileContents = new byte[fileStream.Length];
                try
                {
                    fileStream.Read(fileContents, 0, fileContents.Length);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception reading icon file {iconFile.FullName}: {e}");
                    return;
                }

                Texture2D texture = new Texture2D(1, 1);
                if (texture.LoadImage(fileContents))
                {
                    ModSettingsManager.SetModIcon(Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero), GUID, NAME);
                }
            }
        }

        static FileInfo findPluginIconFile()
        {
            FileInfo findPluginFileRecursive(DirectoryInfo directory)
            {
                if (directory is null || !directory.Exists)
                    return null;

                return directory.EnumerateFiles("icon.png", SearchOption.TopDirectoryOnly).FirstOrDefault() ?? findPluginFileRecursive(directory.Parent);
            }

            try
            {
                return findPluginFileRecursive(new DirectoryInfo(Path.GetDirectoryName(Main.Instance.Info.Location)));
            }
            catch (SecurityException e)
            {
                Log.Error($"Unable to find icon file. Encountered Exception: {e}");
                return null;
            }
        }
    }
}
