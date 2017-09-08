using Harmony;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(Verb), "CanHitTarget")]
    public class Verb_CanHitTarget_Patch
    {
        public static void Postfix(ref Verb __instance, ref bool __result, LocalTargetInfo targ)
        {
            if (!Main.Instance.IsModEnabled())
                return;

            if (!__result || !__instance.CasterIsPawn || !targ.IsValid)
                return;

            var pawn = __instance.CasterPawn;
            if (!Main.Instance.GetExtendedDataStorage().IsTrackedPawn(pawn))
                return;

            var extendedData = Main.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            if (!extendedData.AvoidFriendlyFire)
                return;

            if (!FireCalculations.HasValidWeapon(pawn))
                return;

            __result = Main.Instance.GetFireManager().CanHitTargetSafely(pawn, targ.Cell);
        }
    }
}