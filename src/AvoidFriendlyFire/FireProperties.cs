using System.Collections.Generic;
using Verse;

namespace AvoidFriendlyFire
{
    public struct MissAreaDescriptor
    {
        public IntVec3[] AdjustmentVector;
        public int AdjustmentCount;

        public MissAreaDescriptor(IntVec3[] adjustmentVector, int adjustmentCount)
        {
            AdjustmentVector = adjustmentVector;
            AdjustmentCount = adjustmentCount;
        }
    }


    public class FireProperties
    {
        public IntVec3 Target;

        public Map CasterMap => Caster.Map;

        public IntVec3 Origin;

        public float ForcedMissRadius => _weaponVerb.verbProps.ForcedMissRadius;

        public int OriginIndex => CasterMap.cellIndices.CellToIndex(Origin);

        public int TargetIndex => CasterMap.cellIndices.CellToIndex(Target);

        public Pawn Caster { get; }

        private readonly Verb _weaponVerb;

        public FireProperties(Pawn caster, IntVec3 target)
        {
            Target = target;
            Caster = caster;
            _weaponVerb = GetEquippedWeaponVerb(caster);
            Origin = Caster.Position;
        }

        public bool ArePointsVisibleAndValid()
        {
            if (Target == Origin)
                return false;

            if (!Target.InBounds(CasterMap) || Target.Fogged(CasterMap))
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
            ShootLeanUtility.LeanShootingSourcesFromTo(Origin, Target, CasterMap, leaningPositions);
            foreach (var leaningPosition in leaningPositions)
            {
                if (HasClearShotFrom(leaningPosition))
                {
                    Origin = leaningPosition;
                    return;
                }
            }

        }

        public float GetAimOnTargetChance()
        {
            var distance = (Target - Origin).LengthHorizontal;

            var factorFromShooterAndDist = ShotReport.HitFactorFromShooter(Caster, distance);

            var factorFromEquipment = _weaponVerb.verbProps.GetHitChanceFactor(
                _weaponVerb.EquipmentSource, distance);

            var factorFromWeather = 1f;
            if (!Caster.Position.Roofed(CasterMap) || !Target.Roofed(CasterMap))
            {
                factorFromWeather = CasterMap.weatherManager.CurWeatherAccuracyMultiplier;
            }

            ThingDef coveringGas = null;
            foreach (var point in GenSight.PointsOnLineOfSight(Origin, Target))
            {
                if (!point.CanBeSeenOver(CasterMap))
                    break;

                Thing gas = point.GetGas(CasterMap);
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
                if (!point.CanBeSeenOver(CasterMap))
                    return false;

                lineStarted = true;
            }

            return lineStarted;
        }


        public MissAreaDescriptor GetMissAreaDescriptor()
        {
            var adjustedMissRadius = CalculateAdjustedForcedMiss();

            if (adjustedMissRadius > 0.5f)
            {
                // Create fire cone using weapon miss radius
                return new MissAreaDescriptor(
                    GenRadial.RadialPattern,
                    GenRadial.NumCellsInRadius(ForcedMissRadius));
            }

            if (!Main.Instance.ShouldEnableAccurateMissRadius())
            {
                // Create fire cone using target and the 8 cells adjacent to target
                return new MissAreaDescriptor(GenAdj.AdjacentCells, 8);
            }

            var missRadius = ShootTuning.MissDistanceFromAimOnChanceCurves.Evaluate(
                GetAimOnTargetChance(), 1f);

            if (missRadius < 0)
                return new MissAreaDescriptor(GenAdj.AdjacentCells, 8);

            return new MissAreaDescriptor(
                GenRadial.RadialPattern,
                GenRadial.NumCellsInRadius(missRadius));

        }

        private float CalculateAdjustedForcedMiss()
        {
            return ForcedMissRadius <= 0.5f
                ? 0f
                : VerbUtility.CalculateAdjustedForcedMiss(ForcedMissRadius, Target - Origin);
        }

        public static Verb GetEquippedWeaponVerb(Pawn pawn)
        {
            return pawn.equipment?.PrimaryEq?.PrimaryVerb;
        }
    }
}