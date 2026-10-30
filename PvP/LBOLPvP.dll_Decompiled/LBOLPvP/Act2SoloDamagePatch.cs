using System;
using System.Collections.Generic;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBOLPvP;

[HarmonyPatch(typeof(DamageAction), "GetPhases")]
internal static class Act2SoloDamagePatch
{
	[HarmonyPrefix]
	private static bool BeforeGetPhases(DamageAction __instance, ref IEnumerable<Phase> __result)
	{
		if (!Act2SoloBattle.TryQueue(__instance))
		{
			return true;
		}
		__result = Array.Empty<Phase>();
		return false;
	}
}
