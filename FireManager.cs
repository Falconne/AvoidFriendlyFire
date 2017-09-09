using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AvoidFriendlyFire
{
    // TODO clear expired cones

    public class FireManager
    {
        private readonly Dictionary<int, Dictionary<int, CachedFireCone>> _cachedFireCones
            = new Dictionary<int, Dictionary<int, CachedFireCone>>();

        private int _lastCleanupTick;

        public bool CanHitTargetSafely(IntVec3 origin, IntVec3 target)
        {
            HashSet<int> fireCone = GetOrCreatedCachedFireConeFor(origin, target);
            if (fireCone == null)
                return false;

            var map = Find.VisibleMap;
            foreach (var pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
            {
                if (pawn.Dead)
                    continue;

                if (!pawn.RaceProps.Humanlike)
                {
                    // Only consider animals assigned to a master for now
                    if (pawn.playerSettings.master == null)
                        continue;
                }

                var pawnCell = pawn.Position;
                if (pawnCell == target)
                    // Do not allow targeting friendlies
                    return false;

                if (pawnCell == origin)
                    continue;

                var pawnIndex = map.cellIndices.CellToIndex(pawnCell);
                if (fireCone.Contains(pawnIndex))
                    return false;
            }

            return true;
        }

        public void RemoveExpiredCones(int currentTick)
        {
            if (currentTick - _lastCleanupTick < 1000)
                return;

            _lastCleanupTick = currentTick;

            var origins = _cachedFireCones.Keys.ToList();
            foreach (var origin in origins)
            {
                var cachedFireConesFromOneOrigin = _cachedFireCones[origin];
                var targets = cachedFireConesFromOneOrigin.Keys.ToList();
                foreach (var target in targets)
                {
                    var cachedFireCone = cachedFireConesFromOneOrigin[target];
                    if (cachedFireCone.IsExpired())
                    {
                        cachedFireConesFromOneOrigin.Remove(target);
                    }
                }

                if (cachedFireConesFromOneOrigin.Count == 0)
                {
                    _cachedFireCones.Remove(origin);
                }
            }
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
    }
}