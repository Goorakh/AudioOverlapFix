using RoR2;

namespace AudioOverlapFix
{
    public static class SoundEventLibrary
    {
        public static bool IsInitialized { get; private set; }

        public static uint Play_moonBrother_blueWall_explode { get; private set; }

        public static void Init()
        {
            if (AkSoundEngine.IsInitialized())
            {
                setupEventIDs();
            }
            else
            {
                void update()
                {
                    if (AkSoundEngine.IsInitialized())
                    {
                        setupEventIDs();
                        RoR2Application.onUpdate -= update;
                    }
                }

                RoR2Application.onUpdate += update;
            }
        }

        static void setupEventIDs()
        {
            IsInitialized = true;

            Play_moonBrother_blueWall_explode = AkSoundEngine.GetIDFromString("Play_moonBrother_blueWall_explode");
        }
    }
}
