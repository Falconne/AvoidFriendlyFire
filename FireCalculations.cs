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

            if (primaryWeaponVerb.verbProps.forcedMissRadius > 0.5f)
                // Can't handle miniguns and such
                return false;

            if (primaryWeaponVerb.verbProps.projectileDef.projectile.explosionRadius > 0.2f)
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

        public static HashSet<int> GetFireCone(IntVec3 origin, IntVec3 target, float forcedMissRadius)
        {
            var result = new HashSet<int>();

            result.Clear();

            Map map = Find.VisibleMap;

            if (!target.InBounds(map) || target.Fogged(map))
                return null;

            if (target == origin)
                return null;

            if (forcedMissRadius < 0.5f)
            {
                // Create fire cone using target and the 8 cells adjacent to target
                for (var i = 0; i < 8; i++)
                {
                    var splashTarget = origin + GenAdj.AdjacentCells[i];
                    result.UnionWith(GetShootablePointsBetween(origin, splashTarget, map));
                }
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