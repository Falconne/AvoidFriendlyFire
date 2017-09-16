using System.Collections.Generic;
using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AvoidFriendlyFire
{
    public class ExtendedDataStorage : UtilityWorldObject, IExposable
    {
        private Dictionary<int, ExtendedPawnData> _store =
            new Dictionary<int, ExtendedPawnData>();

        private List<int> _idWorkingList;

        private List<ExtendedPawnData> _extendedPawnDataWorkingList;


        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Collections.Look(
                ref _store, "store", 
                LookMode.Value, LookMode.Deep, 
                ref _idWorkingList, ref _extendedPawnDataWorkingList);
        }

        // Return the associate extended data for a given Pawn, creating a new association
        // if required.
        public ExtendedPawnData GetExtendedDataFor(Pawn pawn)
        {

            var id = pawn.thingIDNumber;
            if (_store.TryGetValue(id, out ExtendedPawnData data))
            {
                return data;
            }

            var newExtendedData = new ExtendedPawnData();

            _store[id] = newExtendedData;
            return newExtendedData;
        }

        public bool canTrackPawn(Pawn pawn)
        {
            return pawn?.Faction != null && pawn.Faction == Faction.OfPlayer;
        }

        public bool ShouldPawnAvoidFriendlyFire(Pawn pawn)
        {
            if (!canTrackPawn(pawn))
                return false;

            if (!GetExtendedDataFor(pawn).AvoidFriendlyFire)
                return false;

            if (!FireCalculations.HasValidWeapon(pawn))
                return false;

            return true;
        }

        public void DeleteExtendedDataFor(Pawn pawn)
        {
            _store.Remove(pawn.thingIDNumber);
        }
    }
}