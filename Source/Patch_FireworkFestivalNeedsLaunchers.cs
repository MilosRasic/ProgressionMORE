using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace ProgressionMORE
{
    /// <summary>
    /// The firework festival gizmo only checks that an operator exists. Disable it until each
    /// operator the ritual would assign has a launcher they carry or can walk to. Ground,
    /// stockpiles, and placed storage all count. Forbidden and unreachable launchers do not.
    /// </summary>
    [HarmonyPatch(typeof(RitualBehaviorWorker), nameof(RitualBehaviorWorker.CanStartRitualNow))]
    public static class Patch_FireworkFestivalNeedsLaunchers
    {
        const string BehaviorDefName = "FireworkCelebration_Behavior";
        const string RoleId = "operator";
        const string LauncherDefName = "FireworkLauncher";

        static bool Prepare()
        {
            bool found = DefDatabase<RitualBehaviorDef>.GetNamedSilentFail(BehaviorDefName) != null;

            return found;
        }

        static void Postfix(TargetInfo target, Precept_Ritual ritual, ref string __result)
        {
            if (!__result.NullOrEmpty() || ritual?.behavior?.def?.defName != BehaviorDefName || !target.IsValid)
            {
                return;
            }

            ThingDef launcherDef = DefDatabase<ThingDef>.GetNamedSilentFail(LauncherDefName);
            RitualRole role = ritual.behavior.def.roles?.FirstOrDefault(candidate => candidate.id == RoleId);
            if (launcherDef == null || role == null)
            {
                return;
            }

            List<Pawn> operators = target.Map.mapPawns.FreeColonistsSpawned
                .Where(pawn => role.AppliesToPawn(pawn, out _, target, null, null, ritual, true))
                .ToList();
            role.OrderByDesirability(operators);

            int limit = role.maxCount > 0 ? role.maxCount : operators.Count;
            if (operators.Count > limit)
            {
                operators.RemoveRange(limit, operators.Count - limit);
            }

            if (operators.Count == 0)
            {
                return;
            }

            int supplied = LaunchersSupplied(target.Map, launcherDef, operators);
            if (supplied < operators.Count)
            {
                __result = "RitualTargetFireworkLauncherInfo".Translate(1, supplied, operators.Count);
            }
        }

        static int LaunchersSupplied(Map map, ThingDef launcherDef, List<Pawn> operators)
        {
            List<Thing> launchers = map.listerThings.ThingsOfDef(launcherDef);
            Dictionary<Thing, int> claimed = new Dictionary<Thing, int>();
            int supplied = 0;

            foreach (Pawn pawn in operators)
            {
                if (pawn.inventory.Count(launcherDef) >= 1)
                {
                    supplied++;
                    continue;
                }

                for (int i = 0; i < launchers.Count; i++)
                {
                    Thing launcher = launchers[i];
                    if (launcher.IsForbidden(pawn) || !pawn.CanReserveAndReach(launcher, PathEndMode.Touch, pawn.NormalMaxDanger()))
                    {
                        continue;
                    }

                    int used = 0;
                    claimed.TryGetValue(launcher, out used);
                    if (launcher.stackCount - used < 1)
                    {
                        continue;
                    }

                    claimed[launcher] = used + 1;
                    supplied++;
                    break;
                }
            }

            return supplied;
        }
    }
}
