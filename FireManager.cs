using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireManager
    {
        private readonly Dictionary<int, Dictionary<int, CachedFireCone>> _cachedFireCones
            = new Dictionary<int, Dictionary<int, CachedFireCone>>();

        private int _lastCleanupTick;

        public bool CanHitTargetSafely(IntVec3 origin, IntVec3 target)
        {
            HashSet<int> fireCone = GetOrCreatedCachedFireConeFor(origin, target);
            if (fireCone == null)
                return true;

            var map = Find.VisibleMap;
            foreach (var pawn in map.mapPawns.AllPawns)
            {
                if (pawn?.RaceProps == null || pawn.Dead)
                    continue;

                if (pawn.Faction == null)
                    continue;

                if (pawn.RaceProps.Humanlike)
                {
                    if (pawn.IsPrisoner)
                        continue;

                    if (pawn.HostileTo(Faction.OfPlayer))
                        continue;
                }
                else if (!ShouldProtectAnimal(pawn))
                {
                    continue;
                }

                var pawnCell = pawn.Position;
                if (pawnCell == origin || pawnCell == target)
                    continue;

                var pawnIndex = map.cellIndices.CellToIndex(pawnCell);
                if (fireCone.Contains(pawnIndex))
                    return false;
            }

            return true;
        }

        private bool ShouldProtectAnimal(Pawn animal)
        {
            if (animal.Faction != Faction.OfPlayer)
                return false;

            if (Main.Instance.ShouldProtectAllColonyAnimals())
                return true;

            if (animal.playerSettings?.master != null)
                return true;

            return false;
        }

        public void RemoveExpiredCones(int currentTick)
        {
            if (currentTick - _lastCleanupTick < 400)
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