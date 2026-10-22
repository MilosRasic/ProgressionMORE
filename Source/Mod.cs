using HarmonyLib;

namespace ProgressionMORE
{
    [StaticConstructorOnStartup]
    public static class Mod
    {
        static Mod()
        {
            new Harmony("ProgressionMORE").PatchAll();
            Log.Message("[ProgressionMORE] Specialization nodes use a stable hitbox.");
        }
    }
}
