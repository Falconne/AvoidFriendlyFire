using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public class CachedFireCone
    {
        public CachedFireCone(HashSet<int> fireCone)
        {
            FireCone = fireCone;
            _expireAt = Find.TickManager.TicksGame + 1000;
        }

        public bool IsExpired()
        {
            return Find.TickManager.TicksGame >= _expireAt;
        }

        public readonly HashSet<int> FireCone;

        private readonly int _expireAt;
    }
}