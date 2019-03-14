using System;
using Harmony;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public class Pawn_Kill_Patch
    {
        public static void Postfix(ref Pawn __instance)
        {
            if (__instance == null)
                return;

            try
            {
                if (!Main.Instance.IsModEnabled())
                    return;

                if (!__instance.Dead)
                    return;

                Main.Instance.PawnStatusTracker.KillOff(__instance);

                Main.Instance.GetExtendedDataStorage().DeleteExtendedDataFor(__instance);

            }
            catch (Exception )
            {
            }
        }
    }
}