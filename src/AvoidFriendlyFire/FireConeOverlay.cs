using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AvoidFriendlyFire
{
    public class FireConeOverlay : ICellBoolGiver
    {
        private IntVec3 _lastMouseCell;

        private CellBoolDrawer _drawerInt;

        private HashSet<int> _fireCone;


        public CellBoolDrawer Drawer
        {
            get
            {
                if (_drawerInt == null)
                {
                    var map = Find.CurrentMap;
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
            var pawn = Main.GetSelectedPawn();
            if (pawn == null)
                return;

            if (!HasValidWeapon(pawn))
                return;

            var targetCell = UI.MouseCell();
            var pawnCell = pawn.Position;
            if (pawnCell.DistanceTo(targetCell) > GetEquippedWeaponRange(pawn))
                return;

            var fireProperties = new FireProperties(pawn, targetCell);
            _fireCone = FireCalculations.GetFireCone(fireProperties);
        }

        public static float GetEquippedWeaponRange(Pawn pawn)
        {
            var primaryWeaponVerb = FireProperties.GetEquippedWeaponVerb(pawn);
            return primaryWeaponVerb?.verbProps.range ?? 0;
        }

        public static bool HasValidWeapon(Pawn pawn)
        {
            var primaryWeaponVerb = FireProperties.GetEquippedWeaponVerb(pawn);

            if (primaryWeaponVerb?.verbProps?.defaultProjectile?.projectile == null)
                return false;

            if (primaryWeaponVerb.verbProps.defaultProjectile.projectile.explosionRadius > 0.2f)
                // Can't handle explosive projectiles yet
                return false;

            // TODO check if projectile is flyOverhead

            return true;
        }
    }
}