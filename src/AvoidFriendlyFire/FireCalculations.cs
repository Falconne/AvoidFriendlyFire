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

        public static float GetEquippedWeaponMissRadius(Pawn pawn)
        {
            var primaryWeaponVerb = GetEquippedWeaponVerb(pawn);

            return primaryWeaponVerb.verbProps.forcedMissRadius;
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

            // Create fire cone using target and the 8 cells adjacent to target
            var adjustmentVector = GenAdj.AdjacentCells;
            var adjustmentCount = 8;

            var adjustedMissRadius = CalculateAdjustedForcedMiss(
                fireProperties.ForcedMissRadius, fireProperties.Target - fireProperties.Origin);

            if (adjustedMissRadius > 0.5f)
            {
                // Create fire cone using full miss radius
                adjustmentVector = GenRadial.RadialPattern;
                adjustmentCount = GenRadial.NumCellsInRadius(fireProperties.ForcedMissRadius);
            }

            var result = new HashSet<int>();
            result.Clear();
            var map = Find.CurrentMap;
            for (var i = 0; i < adjustmentCount; i++)
            {
                var splashTarget = fireProperties.Target + adjustmentVector[i];
                result.UnionWith(GetShootablePointsBetween(fireProperties.Origin, splashTarget, map));
            }


            return result;
        }

        private static float CalculateAdjustedForcedMiss(float forcedMiss, IntVec3 vector)
        {
            return forcedMiss <= 0.5f
                ? 0f
                : VerbUtility.CalculateAdjustedForcedMiss(forcedMiss, vector);
        }


        private static Verb GetEquippedWeaponVerb(Pawn pawn)
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