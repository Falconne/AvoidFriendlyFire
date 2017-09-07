using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    // TODO clear expired cones

    public class FireManager
    {
        public bool CanHitTargetSafely(Pawn shooterPawn, IntVec3 target)
        {
            var origin = shooterPawn.Position;
            HashSet<int> fireCone = GetOrCreatedCachedFireConeFor(origin, target);
            if (fireCone == null)
                return false;

            var map = Find.VisibleMap;
            foreach (var pawn in map.mapPawns.FreeColonists)
            {
                if (pawn.Dead)
                    continue;

                if (pawn == shooterPawn)
                    continue;

                var pawnCell = pawn.Position;
                if (pawnCell == origin || pawnCell == target)
                    continue;

                var pawnIndex = map.cellIndices.CellToIndex(pawnCell);
                if (fireCone.Contains(pawnIndex))
                    return false;
            }

            return true;
        }

        private HashSet<int> GetOrCreatedCachedFireConeFor(IntVec3 origin, IntVec3 target)
        {
            var map = Find.VisibleMap;
            var originIndex = map.cellIndices.CellToIndex(origin);
            var targetIndex = map.cellIndices.CellToIndex(target);

            if (_cachedFireCones.TryGetValue(originIndex, out var cachedFireConesFromOrigin))
            {
                if (cachedFireConesFromOrigin.TryGetValue(targetIndex, out var cachedFireCone))
                {
                    if (!cachedFireCone.IsExpired())
                    {
                        cachedFireCone.Prolong();
                        return cachedFireCone.FireCone;
                    }
                }
            }

            var newFireCone = new CachedFireCone(FireCalculations.GetFireCone(origin, target));
            if (!_cachedFireCones.ContainsKey(originIndex))
                _cachedFireCones.Add(originIndex, new Dictionary<int, CachedFireCone>());
            _cachedFireCones[originIndex][targetIndex] = newFireCone;


            return newFireCone.FireCone;
        }

        private readonly Dictionary<int, Dictionary<int, CachedFireCone>> _cachedFireCones
            = new Dictionary<int, Dictionary<int, CachedFireCone>>();
    }
}