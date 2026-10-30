using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBOLPvP;

internal static class PlayerIntentDisplay
{
	private const int PerRow = 8;

	private static readonly Dictionary<Act2SoloBattle.QueuedEffect, IntentionWidget> Widgets = new Dictionary<Act2SoloBattle.QueuedEffect, IntentionWidget>();

	private static UnitInfoWidget _host;

	private static IntentionWidget _template;

	private static Transform _layout;

	private static Transform _anchor;

	private static TextMeshProUGUI _total;

	private static bool _dirty;

	private static bool _reportedError;

	private static float _nextRefresh;

	private static float _retryAfter;

	internal static bool Ready
	{
		get
		{
			if (Object.op_Implicit((Object)(object)_host) && Object.op_Implicit((Object)(object)_template))
			{
				return Object.op_Implicit((Object)(object)_layout);
			}
			return false;
		}
	}

	internal static void Invalidate()
	{
		_dirty = true;
	}

	internal static void Clear()
	{
		DestroyView();
		_reportedError = false;
		_retryAfter = 0f;
	}

	private static void DestroyView()
	{
		if (Object.op_Implicit((Object)(object)_host))
		{
			((Component)_host).gameObject.SetActive(false);
			Object.Destroy((Object)(object)((Component)_host).gameObject);
		}
		if (Object.op_Implicit((Object)(object)_anchor))
		{
			Object.Destroy((Object)(object)((Component)_anchor).gameObject);
		}
		Widgets.Clear();
		_host = null;
		_template = null;
		_layout = null;
		_anchor = null;
		_total = null;
		_dirty = true;
	}

	internal static void Tick()
	{
		if (!Act2SoloBattle.Active || !Object.op_Implicit((Object)(object)GameDirector.Player) || Time.unscaledTime < _retryAfter)
		{
			return;
		}
		try
		{
			if ((Ready || CreateView()) && (_dirty || !(Time.unscaledTime < _nextRefresh)))
			{
				Refresh();
				_dirty = false;
				_nextRefresh = Time.unscaledTime + 0.2f;
			}
		}
		catch (Exception ex)
		{
			if (!_reportedError)
			{
				ManualLogSource log = BepinexPlugin.Log;
				if (log != null)
				{
					log.LogError((object)("Player intention HUD could not be created/refreshed: " + ex));
				}
			}
			_reportedError = true;
			DestroyView();
			_retryAfter = Time.unscaledTime + 5f;
		}
	}

	private static bool CreateView()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected Obj, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected Obj, but got Unknown
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected Obj, but got Unknown
		UnitView player = GameDirector.Player;
		object? value = AccessTools.Field(typeof(UnitView), "_infoWidget").GetValue(player);
		UnitInfoWidget val = (UnitInfoWidget)((value is UnitInfoWidget) ? value : null);
		UnitStatusHud panel = UiManager.GetPanel<UnitStatusHud>();
		if (!Object.op_Implicit((Object)(object)val) || !Object.op_Implicit((Object)(object)val.TargetTransform) || !Object.op_Implicit((Object)(object)panel))
		{
			return false;
		}
		_anchor = new GameObject("PvPPlayerIntentAnchor").transform;
		_anchor.SetParent(val.TargetTransform, false);
		_anchor.localPosition = new Vector3(0f, 0.45f, 0f);
		_host = panel.CreateInfoWidget(player.Unit);
		((Object)_host).name = "PvP Player Outgoing Intentions";
		((Component)_host).gameObject.SetActive(true);
		_host.TargetTransform = _anchor;
		_host.UnitView = player;
		_host.Alpha = 1f;
		_host.IsMoveOrderVisible = false;
		if (Object.op_Implicit((Object)(object)_host.GamepadInteractionButton))
		{
			((Component)_host.GamepadInteractionButton).gameObject.SetActive(false);
		}
		_template = (IntentionWidget)AccessTools.Field(typeof(UnitInfoWidget), "intentionTemplate").GetValue(_host);
		_layout = (Transform)AccessTools.Field(typeof(UnitInfoWidget), "intentionLayout").GetValue(_host);
		if (!Object.op_Implicit((Object)(object)_template) || !Object.op_Implicit((Object)(object)_layout))
		{
			throw new InvalidOperationException("The native intention template/layout is missing.");
		}
		((Component)_layout).gameObject.SetActive(true);
		_total = (TextMeshProUGUI)AccessTools.Field(typeof(UnitInfoWidget), "moveNameText").GetValue(_host);
		((Component)_total).gameObject.SetActive(true);
		((Graphic)_total).raycastTarget = false;
		((TMP_Text)_total).alignment = (TextAlignmentOptions)514;
		_dirty = true;
		ManualLogSource log = BepinexPlugin.Log;
		if (log != null)
		{
			log.LogInfo((object)"Native PvP intention widgets attached above the player.");
		}
		return true;
	}

	private static void Refresh()
	{
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Expected Obj, but got Unknown
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<Act2SoloBattle.QueuedEffect> effects = Act2SoloBattle.QueuedEffects;
		KeyValuePair<Act2SoloBattle.QueuedEffect, IntentionWidget>[] array = Widgets.Where((KeyValuePair<Act2SoloBattle.QueuedEffect, IntentionWidget> entry) => !effects.Contains(entry.Key)).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			KeyValuePair<Act2SoloBattle.QueuedEffect, IntentionWidget> keyValuePair = array[num];
			((Component)keyValuePair.Value).gameObject.SetActive(false);
			Object.Destroy((Object)(object)((Component)keyValuePair.Value).gameObject);
			Widgets.Remove(keyValuePair.Key);
		}
		int num2 = 0;
		foreach (Act2SoloBattle.QueuedEffect item in effects)
		{
			if (!Widgets.TryGetValue(item, out var value))
			{
				value = Object.Instantiate<IntentionWidget>(_template, _layout);
				Widgets.Add(item, value);
				((Object)value).name = "PvP intent: " + item.Label;
				((Component)value).gameObject.SetActive(true);
				value.SetIntention(item.Intention, true);
				AddStatusBadge(value, item);
			}
			else
			{
				value.UpdateProperties();
			}
			num2 += Act2SoloBattle.PreviewDamage(item);
		}
		float num3 = Math.Max(82f, ((RectTransform)((Component)_template).transform).sizeDelta.y + 24f);
		for (int num4 = 0; num4 < effects.Count; num4 += 8)
		{
			int num5 = Math.Min(num4 + 8, effects.Count);
			float num6 = 0f;
			for (int num7 = num4; num7 < num5; num7++)
			{
				num6 += Widgets[effects[num7]].GetWidth() + 12f;
			}
			float num8 = (0f - num6) / 2f;
			for (int num9 = num4; num9 < num5; num9++)
			{
				IntentionWidget val = Widgets[effects[num9]];
				RectTransform val2 = (RectTransform)((Component)val).transform;
				val2.anchoredPosition = new Vector2(num8 + val2.sizeDelta.x / 2f, (float)(num4 / 8) * num3);
				num8 += val.GetWidth() + 12f;
			}
		}
		int num10 = (effects.Count + 8 - 1) / 8;
		((TMP_Text)_total).text = $"Outgoing: {num2} damage";
		((TMP_Text)_total).rectTransform.anchoredPosition = new Vector2(0f, (float)num10 * num3 + 24f);
		_host.SetVisible(effects.Count > 0, true);
	}

	private static void AddStatusBadge(IntentionWidget widget, Act2SoloBattle.QueuedEffect effect)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		object obj2;
		if (!effect.IsCold)
		{
			if (!effect.IsPoison)
			{
				BattleAction action = effect.Action;
				BattleAction obj = ((action is ApplyStatusEffectAction) ? action : null);
				obj2 = ((obj != null) ? ((GameEntity)((EventBattleAction<StatusEffectApplyEventArgs>)(object)obj).Args.Effect).Id : null);
			}
			else
			{
				obj2 = "Poison";
			}
		}
		else
		{
			obj2 = "Cold";
		}
		string text = (string)obj2;
		if (text != null)
		{
			Sprite val = ResourcesHelper.TryGetSprite<StatusEffect>(text);
			if (Object.op_Implicit((Object)(object)val))
			{
				GameObject val2 = new GameObject("Queued " + text, new Type[2]
				{
					typeof(RectTransform),
					typeof(Image)
				});
				val2.transform.SetParent(((Component)widget).transform, false);
				RectTransform val3 = (RectTransform)val2.transform;
				Vector2 anchorMin = (val3.anchorMax = new Vector2(0f, 0f));
				val3.anchorMin = anchorMin;
				val3.pivot = new Vector2(0.5f, 0.5f);
				val3.anchoredPosition = new Vector2(4f, 4f);
				val3.sizeDelta = new Vector2(28f, 28f);
				Image component = val2.GetComponent<Image>();
				component.sprite = val;
				component.preserveAspect = true;
				((Graphic)component).raycastTarget = false;
			}
		}
	}
}
