using System.Linq;
using RimWorld;
using Verse;

namespace AvoidFriendlyFire
{
    public class ExtendedPawnData : IExposable
    {
        public bool AvoidFriendlyFire = true;

        public void ExposeData()
        {
            Scribe_Values.Look(ref AvoidFriendlyFire, "AvoidFriendlyFire", false);
        }
    }
}
