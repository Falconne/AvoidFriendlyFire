using System.Collections.Generic;
using System.Net;
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

        public float GetAimOnTargetChance(Thing caster, Verb weaponVerb)
        {
            var distance = (Target - Origin).LengthHorizontal;

            var factorFromShooterAndDist = ShotReport.HitFactorFromShooter(caster, distance);

            var factorFromEquipment = weaponVerb.verbProps.GetHitChanceFactor(
                weaponVerb.EquipmentSource, distance);

            var factorFromWeather = 1f;
            if (!caster.Position.Roofed(CurrentMap) || !Target.Roofed(CurrentMap))
            {
                factorFromWeather = caster.Map.weatherManager.CurWeatherAccuracyMultiplier;
            }

            ThingDef coveringGas = null;
            foreach (var point in GenSight.PointsOnLineOfSight(Origin, Target))
            {
                if (!point.CanBeSeenOver(CurrentMap))
                    break;

                Thing gas = point.GetGas(CurrentMap);
                if (IsThisGasMorePenalisingThan(gas, coveringGas))
                {
                    coveringGas = gas.def;
                }
            }

            var factorFromCoveringGas = 1f - (coveringGas?.gas.accuracyPenalty ?? 0f);

            var result = factorFromShooterAndDist * factorFromEquipment * factorFromWeather *
                         factorFromCoveringGas;
            if (result < 0.0201f)
                result = 0.0201f;

            return result;
        }

        private static bool IsThisGasMorePenalisingThan(Thing gas, ThingDef otherGas)
        {
            if (gas == null)
                return false;

            if (otherGas == null)
                return true;

            return otherGas.gas.accuracyPenalty < gas.def.gas.accuracyPenalty;
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