using HarmonyLib;
using LBoL.Core.Units;

namespace LBOLPvP;

[HarmonyPatch(typeof(Intention), "CalculateDamage")]
internal static class PlayerIntentDamagePreviewPatch
{
	private static bool Prefix(Intention __instance, ref int __result)
	{
		if (!Act2SoloBattle.TryGetIntent(__instance, out var effect))
		{
			return true;
		}
		__result = Act2SoloBattle.PreviewDamage(effect);
		return false;
	}
}
