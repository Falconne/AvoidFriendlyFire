using Harmony;
using UnityEngine;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(PawnNameColorUtility), "PawnNameColorOf")]
    public class PawnNameColorUtility_PawnNameColorOf_Patch
    {
        public static void Postfix(ref Pawn pawn, ref Color __result)
        {
            if (!Main.Instance.IsModEnabled())
                return;

            if (Main.Instance.PawnStatusTracker.IsAShooter(pawn))
            {
                __result = Color.cyan;
                return;
            }

            if (Main.Instance.PawnStatusTracker.IsABlocker(pawn))
            {
                __result = Color.green;
            }
        }
    }
}