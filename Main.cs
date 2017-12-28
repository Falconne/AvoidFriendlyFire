using System;
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

        private SettingHandle<bool> _ignoreShieldedPawns;

        private SettingHandle<bool> _modEnabled;

        private SettingHandle<bool> _enableWhenUndrafted;

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

            _ignoreShieldedPawns = Settings.GetHandle(
                "ignoreShieldedPawns", "Ignore pawns with active shield belts",
                "Shooters will not worry about pawns wearing a shield belt with at least 10% power standing in the line of fire.",
                true);

            _enableWhenUndrafted = Settings.GetHandle(
                "enableWhenUndrafted", "Always enable when undrafted",
                "If you tend to disable the 'Avoid Friendly Fire' setting on certain pawns during combat, using this option will ensure it is always turned back on again when they are undrafted.",
                false);

            try
            {
                var ceVerb = GenTypes.GetTypeInAnyAssembly("CombatExtended.Verb_LaunchProjectileCE");
                if (ceVerb == null)
                    return;

                Logger.Message("Patching CombatExtended methods");
                var vecType = GenTypes.GetTypeInAnyAssembly("Verse.IntVec3");
                var ltiType = GenTypes.GetTypeInAnyAssembly("Verse.LocalTargetInfo");

                var original = ceVerb.GetMethod("CanHitTargetFrom",
                    new [] {vecType, ltiType });

                var postfix = typeof(Verb_CanHitTargetFrom_Patch).GetMethod("Postfix");
                HarmonyInst.Patch(original, null, new HarmonyMethod(postfix));

            }
            catch (Exception e)
            {
                Logger.Error("Exception while trying to detect CombatExtended:");
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }
        }

        public void UpdateFireConeOverlay(bool enabled)
        {
            _fireConeOverlay?.Update(_showOverlay && enabled);
        }

        public bool ShouldProtectAllColonyAnimals()
        {
            return _protectColonyAnimals;
        }

        public bool ShouldIgnoreShieldedPawns()
        {
            return _ignoreShieldedPawns;
        }

        public bool ShouldEnableWhenUndrafted()
        {
            return _enableWhenUndrafted;
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
