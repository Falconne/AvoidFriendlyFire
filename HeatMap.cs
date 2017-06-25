using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvoidFriendlyFire
{
    public class HeatMap : ICellBoolGiver
    {
        public HeatMap()
        {
            _pawn = GetSelectedPawn();
        }
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
            if (_pawn == null)
                return false;

            if (Mouse.IsInputBlockedNow)
                return false;

            var map = Find.VisibleMap;
            if (map.fogGrid.IsFogged(index))
                return false;

            var mouseCell = UI.MouseCell();
            if (!mouseCell.InBounds(Find.VisibleMap) || mouseCell.Fogged(Find.VisibleMap))
                return false;

            var overlayCell = map.cellIndices.IndexToCell(index);
            if (overlayCell.AdjacentToCardinal(mouseCell) || overlayCell.AdjacentToDiagonal(mouseCell))
                return true;

            return false;
        }

        public Color GetCellExtraColor(int index)
        {
            return Color.white;
        }

        public Color Color => Color.red;

        public void Update()
        {
            Drawer.MarkForDraw();
            if (ShouldUpdate())
            {
                Drawer.SetDirty();
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

        private Pawn GetSelectedPawn()
        {
            return Find.VisibleMap.mapPawns.FreeColonists.FirstOrDefault(p => p.Drafted);
        }

        private Pawn _pawn;

        private IntVec3 _lastMouseCell;

        private CellBoolDrawer _drawerInt;
    }
}