using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public bool IsTrackedPawn(Pawn pawn)
        {
            if (pawn == null)
                return false;

            return _store.ContainsKey(pawn.thingIDNumber);
        }

        public bool ShouldPawnAvoifFriendlyFire(Pawn pawn)
        {
            if (!IsTrackedPawn(pawn))
                return false;

            if (!GetExtendedDataFor(pawn).AvoidFriendlyFire)
                return false;

            if (!FireCalculations.HasValidWeapon(pawn))
                return false;

            return true;
        }

        // TODO Delete extended data when Pawn is killed
        public void DeleteExtendedDataFor(Pawn pawn)
        {
            // TODO implement
            _store.Remove(pawn.thingIDNumber);
        }
    }
}