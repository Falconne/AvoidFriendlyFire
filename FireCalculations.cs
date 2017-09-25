using System;
using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireCalculations
    {
        public static bool HasValidWeapon(Pawn pawn)
        {
            var primaryWeaponVerb = GetEquippedWeaponVerb(pawn);

            if (primaryWeaponVerb?.verbProps?.projectileDef?.projectile == null)
                return false;

            if (primaryWeaponVerb.verbProps.projectileDef.projectile.explosionRadius > 0.2f)
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

        public static HashSet<int> GetFireCone(IntVec3 origin, IntVec3 target, float forcedMissRadius)
        {
            var result = new HashSet<int>();
            result.Clear();

            var map = Find.VisibleMap;

            if (!target.InBounds(map) || target.Fogged(map))
                return null;

            if (target == origin)
                return null;

            // Forced miss radius is reduced at close range
            var adjustedMissRadius = forcedMissRadius;
            if (forcedMissRadius > 0.5f)
            {
                var distanceToTarget = (target - origin).LengthHorizontalSquared;
                if (distanceToTarget < 9f)
                {
                    adjustedMissRadius = 0f;
                }
                else if (distanceToTarget < 25f)
                {
                    adjustedMissRadius *= 0.5f;
                }
                else if (distanceToTarget < 49f)
                {
                    adjustedMissRadius *= 0.8f;
                }
            }

            result.UnionWith(GetShootablePointsBetween(origin, target, map));
            if (result.Count == 0)
            {
                // If we got here the shot is possible, but there is no straight LoS.
                // Must be shooting around a corner, we need to use a different origin.
                var leaningPositions = new List<IntVec3>();
                ShootLeanUtility.LeanShootingSourcesFromTo(origin, target, map, leaningPositions);
                foreach (var leaningPosition in leaningPositions)
                {
                    result.UnionWith(GetShootablePointsBetween(leaningPosition, target, map));
                    if (result.Count != 0)
                    {
                        origin = leaningPosition;
                        break;
                    }
                }
            }

            if (result.Count == 0)
            {
                // Something has gone wrong
                return null;
            }

            // Create fire cone using target and the 8 cells adjacent to target
            var adjustmentVector = GenAdj.AdjacentCells;
            var adjustmentCount = 8;

            if (adjustedMissRadius > 0.5f)
            {
                // Create fire cone using full miss radius
                adjustmentVector = GenRadial.RadialPattern;
                adjustmentCount = GenRadial.NumCellsInRadius(forcedMissRadius);
            }

            for (var i = 0; i < adjustmentCount; i++)
            {
                var splashTarget = target + adjustmentVector[i];
                if (splashTarget == target)
                    continue;  // Already done direct line to target
                result.UnionWith(GetShootablePointsBetween(origin, splashTarget, map));
            }


            return result;
        }

        private static Verb GetEquippedWeaponVerb(Pawn pawn)
        {
            return pawn.equipment?.PrimaryEq?.PrimaryVerb;
        }

        private static IEnumerable<int> GetShootablePointsBetween(
            IntVec3 origin, IntVec3 target, Map map)
        {
            foreach (IntVec3 point in GenSight.PointsOnLineOfSight(origin, target))
            {
                if (!point.CanBeSeenOver(map))
                    yield break;

                // Nearby pawns do not receive friendly fire
                var checkedCellToOriginDistance = point - origin;
                {
                    var xDiff = Math.Abs(checkedCellToOriginDistance.x);
                    var zDiff = Math.Abs(checkedCellToOriginDistance.z);
                    if ((xDiff == 0 && zDiff < 5) || (zDiff == 0 && xDiff < 5))
                        continue;

                    if (xDiff > 0 && zDiff > 0 && xDiff + zDiff < 6)
                        continue;
                }

                yield return map.cellIndices.CellToIndex(point.x, point.z);
            }

            yield return map.cellIndices.CellToIndex(target.x, target.z);
        }
    }
}