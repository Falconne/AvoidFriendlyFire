using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AvoidFriendlyFire
{
    public class Main : HugsLib.ModBase
    {
        public override string ModIdentifier => "AvoidFriendlyFire";

        internal static Main Instance { get; private set; }

        internal new ModLogger Logger => base.Logger;

        private FireConeOverlay _fireConeOverlay;

        private ExtendedDataStorage _extendedDataStorage;

        private FireManager _fireManager;

        private SettingHandle<bool> _showOverlay;

        public Main()
        {
            Instance = this;
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);
            _fireManager?.RemoveExpiredCones(currentTick);
        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            _extendedDataStorage =
                UtilityWorldObjectManager.GetUtilityWorldObject<ExtendedDataStorage>();
        }

        public override void MapLoaded(Map map)
        {
            base.MapLoaded(map);
            _fireManager = new FireManager();
            _fireConeOverlay = new FireConeOverlay();
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            _showOverlay = Settings.GetHandle(
                "showOverlay", "Show targeting overlay",
                "When manually targeting a ranged weapon, highlight all tiles the projectile could pass through, accounting for miss radius.",
                true);
        }

        public void UpdateFireConeOverlay(bool enabled)
        {
            _fireConeOverlay?.Update(_showOverlay && enabled);
        }

        public static Pawn GetSelectedDraftedPawn()
        {
            List<object> selectedObjects = Find.Selector.SelectedObjects;
            if (selectedObjects == null || selectedObjects.Count != 1)
                return null;

            var pawn = selectedObjects.First() as Pawn;
            if (pawn == null || !pawn.Drafted)
                return null;

            return pawn;
        }

        public ExtendedDataStorage GetExtendedDataStorage()
        {
            return _extendedDataStorage;
        }

        public FireManager GetFireManager()
        {
            return _fireManager;
        }
    }
}
