using Harmony;
using Verse;
using Verse.AI;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(AttackTargetFinder), "BestAttackTarget")]
    public class AttackTargetFinder_BestAttackTarget_Patch
    {
        public static void Postfix(ref IAttackTarget __result, IAttackTargetSearcher searcher)
        {
            if (!Main.Instance.IsModEnabled())
                return;

            if (__result?.Thing == null)
                return;

            var shootingPawn = searcher?.Thing as Pawn;
            if (!Main.Instance.GetExtendedDataStorage().ShouldPawnAvoifFriendlyFire(shootingPawn))
                return;

            if (!Main.Instance.GetFireManager().CanHitTargetSafely(shootingPawn.Position, __result.Thing.Position))
                __result = null;
        }
    }
}