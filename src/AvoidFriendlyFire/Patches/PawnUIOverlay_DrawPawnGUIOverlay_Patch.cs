using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(PawnUIOverlay), "DrawPawnGUIOverlay")]
    public class PawnUIOverlay_DrawPawnGUIOverlay_Patch
    {
        private static readonly FieldInfo _pawnGetter =
            typeof(PawnUIOverlay).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(ref PawnUIOverlay __instance)
        {
            var pawn = (Pawn) _pawnGetter.GetValue(__instance);
            if (!pawn.Spawned || pawn.Map.fogGrid.IsFogged(pawn.Position))
            {
                return false;
            }

            if (pawn.RaceProps.Humanlike || pawn.Name == null)
                return true;

            if (!Main.Instance.PawnStatusTracker.IsABlocker(pawn))
                return true;

            Vector2 pos = GenMapUI.LabelDrawPosFor(pawn, -0.6f);
            GenMapUI.DrawPawnLabel(pawn, pos);

            return false;
        }
    }
}