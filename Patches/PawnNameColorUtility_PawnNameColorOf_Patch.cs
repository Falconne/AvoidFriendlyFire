using Harmony;
using UnityEngine;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(PawnNameColorUtility), "PawnNameColorOf")]
    public class PawnNameColorUtility_PawnNameColorOf_Patch
    {
        public static bool Prefix(ref Color __result, Pawn pawn)
        {
            if (!Main.Instance.IsModEnabled())
                return true;

            var extendedDataStorage = Main.Instance.GetExtendedDataStorage();
            if (!extendedDataStorage.ShouldPawnAvoidFriendlyFire(pawn))
                return true;

            if (extendedDataStorage.GetExtendedDataFor(pawn).IsBlocked())
            {
                __result = Color.yellow;
                return false;
            }

            return true;
        }
    }
}