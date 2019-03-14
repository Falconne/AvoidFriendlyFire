using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public class PawnStatus
    {
        public readonly Pawn Shooter;

        public readonly Pawn Blocker;

        private int _expiryTime;

        public PawnStatus(Pawn shooter, Pawn blocker)
        {
            Shooter = shooter;
            Blocker = blocker;
            Refresh();
        }

        public void Refresh()
        {
            _expiryTime = Find.TickManager.TicksGame + 20;
        }

        public bool IsExpired()
        {
            return Find.TickManager.TicksGame >= _expiryTime;
        }
    }
}