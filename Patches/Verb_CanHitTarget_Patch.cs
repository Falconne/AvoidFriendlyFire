using Harmony;
using Verse;

namespace AvoidFriendlyFire.Patches
{
    [HarmonyPatch(typeof(Verb), "CanHitTarget")]
    public class Verb_CanHitTarget_Patch
    {
        public static void Postfix(ref Verb __instance, bool __result, LocalTargetInfo targ)
        {
            if (!__result || !__instance.CasterIsPawn || !targ.IsValid)
                return;

            var pawn = __instance.CasterPawn;
            if (!Main.Instance.GetExtendedDataStorage().IsTrackedPawn(pawn))
                return;

            var extendedData = Main.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            if (!extendedData.AvoidFriendlyFire)
                return;

            var primaryWeaponVerb = pawn.equipment?.PrimaryEq?.PrimaryVerb as Verb_LaunchProjectile;
            if (primaryWeaponVerb == null)
                return;

            if (primaryWeaponVerb.verbProps.forcedMissRadius > 0.5f)
                // Can't handle miniguns and such
                return;

            // TODO check if projectile is flyOverhead
            //if (!primaryWeaponVerb.canFreeInterceptNow)

            __result = Main.Instance.GetFireManager().CanHitTargetSafely(pawn, targ.Cell);
        }
    }
}