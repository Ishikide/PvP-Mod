using HarmonyLib;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace LBOLPvP;

[HarmonyPatch(typeof(TempFirepowerNegative), "OnDamageDealing")]
internal static class Act2SoloTempFirepowerNegativePatch
{
	[HarmonyPrefix]
	private static bool BeforeDamageDealing(DamageDealingEventArgs args)
	{
		return !Act2SoloBattle.IsFirepowerIndependent(args);
	}
}
