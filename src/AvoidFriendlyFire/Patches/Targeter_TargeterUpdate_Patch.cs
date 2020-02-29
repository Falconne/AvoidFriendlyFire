using HarmonyLib;
using RimWorld;
using Verse;

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
            if (__instance.targetingSource == null)
                return;

            if (__instance.targetingSource.IsMeleeAttack)
                return;

            if (!(__instance.targetingSource is Verb verb)) 
                return;

            if (verb.HighlightFieldRadiusAroundTarget(out _) > 0.2f)
                return;

            Main.Instance.UpdateFireConeOverlay(true);
        }
    }
}