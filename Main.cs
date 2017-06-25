using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AvoidFriendlyFire
{
    public class Main : HugsLib.ModBase
    {
        public Main()
        {
            Instance = this;
        }

        public override void MapLoaded(Map map)
        {
            _heatMap = new HeatMap();
        }

        public override string ModIdentifier => "AvoidFriendlyFire";

        internal static Main Instance { get; private set; }

        public void UpdateHeatMap()
        {
            _heatMap?.Update();
        }

        private HeatMap _heatMap;
    }
}
