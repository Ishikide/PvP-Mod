using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;

namespace LBOLPvP;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class PlayerIntentDescriptionPatch
{
	private static bool Prefix(IntentionTooltipSource __instance, ref string __result)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		IntentionWidget widget = __instance.widget;
		if (((widget != null) ? widget.Intention : null) == null || !Act2SoloBattle.TryGetIntent(__instance.widget.Intention, out var effect))
		{
			return true;
		}
		BattleAction action = effect.Action;
		DamageAction val = (DamageAction)(object)((action is DamageAction) ? action : null);
		if (val != null)
		{
			__result = $"Deal {Act2SoloBattle.PreviewDamage(effect)} damage to the opponent after their defensive turn. Resolves in queue order.";
			DamageInfo damageInfo = val.DealingArgs.DamageInfo;
			if (damageInfo.IsAccuracy)
			{
				__result += " Accurate: cannot be grazed; Block and Barrier still absorb it.";
			}
			if (effect.FixedDamage)
			{
				__result += " Unaffected by outgoing Firepower modifiers.";
			}
		}
		else
		{
			__result = "Apply " + effect.Label + " to the opponent when this queued effect resolves.";
		}
		return false;
	}
}
