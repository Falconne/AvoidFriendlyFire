using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AvoidFriendlyFire
{
    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    public class Pawn_DraftController_GetGizmos_Patch
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, ref Pawn_DraftController __instance)
        {
            if (!Main.Instance.IsModEnabled())
                return;

            if (Find.CurrentMap == null || Find.World == null || Find.World.renderer == null ||
                Find.World.renderer.wantedMode == WorldRenderMode.Planet)
            {
                return;
            }

            if (__result == null || !__result.Any())
                return;

            var extendedDataStore = Main.Instance.GetExtendedDataStorage();
            var pawn = __instance.pawn;
            if (!extendedDataStore.CanTrackPawn(pawn))
                return;

            if (!FireConeOverlay.HasValidWeapon(pawn))
                return;

            var pawnData = extendedDataStore.GetExtendedDataFor(pawn);

            var gizmoList = __result.ToList();
            var ourGizmo = new Command_Toggle
            {
                defaultLabel = "Avoid Friendly Fire",
                icon = Resources.FriendlyFireIcon,
                isActive = () => pawnData.AvoidFriendlyFire,
                toggleAction = () => pawnData.AvoidFriendlyFire = !pawnData.AvoidFriendlyFire
            };
            gizmoList.Add(ourGizmo);
            __result = gizmoList;
        }
    }
}