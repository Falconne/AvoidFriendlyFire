using HugsLib.Utils;
using Verse;

namespace AvoidFriendlyFire
{
    public class Main : HugsLib.ModBase
    {
        private FireConeOverlay _fireConeOverlay;

        private ExtendedDataStorage _extendedDataStorage;

        private FireManager _fireManager;

        public Main()
        {
            Instance = this;
        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            _extendedDataStorage =
                UtilityWorldObjectManager.GetUtilityWorldObject<ExtendedDataStorage>();
            _fireManager = new FireManager();
            _fireConeOverlay = new FireConeOverlay();
        }

        public void UpdateFireConeOverlay(bool enabled)
        {
            if (_fireConeOverlay == null)
                return;

            _fireConeOverlay.Update(enabled);
        }

        public override string ModIdentifier => "AvoidFriendlyFire";

        public ExtendedDataStorage GetExtendedDataStorage()
        {
            return _extendedDataStorage;
        }

        public FireManager GetFireManager()
        {
            return _fireManager;
        }

        internal static Main Instance { get; private set; }

        internal new ModLogger Logger => base.Logger;
    }
}
