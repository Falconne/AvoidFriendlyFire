using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireCalculations
    {
        public static HashSet<int> GetFireCone(IntVec3 origin, IntVec3 target)
        {
            var result = new HashSet<int>();
            
            result.Clear();

            Map map = Find.VisibleMap;

            if (!target.InBounds(map) || target.Fogged(map))
                return null;

            if (target == origin)
                return null;

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
            IntVec3 source, IntVec3 target, Map map)
        {
            foreach (IntVec3 point in GenSight.PointsOnLineOfSight(source, target))
            {
                if (!point.CanBeSeenOver(map))
                    yield break;

                yield return map.cellIndices.CellToIndex(point.x, point.z);
            }

            yield return map.cellIndices.CellToIndex(target.x, target.z);
        }
    }
}