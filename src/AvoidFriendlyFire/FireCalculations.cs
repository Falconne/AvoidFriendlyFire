using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireCalculations
    {
        public static bool HasValidWeapon(Pawn pawn)
        {
            var primaryWeaponVerb = GetEquippedWeaponVerb(pawn);

            if (primaryWeaponVerb?.verbProps?.defaultProjectile?.projectile == null)
                return false;

            if (primaryWeaponVerb.verbProps.defaultProjectile.projectile.explosionRadius > 0.2f)
                // Can't handle explosive projectiles yet
                return false;

            // TODO check if projectile is flyOverhead

            return true;
        }

        public static float GetEquippedWeaponRange(Pawn pawn)
        {
            var primaryWeaponVerb = GetEquippedWeaponVerb(pawn);
            return primaryWeaponVerb?.verbProps.range ?? 0;
        }

        public static HashSet<int> GetFireCone(FireProperties fireProperties)
        {
            if (!fireProperties.ArePointsVisibleAndValid())
                return null;

            fireProperties.AdjustForLeaning();

            var missAreaDescriptor = fireProperties.GetMissAreaDescriptor();

            var result = new HashSet<int>();
            result.Clear();
            var map = Find.CurrentMap;
            for (var i = 0; i < missAreaDescriptor.AdjustmentCount; i++)
            {
                var splashTarget = fireProperties.Target + missAreaDescriptor.AdjustmentVector[i];
                result.UnionWith(GetShootablePointsBetween(fireProperties.Origin, splashTarget, map));
            }


            return result;
        }

        public static Verb GetEquippedWeaponVerb(Pawn pawn)
        {
            return pawn.equipment?.PrimaryEq?.PrimaryVerb;
        }


        private static IEnumerable<int> GetShootablePointsBetween(
            IntVec3 origin, IntVec3 target, Map map)
        {
            foreach (var point in GenSight.PointsOnLineOfSight(origin, target))
            {
                if (!point.CanBeSeenOver(map))
                    yield break;

                // Nearby pawns do not receive friendly fire
                if (IsInCloseRange(origin, point))
                {
                    continue;
                }

                yield return map.cellIndices.CellToIndex(point.x, point.z);
            }

            if (!IsAdjacent(origin, target))
                yield return map.cellIndices.CellToIndex(target.x, target.z);
        }

        private static bool IsInCloseRange(IntVec3 origin, IntVec3 point)
        {
            var checkedCellToOriginDistance = point - origin;
            var xDiff = Math.Abs(checkedCellToOriginDistance.x);
            var zDiff = Math.Abs(checkedCellToOriginDistance.z);
            if ((xDiff == 0 && zDiff < 5) || (zDiff == 0 && xDiff < 5))
                return true;

            if (xDiff > 0 && zDiff > 0 && xDiff + zDiff < 6)
                return true;

            return false;
        }

        private static bool IsAdjacent(IntVec3 origin, IntVec3 point)
        {
            IntVec3[] adjustmentVector = GenAdj.AdjacentCells;
            const int adjustmentCount = 8;
            for (var i = 0; i < adjustmentCount; i++)
            {
                var adjacentPoint = origin + adjustmentVector[i];
                if (point == adjacentPoint)
                    return true;
            }

            return false;
        }
    }
}