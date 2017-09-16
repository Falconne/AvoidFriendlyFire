using System.Collections.Generic;
using System.Linq;
using Harmony;
using HugsLib.Settings;
using HugsLib.Utils;
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

        private SettingHandle<bool> _protectColonyAnimals;

        private SettingHandle<bool> _modEnabled;

        public Main()
        {
            Instance = this;
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);
            if (!IsModEnabled())
                return;
            _fireManager?.RemoveExpiredCones(currentTick);
        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            _extendedDataStorage =
                UtilityWorldObjectManager.GetUtilityWorldObject<ExtendedDataStorage>();

            // Ticks appear to be run before MapLoaded, so we need a FireManager available
            _fireManager = new FireManager();
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

            _modEnabled = Settings.GetHandle(
                "enabled", "Enable Mod",
                "If mod is causing problems, please log a bug and disable from here till an update is available, so as to preserve yours settings.",
                true);

            _showOverlay = Settings.GetHandle(
                "showOverlay", "Show targeting overlay",
                "When manually targeting a ranged weapon, highlight all tiles the projectile could pass through, accounting for miss radius.",
                true);

            _protectColonyAnimals = Settings.GetHandle(
                "protectColonyAnimals", "Protect All Tame Animals",
                "When Off, only trained animals with a master are protected. When On, all tame animals belonging to the colony are protected. May cause performance issues on lower end machines with large numbers of livestock.",
                false);
        }

        public void UpdateFireConeOverlay(bool enabled)
        {
            _fireConeOverlay?.Update(_showOverlay && enabled);
        }

        public bool ShouldProtectAllColonyAnimals()
        {
            return _protectColonyAnimals;
        }

        public static Pawn GetSelectedPawn()
        {
            List<object> selectedObjects = Find.Selector.SelectedObjects;
            if (selectedObjects == null || selectedObjects.Count != 1)
                return null;

            return selectedObjects.First() as Pawn;
        }

        public ExtendedDataStorage GetExtendedDataStorage()
        {
            return _extendedDataStorage;
        }

        public FireManager GetFireManager()
        {
            return _fireManager;
        }

        public bool IsModEnabled()
        {
            return _modEnabled;
        }
    }
}
