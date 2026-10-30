using HarmonyLib;
using LBoL.Core.Battle.BattleActions;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;

namespace LBOLPvP;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class PlayerIntentTitlePatch
{
	private static bool Prefix(IntentionTooltipSource __instance, ref string __result)
	{
		IntentionWidget widget = __instance.widget;
		if (((widget != null) ? widget.Intention : null) == null || !Act2SoloBattle.TryGetIntent(__instance.widget.Intention, out var effect))
		{
			return true;
		}
		ref string reference = ref __result;
		object obj;
		if (effect.IsCold)
		{
			obj = "Outgoing Cold";
		}
		else if (effect.IsPoison)
		{
			obj = "Outgoing Poison";
		}
		else
		{
			obj = ((effect.Action is DamageAction) ? "Outgoing attack" : ("Queued " + effect.Label));
		}
		reference = (string)obj;
		return false;
	}
}
