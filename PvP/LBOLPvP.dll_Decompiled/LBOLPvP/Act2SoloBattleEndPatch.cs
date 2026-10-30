using HarmonyLib;
using LBoL.Core;

namespace LBOLPvP;

[HarmonyPatch(typeof(GameRunController), "LeaveBattle")]
internal static class Act2SoloBattleEndPatch
{
	[HarmonyPostfix]
	private static void AfterLeaveBattle()
	{
		Act2SoloBattle.Clear();
	}
}
