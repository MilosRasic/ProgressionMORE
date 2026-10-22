using System.Reflection;
using HarmonyLib;
using PsycastSynergies;
using UnityEngine;

namespace ProgressionMORE
{
    /// <summary>
    /// RimWorld 1.7 accepts a GUI button click only when the rectangle on mouse-up
    /// is the same rectangle stored on mouse-down. Psycasts² wobbles every node each
    /// frame, so the click never lands. Draw and hit-test the un-wobbled position.
    /// </summary>
    [HarmonyPatch(typeof(Window_Specializations), "NodeCenter")]
    public static class Patch_StableSpecNodes
    {
        static readonly MethodInfo RawCenter = AccessTools.Method(typeof(Window_Specializations), "RawCenter");

        static bool Prefix(Window_Specializations __instance, Spec sp, ref Vector2 __result)
        {
            if (RawCenter == null)
            {
                return true;
            }

            __result = (Vector2)RawCenter.Invoke(__instance, new object[] { sp });
            return false;
        }
    }
}
