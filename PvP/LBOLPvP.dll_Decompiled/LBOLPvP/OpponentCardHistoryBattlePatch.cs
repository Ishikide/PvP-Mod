using HarmonyLib;
using LBoL.Core;

namespace LBOLPvP;

[HarmonyPatch(typeof(GameRunController), "EnterBattle")]
internal static class OpponentCardHistoryBattlePatch
{
	[HarmonyPostfix]
	private static void AfterEnterBattle()
	{
		OpponentCardHistory.Clear();
	}
}
