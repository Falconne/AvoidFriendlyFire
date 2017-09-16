using Harmony;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public class Pawn_Kill_Patch
    {
        public static void Postfix(ref Pawn __instance)
        {
            if (!Main.Instance.IsModEnabled())
                return;

            if (!__instance.Dead)
                return;

            Main.Instance.GetExtendedDataStorage().DeleteExtendedDataFor(__instance);
        }
    }
}