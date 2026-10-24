using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace ProgressionMORE
{
    /// <summary>
    /// Character Development only completes a ritual want when the outcome is positive.
    /// Anima tree linking and Gauranlen connection use a quality worker with no outcome
    /// tiers, so that check gets no outcome and drops the want. A finished ritual with
    /// no outcome tiers counts as a success, and the original check is skipped so it
    /// does not roll an outcome that does not exist.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_RitualWantWithoutOutcome
    {
        static bool Prepare()
        {
            bool found = TargetMethod() != null;

            return found;
        }

        static MethodBase TargetMethod()
        {
            var patch = AccessTools.TypeByName("WantsAndQuirks.LordJob_Ritual_ApplyOutcome_Patch");
            if (patch == null)
            {
                return null;
            }

            return AccessTools.DeclaredMethod(patch, "RitualOutcomePositive");
        }

        static bool Prefix(LordJob_Ritual ritual, ref bool __result)
        {
            if (ritual?.Ritual?.outcomeEffect is RitualOutcomeEffectWorker_FromQuality fromQuality
                && fromQuality.def.outcomeChances.NullOrEmpty())
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
