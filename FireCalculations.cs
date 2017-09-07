using System;
using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireCalculations
    {
        public static bool HasValidWeapon(Pawn pawn)
        {
            var primaryWeaponVerb = pawn.equipment?.PrimaryEq?.PrimaryVerb as Verb_LaunchProjectile;
            if (primaryWeaponVerb == null)
                return false;

            if (primaryWeaponVerb.verbProps.forcedMissRadius > 0.5f)
                // Can't handle miniguns and such
                return false;

            if (primaryWeaponVerb.HighlightFieldRadiusAroundTarget() > 0.2f)
                // Can't handle explosive projectiles yet
                return false;

            // TODO check if projectile is flyOverhead
            //if (!primaryWeaponVerb.canFreeInterceptNow)

            return true;
        }

        public static HashSet<int> GetFireCone(IntVec3 origin, IntVec3 target)
        {
            var result = new HashSet<int>();
            
            result.Clear();

            Map map = Find.VisibleMap;

            if (!target.InBounds(map) || target.Fogged(map))
                return null;

            if (target == origin)
                return null;

            // Create fire cone using target and the 8 cells adjacent to target
            for (var xSplash = target.x - 1; xSplash <= target.x + 1; xSplash++)
            {
                for (var zSplash = target.z - 1; zSplash <= target.z + 1; zSplash++)
                {
                    var splashTarget = new IntVec3(xSplash, target.y, zSplash);
                    result.UnionWith(GetShootablePointsBetween(origin, splashTarget, map));
                }
            }

            return result;
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