using Harmony;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(Verb), "CanHitTargetFrom")]
    public class Verb_CanHitTargetFrom_Patch
    {
        public static void Postfix(ref Verb __instance, ref bool __result, IntVec3 root, 
            LocalTargetInfo targ)
        {
            if (!Main.Instance.IsModEnabled())
                return;

            if (!__result || !__instance.CasterIsPawn || !targ.IsValid)
                return;

            if (targ.Thing != null && targ.Thing == __instance.caster)
                return;

            if (root.DistanceTo(targ.Cell) <= 2f)
                return;

            var pawn = __instance.CasterPawn;
            if (!Main.Instance.GetExtendedDataStorage().ShouldPawnAvoidFriendlyFire(pawn))
                return;

            var weaponMissRadius = FireCalculations.GetEquippedWeaponMissRadius(pawn);
            __result = Main.Instance.GetFireManager().CanHitTargetSafely(
                root, targ.Cell, weaponMissRadius);
        }
    }
}