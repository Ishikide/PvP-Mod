using HarmonyLib;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace LBOLPvP;

[HarmonyPatch(typeof(FirepowerNegative), "OnDamageDealing")]
internal static class Act2SoloFirepowerNegativePatch
{
	[HarmonyPrefix]
	private static bool BeforeDamageDealing(DamageDealingEventArgs args)
	{
		return !Act2SoloBattle.IsFirepowerIndependent(args);
	}
}
