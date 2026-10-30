using HarmonyLib;
using LBoL.Core.Battle;

namespace LBOLPvP;

[HarmonyPatch(typeof(BattleController), "StartBattle")]
internal static class Act2SoloStatusPatch
{
	[HarmonyPostfix]
	private static void AfterStartBattle(BattleController __instance)
	{
		Act2SoloBattle.Subscribe(__instance);
	}
}
