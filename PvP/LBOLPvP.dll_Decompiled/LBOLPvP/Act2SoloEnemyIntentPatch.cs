using HarmonyLib;
using LBoL.Core.Units;

namespace LBOLPvP;

[HarmonyPatch(typeof(EnemyUnit), "UpdateTurnMoves")]
internal static class Act2SoloEnemyIntentPatch
{
	[HarmonyPostfix]
	private static void AfterUpdateTurnMoves(EnemyUnit __instance)
	{
		Act2SoloBattle.ShowDefensiveIntent(__instance);
	}
}
