using Harmony;
using RimWorld;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(Targeter), "TargeterUpdate")]
    public static class Targeter_TargeterUpdate_Patch
    {
        public static void Postfix(ref Targeter __instance)
        {
            if (!Main.Instance.IsModEnabled())
                return;

            Main.Instance.UpdateFireConeOverlay(false);
            if (__instance.targetingVerb == null)
                return;

            if (__instance.targetingVerb.verbProps.MeleeRange)
                return;

            bool dummy;
            if (__instance.targetingVerb.HighlightFieldRadiusAroundTarget(out dummy) > 0.2f)
                return;

            Main.Instance.UpdateFireConeOverlay(true);
        }
    }
}