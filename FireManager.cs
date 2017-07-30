using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireManager
    {
        bool CanHitTargetSafely(IntVec3 origin, IntVec3 target)
        {
            var map = Find.VisibleMap;
            var originIndex = map.cellIndices.CellToIndex(origin);
            var targetIndex = map.cellIndices.CellToIndex(target);

            if (_cachedFireCones.TryGetValue(originIndex, out var cachedFireConesFromOrigin))
        }

        private readonly Dictionary<int, Dictionary<int, CachedFireCone>> _cachedFireCones
            = new Dictionary<int, Dictionary<int, CachedFireCone>>();
    }
}