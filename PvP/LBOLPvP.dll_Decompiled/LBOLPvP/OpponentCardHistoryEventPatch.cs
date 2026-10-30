using HarmonyLib;
using LBOLMP.Api;

namespace LBOLPvP;

[HarmonyPatch(typeof(MpBattleEvents), "RaisePartnerCardPlayed")]
internal static class OpponentCardHistoryEventPatch
{
	[HarmonyPostfix]
	private static void AfterPartnerCardPlayed(MpPartnerCardEventArgs args)
	{
		OpponentCardHistory.Record(args);
	}
}
