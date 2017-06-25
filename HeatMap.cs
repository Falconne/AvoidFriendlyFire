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

            var targetCell = UI.MouseCell();
            if (!targetCell.InBounds(Find.VisibleMap) || targetCell.Fogged(Find.VisibleMap))
                return false;

            var pawnCell = _pawn.PositionHeld;
            if (targetCell == pawnCell)
                return false;

            var overlayCell = map.cellIndices.IndexToCell(index);
            if (overlayCell == targetCell || overlayCell == pawnCell)
                return false;

            if (overlayCell.AdjacentToCardinal(pawnCell) || overlayCell.AdjacentToDiagonal(pawnCell))
                return false;

            if (overlayCell.AdjacentToCardinal(targetCell) || overlayCell.AdjacentToDiagonal(targetCell))
                return true;

            var pawnToTargetDiff = targetCell - pawnCell;
            var checkedCellToTargetDiff = targetCell - overlayCell;
            if (Math.Abs(checkedCellToTargetDiff.x) > Math.Abs(pawnToTargetDiff.x) + 2)
                return false;

            if (Math.Abs(checkedCellToTargetDiff.z) > Math.Abs(pawnToTargetDiff.z) + 2)
                return false;

            var distanceFromPawnToTarget = pawnToTargetDiff.LengthManhattan;
            var distanceFromTargetToCheckedCell = checkedCellToTargetDiff.LengthManhattan;

            if (distanceFromTargetToCheckedCell > distanceFromPawnToTarget)
                return false;

            var checkedCellToPawnDiff = overlayCell - pawnCell;
            var distanceFromPawnToCheckedCell = checkedCellToPawnDiff.LengthManhattan;
            if (distanceFromPawnToCheckedCell > distanceFromPawnToTarget)
                return false;

            // (y1 - y2) * (x1 - x3) == (y1 - y3) * (x1 - x2);
            var lhs = Math.Abs((pawnCell.z - overlayCell.z) * (pawnCell.x - targetCell.x));
            var rhs = Math.Abs((pawnCell.z - targetCell.z) * (pawnCell.x - overlayCell.x));
            return Math.Abs(lhs - rhs) < 10;
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