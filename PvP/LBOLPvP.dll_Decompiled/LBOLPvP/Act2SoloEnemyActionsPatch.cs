using System.Collections.Generic;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Units;

namespace LBOLPvP;

[HarmonyPatch(typeof(EnemyUnit), "GetActions")]
internal static class Act2SoloEnemyActionsPatch
{
	[HarmonyPrefix]
	private static bool BeforeGetActions(EnemyUnit __instance, ref IEnumerable<BattleAction> __result)
	{
		if (!Act2SoloBattle.IsOpponent(__instance))
		{
			return true;
		}
		__result = Act2SoloBattle.DefendAndFlush(__instance);
		return false;
	}
}
