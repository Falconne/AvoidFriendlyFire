using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireConeOverlay : ICellBoolGiver
    {
        public CellBoolDrawer Drawer
        {
            get
            {
                if (_drawerInt == null)
                {
                    var map = Find.VisibleMap;
                    _drawerInt = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.33f);
                }
                return _drawerInt;
            }
        }

        public bool GetCellBool(int index)
        {
            return _fireCone != null && _fireCone.Contains(index);
        }

        public Color GetCellExtraColor(int index)
        {
            return Color.white;
        }

        public Color Color => Color.red;

        public void Update(bool enabled)
        {
            if (enabled)
            {
                Drawer.MarkForDraw();
                if (ShouldUpdate())
                {
                    BuildFireCone();
                    Drawer.SetDirty();
                }
                
            }
            Drawer.CellBoolDrawerUpdate();
        }

        private bool ShouldUpdate()
        {
            if (Mouse.IsInputBlockedNow)
                return false;

            var mouseCell = UI.MouseCell();
            if (mouseCell != _lastMouseCell)
            {
                _lastMouseCell = mouseCell;
                return true;
            }

            return false;
        }

        private void BuildFireCone()
        {
            _fireCone = null;
            var pawn = Main.GetSelectedDraftedPawn();
            if (pawn == null)
                return;

            if (!FireCalculations.HasValidWeapon(pawn))
                return;

            var targetCell = UI.MouseCell();
            _fireCone = FireCalculations.GetFireCone(pawn.Position, targetCell);
        }


        private IntVec3 _lastMouseCell;

        private CellBoolDrawer _drawerInt;

        private HashSet<int> _fireCone;
    }
}