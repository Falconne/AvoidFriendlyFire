using Harmony;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(TooltipUtility), "ShotCalculationTipString")]
    public class TooltipUtility_ShotCalculationTipString_Patch
    {
        public static bool Prefix(ref Thing target)
        {
            if (Find.Selector.SingleSelectedThing is Pawn pawn)
            {
                if (pawn != target && pawn.equipment?.Primary != null
                    && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_LaunchProjectile)
                {
                    Main.Instance.GetFireManager().SkipNextCheck = true;
                }
            }

            return true;
        }
    }
}