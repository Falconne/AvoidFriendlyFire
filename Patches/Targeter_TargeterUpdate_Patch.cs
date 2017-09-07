using Harmony;
using RimWorld;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(Targeter), "TargeterUpdate")]
    public static class Targeter_TargeterUpdate_Patch
    {
        public static void Postfix(ref Targeter __instance)
        {
            if (__instance.targetingVerb == null)
                return;

            if (__instance.targetingVerb.verbProps.MeleeRange)
                return;

            if (__instance.targetingVerb.HighlightFieldRadiusAroundTarget() > 0.2f)
                return;
        }
    }
}