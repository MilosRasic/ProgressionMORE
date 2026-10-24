using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace ProgressionMORE
{
    /// <summary>
    /// Make a dessert takes the first thing on the tile and requires an exact FoodMeals
    /// category. Meal subcategories such as Davai's basic meals fail that check.
    /// Use the first thing on the tile that is within FoodMeals, including child categories.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_MakeADessert
    {
        static bool Prepare()
        {
            bool found = AccessTools.TypeByName("VanillaMemesExpanded.CompAbilityMakeADessert") != null;

            return found;
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.DeclaredMethod(
                AccessTools.TypeByName("VanillaMemesExpanded.CompAbilityMakeADessert"),
                "Apply");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo firstMeal = AccessTools.Method(typeof(Patch_MakeADessert), nameof(FirstMealOnTile));
            MethodInfo isMeal = AccessTools.Method(typeof(Patch_MakeADessert), nameof(IsWithinMealCategory));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand is MethodInfo method)
                {
                    if (method.DeclaringType == typeof(GenCollection) && method.Name == "FirstOrFallback")
                    {
                        instruction.operand = firstMeal;
                    }
                    else if (method.DeclaringType == typeof(ThingUtility) && method.Name == "HasThingCategory")
                    {
                        instruction.operand = isMeal;
                    }
                }

                yield return instruction;
            }
        }

        public static Thing FirstMealOnTile(IEnumerable<Thing> things, Thing fallback)
        {
            ThingCategoryDef meals = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("FoodMeals");
            if (meals == null || things == null)
            {
                return null;
            }

            foreach (Thing thing in things)
            {
                if (thing?.def != null && thing.def.IsWithinCategory(meals))
                {
                    return thing;
                }
            }

            return null;
        }

        public static bool IsWithinMealCategory(Thing thing, ThingCategoryDef category)
        {
            return thing?.def != null && category != null && thing.def.IsWithinCategory(category);
        }
    }
}
