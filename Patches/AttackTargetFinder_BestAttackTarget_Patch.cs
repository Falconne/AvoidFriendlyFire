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
            var extendedDataStorage = Main.Instance.GetExtendedDataStorage();
            if (!extendedDataStorage.ShouldPawnAvoidFriendlyFire(shooter))
                return true;

            var weaponMissRadius = FireCalculations.GetEquippedWeaponMissRadius(shooter);
            validator = target =>
            {
                var result = Main.Instance.GetFireManager().CanHitTargetSafely(
                    shooter.Position, target.Position, weaponMissRadius);

                if (!result)
                {
                    extendedDataStorage.GetExtendedDataFor(shooter).SetBlocked();
                }

                return result;
            };

            return true;
        }
   }
}