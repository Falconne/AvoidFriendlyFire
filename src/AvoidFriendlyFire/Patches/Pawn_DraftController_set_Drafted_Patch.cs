using Harmony;
using RimWorld;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    public class Pawn_DraftController_set_Drafted_Patch
    {
        public static void Postfix(Pawn_DraftController __instance, bool value)
        {
            if (!Main.Instance.ShouldEnableWhenUndrafted())
                return;

            if (value)
                return;

            var extendedDataStore = Main.Instance.GetExtendedDataStorage();
            if (extendedDataStore == null)
                return;

            var pawn = __instance.pawn;
            if (!extendedDataStore.CanTrackPawn(pawn))
                return;

            var pawnData = extendedDataStore.GetExtendedDataFor(pawn);
            pawnData.AvoidFriendlyFire = true;
        }
    }
}