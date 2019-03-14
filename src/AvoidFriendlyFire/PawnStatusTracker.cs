using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AvoidFriendlyFire
{
    public class PawnStatusTracker
    {
        private readonly List<PawnStatus> _shooters = new List<PawnStatus>();

        public void AddBlockedShooter(Pawn shooter, Pawn blocker)
        {
            var existingEntry = _shooters.FirstOrDefault(ps => ps.Shooter == shooter);
            if (existingEntry != null)
            {
                existingEntry.Refresh();
                return;
            }
            _shooters.Add(new PawnStatus(shooter, blocker));
        }

        public bool IsAShooter(Pawn pawn)
        {
            return _shooters.Any(ps => ps.Shooter == pawn);
        }

        public bool IsABlocker(Pawn pawn)
        {
            return _shooters.Any(ps => ps.Blocker == pawn);
        }

        public void KillOff(Pawn pawn)
        {
            _shooters.RemoveAll(ps => ps.Shooter == pawn || ps.Blocker == pawn);
        }

        public void Remove(Pawn pawn)
        {
            _shooters.RemoveAll(ps => ps.Shooter == pawn);
        }

        public void RemoveExpired()
        {
            _shooters.RemoveAll(ps => ps.IsExpired());
        }

        public void Reset()
        {
            _shooters.Clear();
        }
    }
}