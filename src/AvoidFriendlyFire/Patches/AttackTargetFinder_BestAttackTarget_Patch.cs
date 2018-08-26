using System;
using Harmony;
using Verse;
using Verse.AI;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(AttackTargetFinder), "BestAttackTarget")]
    public class AttackTargetFinder_BestAttackTarget_Patch
    {
        public static bool Prefix(IAttackTargetSearcher searcher, ref Predicate<Thing> validator)
        {
            if (!Main.Instance.IsModEnabled())
                return true;

            if (validator != null)
                return true;

            var shooter = searcher.Thing as Pawn;
            if (!Main.Instance.GetExtendedDataStorage().ShouldPawnAvoidFriendlyFire(shooter))
                return true;

            var weaponMissRadius = FireCalculations.GetEquippedWeaponMissRadius(shooter);
            validator = target => Main.Instance.GetFireManager().CanHitTargetSafely(
                shooter.Position, target.Position, weaponMissRadius);

            return true;
        }
   }
}