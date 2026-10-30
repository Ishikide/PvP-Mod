using HarmonyLib;
using LBoL.Core;

namespace LBOLPvP;

[HarmonyPatch(typeof(GameRunController), "EnterBattle")]
internal static class Act2SoloBattleStartPatch
{
	[HarmonyPostfix]
	private static void AfterEnterBattle(GameRunController __instance)
	{
		Act2SoloBattle.Start(__instance);
	}
}
