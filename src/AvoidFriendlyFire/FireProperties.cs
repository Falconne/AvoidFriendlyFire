using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireProperties
    {
        public Map CurrentMap;
        public IntVec3 Origin;
        public IntVec3 Target;
        public float ForcedMissRadius;

        public FireProperties(Map currentMap, IntVec3 origin, IntVec3 target, float forcedMissRadius)
        {
            Origin = origin;
            Target = target;
            ForcedMissRadius = forcedMissRadius;
            CurrentMap = currentMap;
        }

        public bool ArePointsVisibleAndValid()
        {
            if (Target == Origin)
                return false;

            if (!Target.InBounds(CurrentMap) || Target.Fogged(CurrentMap))
                return false;

            return true;
        }

        public void AdjustForLeaning()
        {
            if (HasClearShotFrom(Origin))
                return;

            // If we got this far the shot is possible, but there is no straight LoS.
            // Must be shooting around a corner, so we need to use a different origin.
            var leaningPositions = new List<IntVec3>();
            ShootLeanUtility.LeanShootingSourcesFromTo(Origin, Target, CurrentMap, leaningPositions);
            foreach (var leaningPosition in leaningPositions)
            {
                if (HasClearShotFrom(leaningPosition))
                {
                    Origin = leaningPosition;
                    return;
                }
            }

        }

        private bool HasClearShotFrom(IntVec3 tryFromOrigin)
        {
            var lineStarted = false;
            foreach (var point in GenSight.PointsOnLineOfSight(tryFromOrigin, Target))
            {
                if (!point.CanBeSeenOver(CurrentMap))
                    return false;

                lineStarted = true;
            }

            return lineStarted;
        }


    }
}