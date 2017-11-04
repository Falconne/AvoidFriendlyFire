using System.Linq;
using RimWorld;
using Verse;

namespace AvoidFriendlyFire
{
    public class ExtendedPawnData : IExposable
    {
        public bool AvoidFriendlyFire = true;

        //private bool _isBlocked = false;
        private int _showBlockedStatusUntil;

        public bool IsBlocked()
        {
            if (_showBlockedStatusUntil == 0)
                return false;

            if (Find.TickManager.TicksGame >= _showBlockedStatusUntil)
            {
                _showBlockedStatusUntil = 0;
                return false;
            }

            return true;
        }

        public void SetBlocked()
        {
            _showBlockedStatusUntil = Find.TickManager.TicksGame + 200;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref AvoidFriendlyFire, "AvoidFriendlyFire", false);
        }
    }
}
