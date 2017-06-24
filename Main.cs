using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvoidFriendlyFire
{
    public class Main : HugsLib.ModBase
    {
        public Main()
        {
            Instance = this;
        }

        public override string ModIdentifier => "AvoidFriendlyFire";

        internal static Main Instance { get; private set; }
    }
}
