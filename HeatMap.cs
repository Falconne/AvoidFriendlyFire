using System;
using System.Collections.Generic;
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
            if (!targetCell.InBounds(map) || targetCell.Fogged(map))
                return false;

            var pawnCell = _pawn.Position;
            if (targetCell == pawnCell)
                return false;

            var overlayCell = map.cellIndices.IndexToCell(index);
            if (overlayCell == targetCell || overlayCell == pawnCell)
                return false;

            // Nearby pawns do not receive friendly fire
            var checkedCellToPawnDiff = overlayCell - pawnCell;

            {
                var xDiff = Math.Abs(checkedCellToPawnDiff.x);
                var zDiff = Math.Abs(checkedCellToPawnDiff.z);
                //Main.Instance.Logger.Message($"x,z: {xDiff}, {zDiff}");
                if ((xDiff == 0 && zDiff < 5) || (zDiff == 0 && xDiff < 5))
                    return false;

                if (xDiff + zDiff < 6)
                    return false;
            }

            // Any pawn adjacent to target can get hit
            if (overlayCell.AdjacentToCardinal(targetCell) || overlayCell.AdjacentToDiagonal(targetCell))
                return true;

            // Eliminate as many cells as possible based on simple distance arithmetic
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

            var distanceFromPawnToCheckedCell = checkedCellToPawnDiff.LengthManhattan;
            if (distanceFromPawnToCheckedCell > distanceFromPawnToTarget)
                return false;

            // No more easy elimination; now do brute force check to see if cell is in the fire cone
            if (_fireCone.Contains(index))
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
                BuildFireCone();
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

        private void BuildFireCone()
        {
            _fireCone.Clear();
            if (_pawn == null)
                return;

            if (Mouse.IsInputBlockedNow)
                return;

            var map = Find.VisibleMap;

            var targetCell = UI.MouseCell();
            if (!targetCell.InBounds(map) || targetCell.Fogged(map))
                return;

            var pawnCell = _pawn.Position;
            if (targetCell == pawnCell)
                return;

            for (int xSplash = targetCell.x - 1; xSplash <= targetCell.x + 1; xSplash++)
            {
                for (int zSplash = targetCell.z - 1; zSplash <= targetCell.z + 1; zSplash++)
                {
                    var splashTarget = new IntVec3(xSplash, targetCell.y, zSplash);
                    _fireCone.AddRange(GetShootablePointsBetween(pawnCell, splashTarget, map));
                }
            }
        }

        private IEnumerable<int> GetShootablePointsBetween(IntVec3 source, IntVec3 target, Map map)
        {
            return GenSight.PointsOnLineOfSight(source, target)
                .Select(v => map.cellIndices.CellToIndex(v.x, v.z))
                .Where(i => !_fireCone.Contains(i));
        }

        private Pawn GetSelectedPawn()
        {
            return Find.VisibleMap.mapPawns.FreeColonists.FirstOrDefault(p => p.Drafted);
        }

        private readonly Pawn _pawn;

        private IntVec3 _lastMouseCell;

        private CellBoolDrawer _drawerInt;

        private readonly List<int> _fireCone = new List<int>();
    }
}