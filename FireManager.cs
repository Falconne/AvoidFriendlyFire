using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireManager
    {
        bool CanHitTargetSafely(Pawn source, Pawn target)
        {
            
        }

        private readonly Dictionary<int, Dictionary<int, CachedFireCone>> _cachedFireCones
            = new Dictionary<int, Dictionary<int, CachedFireCone>>();
    }
}