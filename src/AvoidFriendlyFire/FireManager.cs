using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireManager
    {
        private readonly Dictionary<int, Dictionary<int, CachedFireCone>> _cachedFireCones
            = new Dictionary<int, Dictionary<int, CachedFireCone>>();

        private int _lastCleanupTick;

        public bool CanHitTargetSafely(FireProperties fireProperties)
        {
            HashSet<int> fireCone = GetOrCreatedCachedFireConeFor(fireProperties);
            if (fireCone == null)
                return true;

            var map = fireProperties.CasterMap;
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
                if (pawnCell == fireProperties.Origin || pawnCell == fireProperties.Target)
                    continue;

                var pawnIndex = map.cellIndices.CellToIndex(pawnCell);
                if (!fireCone.Contains(pawnIndex))
                    continue;

                if (IsPawnWearingUsefulShield(pawn))
                    continue;

                return false;
            }

            return true;
        }

        private bool IsPawnWearingUsefulShield(Pawn pawn)
        {
            if (!Main.Instance.ShouldIgnoreShieldedPawns())
                return false;

            if (pawn.apparel == null)
                return false;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                var shield = apparel as ShieldBelt;
                if (shield == null)
                    continue;

                if (shield.ShieldState != ShieldState.Active)
                    return false;

                var energyMax = shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
                return shield.Energy / energyMax > 0.1f;
            }

            return false;
        }

        private static bool ShouldProtectAnimal(Pawn animal)
        {
            if (animal.Faction != Faction.OfPlayer)
                return false;

            if (Main.Instance.ShouldProtectAllColonyAnimals())
                return true;

            if (!Main.Instance.ShouldProtectPets())
                return false;

            return animal.playerSettings?.Master != null;
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

        private HashSet<int> GetOrCreatedCachedFireConeFor(FireProperties fireProperties)
        {
            var originIndex = fireProperties.OriginIndex;
            var targetIndex = fireProperties.TargetIndex;

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

            // No cached firecone, create one
            var newFireCone = new CachedFireCone(FireCalculations.GetFireCone(fireProperties));
            if (!_cachedFireCones.ContainsKey(originIndex))
                _cachedFireCones.Add(originIndex, new Dictionary<int, CachedFireCone>());

            _cachedFireCones[originIndex][targetIndex] = newFireCone;

            return newFireCone.FireCone;
        }
    }
}