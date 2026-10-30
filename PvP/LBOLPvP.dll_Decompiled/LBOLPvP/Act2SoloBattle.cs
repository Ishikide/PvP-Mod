using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.Presentation.Units;
using UnityEngine;

namespace LBOLPvP;

internal static class Act2SoloBattle
{
	internal sealed class QueuedEffect
	{
		internal BattleAction Action;

		internal string Label;

		internal float Damage;

		internal bool IsCold;

		internal bool IsPoison;

		internal bool Submitted;

		internal bool FixedDamage;

		internal Intention Intention;
	}

	private static readonly List<QueuedEffect> Pending = new List<QueuedEffect>();

	private static readonly HashSet<DamageAction> ReplayingDamage = new HashSet<DamageAction>();

	private static readonly HashSet<StatusEffect> ReplayingStatus = new HashSet<StatusEffect>();

	private static readonly Dictionary<DamageDealingEventArgs, float> FixedDamageAmounts = new Dictionary<DamageDealingEventArgs, float>();

	private static readonly Dictionary<Intention, QueuedEffect> Intentions = new Dictionary<Intention, QueuedEffect>();

	private static EnemyUnit _opponent;

	private static BattleController _battle;

	private static bool _subscribed;

	private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

	private static readonly MethodInfo ExecuteDealing = typeof(GameEvent<DamageDealingEventArgs>).GetMethod("Execute", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo ExecuteReceiving = typeof(GameEvent<DamageEventArgs>).GetMethod("Execute", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo SetIntentionSource = typeof(Intention).GetMethod("SetSource", BindingFlags.Instance | BindingFlags.NonPublic);

	private static GUIStyle _intentStyle;

	private static GUIStyle _totalStyle;

	internal static bool Active => _battle != null;

	internal static IReadOnlyList<QueuedEffect> QueuedEffects => Pending;

	internal static bool IsOpponent(EnemyUnit unit)
	{
		if (_battle != null)
		{
			return unit == _opponent;
		}
		return false;
	}

	internal static void Start(GameRunController gameRun)
	{
		Clear();
		ConfigEntry<bool> soloTestingMode = BepinexPlugin.SoloTestingMode;
		if (soloTestingMode == null || !soloTestingMode.Value)
		{
			return;
		}
		Stage currentStage = gameRun.CurrentStage;
		if (currentStage == null || currentStage.Index != 1 || !(gameRun.CurrentStation is BossStation))
		{
			return;
		}
		BattleController battle = gameRun.Battle;
		if (battle == null || battle.EnemyGroup.Count != 1)
		{
			return;
		}
		EnemyUnit val = ((IEnumerable<EnemyUnit>)gameRun.Battle.EnemyGroup).First();
		if (!(((object)val).GetType().Namespace != "LBoL.EntityLib.EnemyUnits.Opponent"))
		{
			_battle = gameRun.Battle;
			_opponent = val;
			ManualLogSource log = BepinexPlugin.Log;
			if (log != null)
			{
				log.LogInfo((object)("Act 2 solo battle active against " + ((GameEntity)_opponent).Id + "."));
			}
		}
	}

	internal static void Clear()
	{
		if (_subscribed && _battle != null)
		{
			((Unit)_battle.Player).DamageDealing.RemoveHandler((GameEventHandler<DamageDealingEventArgs>)RestoreFixedDamage, (GameEventPriority)int.MaxValue);
		}
		if (_subscribed && _opponent != null)
		{
			((Unit)_opponent).DamageDealing.RemoveHandler((GameEventHandler<DamageDealingEventArgs>)RestoreFixedDamage, (GameEventPriority)int.MaxValue);
			((Unit)_opponent).StatusEffectAdding.RemoveHandler((GameEventHandler<StatusEffectApplyEventArgs>)ConvertDirectStatus, (GameEventPriority)int.MinValue);
			((Unit)_opponent).StatusEffectAdding.RemoveHandler((GameEventHandler<StatusEffectApplyEventArgs>)QueueStatus, (GameEventPriority)int.MaxValue);
		}
		_subscribed = false;
		PlayerIntentDisplay.Clear();
		_battle = null;
		_opponent = null;
		Pending.Clear();
		ReplayingDamage.Clear();
		ReplayingStatus.Clear();
		FixedDamageAmounts.Clear();
		Intentions.Clear();
	}

	internal static void Subscribe(BattleController battle)
	{
		if (battle == _battle && !_subscribed)
		{
			((Unit)battle.Player).DamageDealing.AddHandler((GameEventHandler<DamageDealingEventArgs>)RestoreFixedDamage, (GameEventPriority)int.MaxValue);
			((Unit)_opponent).DamageDealing.AddHandler((GameEventHandler<DamageDealingEventArgs>)RestoreFixedDamage, (GameEventPriority)int.MaxValue);
			((Unit)_opponent).StatusEffectAdding.AddHandler((GameEventHandler<StatusEffectApplyEventArgs>)ConvertDirectStatus, (GameEventPriority)int.MinValue);
			((Unit)_opponent).StatusEffectAdding.AddHandler((GameEventHandler<StatusEffectApplyEventArgs>)QueueStatus, (GameEventPriority)int.MaxValue);
			_subscribed = true;
		}
	}

	private static void RestoreFixedDamage(DamageDealingEventArgs args)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (FixedDamageAmounts.TryGetValue(args, out var value))
		{
			args.DamageInfo = DamageInfo.Attack(value, true);
		}
	}

	internal static bool TryQueue(DamageAction action)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected Obj, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (ReplayingDamage.Remove(action))
		{
			RemovePending((BattleAction)(object)action);
			return false;
		}
		if (_battle == null || !action.DealingArgs.Targets.Any((Unit target) => (object)target == _opponent))
		{
			return false;
		}
		DamageInfo val = action.DealingArgs.DamageInfo;
		bool flag = (int)val.DamageType == 0;
		if (flag)
		{
			val = DamageInfo.Attack(val.Amount, true);
		}
		DamageAction val2 = new DamageAction(action.DealingArgs.Source, (IEnumerable<Unit>)action.DealingArgs.Targets, val, action.GunName, action.GunType);
		((BattleAction)val2).SetSource(((BattleAction)action).Source).SetCause(((BattleAction)action).Cause);
		AddPending(new QueuedEffect
		{
			Action = (BattleAction)(object)val2,
			Damage = val.Amount,
			FixedDamage = flag,
			Label = string.Format("{0} {1:0}", flag ? DirectDamageLabel(action.GunName) : "ATK", val.Amount)
		});
		if (flag)
		{
			FixedDamageAmounts[val2.DealingArgs] = val.Amount;
		}
		ReplayingDamage.Add(val2);
		return true;
	}

	private static void ConvertDirectStatus(StatusEffectApplyEventArgs args)
	{
		if (args.Effect is Cold || args.Effect is Poison)
		{
			QueueStatus(args);
		}
	}

	internal static void QueueStatus(StatusEffectApplyEventArgs args)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Invalid comparison between Unknown and I4
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Expected Obj, but got Unknown
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		if (((GameEventArgs)args).IsCanceled)
		{
			return;
		}
		if (ReplayingStatus.Remove(args.Effect))
		{
			QueuedEffect queuedEffect = Pending.FirstOrDefault((QueuedEffect effect) =>
			{
				BattleAction action = effect.Action;
				ApplyStatusEffectAction val2 = (ApplyStatusEffectAction)(object)((action is ApplyStatusEffectAction) ? action : null);
				return val2 != null && ((EventBattleAction<StatusEffectApplyEventArgs>)(object)val2).Args.Effect == args.Effect;
			});
			if (queuedEffect != null)
			{
				RemovePending(queuedEffect.Action);
			}
		}
		else
		{
			if (_battle == null || (object)args.Unit != _opponent || (int)args.Effect.Type != 1)
			{
				return;
			}
			if (args.Effect is Cold)
			{
				QueuedEffect queuedEffect2 = Pending.LastOrDefault((QueuedEffect effect) => effect.IsCold && !effect.Submitted);
				if (queuedEffect2 != null)
				{
					ReplaceQueuedDirectDamage(queuedEffect2, queuedEffect2.Damage + 9f, "Cold");
				}
				QueueDirectHit(((GameEventArgs)args).ActionSource, ((GameEventArgs)args).Cause, 9f, "Cold");
				((GameEventArgs)args).CancelBy((GameEntity)(object)_opponent);
			}
			else if (args.Effect is Poison)
			{
				int num = args.Level ?? args.Effect.Level;
				if (num > 0)
				{
					QueuedEffect queuedEffect3 = Pending.FirstOrDefault((QueuedEffect effect) => effect.IsPoison && !effect.Submitted);
					if (queuedEffect3 != null)
					{
						ReplaceQueuedDirectDamage(queuedEffect3, queuedEffect3.Damage + (float)num, "Poison");
					}
					else
					{
						QueueDirectHit(((GameEventArgs)args).ActionSource, ((GameEventArgs)args).Cause, num, "Poison");
					}
				}
				((GameEventArgs)args).CancelBy((GameEntity)(object)_opponent);
			}
			else
			{
				ApplyStatusEffectAction val = new ApplyStatusEffectAction(((object)args.Effect).GetType(), (Unit)(object)_opponent, args.Level, args.Duration, args.Count, (int?)args.Effect.Limit, args.WaitTime, true);
				((BattleAction)val).SetSource(((GameEventArgs)args).ActionSource).SetCause(((GameEventArgs)args).Cause);
				ReplayingStatus.Add(((EventBattleAction<StatusEffectApplyEventArgs>)(object)val).Args.Effect);
				AddPending(new QueuedEffect
				{
					Action = (BattleAction)(object)val,
					Label = ((GameEntity)args.Effect).Id
				});
				((GameEventArgs)args).CancelBy((GameEntity)(object)_opponent);
			}
		}
	}

	private static void QueueDirectHit(GameEntity source, ActionCause cause, float damage, string label)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		DamageAction val = new DamageAction((Unit)(object)_battle.Player, (Unit)(object)_opponent, DamageInfo.Attack(damage, true), "Instant", (GunType)0);
		((BattleAction)val).SetSource(source).SetCause(cause);
		AddPending(new QueuedEffect
		{
			Action = (BattleAction)(object)val,
			Damage = damage,
			Label = $"{label} {damage:0}",
			IsCold = (label == "Cold"),
			IsPoison = (label == "Poison"),
			FixedDamage = true
		});
		FixedDamageAmounts[val.DealingArgs] = damage;
		ReplayingDamage.Add(val);
	}

	private static void ReplaceQueuedDirectDamage(QueuedEffect effect, float damage, string label)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected Obj, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		DamageAction val = (DamageAction)effect.Action;
		DamageAction val2 = new DamageAction(val.DealingArgs.Source, (Unit)(object)_opponent, DamageInfo.Attack(damage, true), val.GunName, (GunType)0);
		((BattleAction)val2).SetSource(((BattleAction)val).Source).SetCause(((BattleAction)val).Cause);
		ReplayingDamage.Remove(val);
		FixedDamageAmounts.Remove(val.DealingArgs);
		ReplayingDamage.Add(val2);
		FixedDamageAmounts[val2.DealingArgs] = damage;
		effect.Action = (BattleAction)(object)val2;
		effect.Damage = damage;
		effect.Label = $"{label} {damage:0}";
		PlayerIntentDisplay.Invalidate();
	}

	private static void AddPending(QueuedEffect effect)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		BattleAction action = effect.Action;
		DamageAction val = (DamageAction)(object)((action is DamageAction) ? action : null);
		Intention intention;
		if (val == null)
		{
			intention = Intention.NegativeEffect((string)null);
		}
		else
		{
			int num = (int)effect.Damage;
			DamageInfo damageInfo = val.DealingArgs.DamageInfo;
			intention = Intention.Attack(num, damageInfo.IsAccuracy);
		}
		effect.Intention = intention;
		SetIntentionSource.Invoke(effect.Intention, new object[1] { _opponent });
		effect.Intention.ShowByEnemyTurn = true;
		Intentions.Add(effect.Intention, effect);
		Pending.Add(effect);
		PlayerIntentDisplay.Invalidate();
	}

	private static void RemovePending(BattleAction action)
	{
		QueuedEffect[] array = Pending.Where((QueuedEffect effect) => effect.Action == action).ToArray();
		foreach (QueuedEffect item in array)
		{
			Pending.Remove(item);
		}
		PlayerIntentDisplay.Invalidate();
	}

	internal static bool TryGetIntent(Intention intention, out QueuedEffect effect)
	{
		return Intentions.TryGetValue(intention, out effect);
	}

	internal static int PreviewDamage(QueuedEffect effect)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected Obj, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		BattleAction action = effect.Action;
		DamageAction val = (DamageAction)(object)((action is DamageAction) ? action : null);
		if (val == null)
		{
			return 0;
		}
		DamageAction val2 = new DamageAction(val.DealingArgs.Source, (Unit)(object)_opponent, val.DealingArgs.DamageInfo, val.GunName, (GunType)0);
		((BattleAction)val2).SetSource(((BattleAction)val).Source).SetCause((ActionCause)20);
		if (effect.FixedDamage)
		{
			FixedDamageAmounts[val2.DealingArgs] = effect.Damage;
		}
		try
		{
			if (val2.DealingArgs.Source != null)
			{
				ExecuteDealing.Invoke(val2.DealingArgs.Source.DamageDealing, new object[1] { val2.DealingArgs });
			}
			RestoreFixedDamage(val2.DealingArgs);
			val2.DamageArgs[0].DamageInfo = val2.DealingArgs.DamageInfo;
			ExecuteReceiving.Invoke(((Unit)_opponent).DamageReceiving, new object[1] { val2.DamageArgs[0] });
			DamageInfo damageInfo = val2.DamageArgs[0].DamageInfo;
			return Math.Max(0, (int)Math.Round(damageInfo.Damage, MidpointRounding.AwayFromZero));
		}
		finally
		{
			FixedDamageAmounts.Remove(val2.DealingArgs);
		}
	}

	private static string DirectDamageLabel(string gunName)
	{
		if (!(gunName ?? string.Empty).StartsWith("Poison", StringComparison.OrdinalIgnoreCase))
		{
			if (!(gunName ?? string.Empty).StartsWith("Cold", StringComparison.OrdinalIgnoreCase))
			{
				return "HIT";
			}
			return "COLD";
		}
		return "POI";
	}

	internal static bool IsFirepowerIndependent(DamageDealingEventArgs args)
	{
		if (_battle == null)
		{
			return false;
		}
		if (FixedDamageAmounts.ContainsKey(args))
		{
			return true;
		}
		string text = args.GunName ?? string.Empty;
		if (text.StartsWith("Poison", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Cold", StringComparison.OrdinalIgnoreCase) || text.Contains("PermanentBorder", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		GameEntity actionSource = ((GameEventArgs)args).ActionSource;
		return string.Equals((actionSource != null) ? actionSource.Id : null, "PermanentBorder", StringComparison.OrdinalIgnoreCase);
	}

	internal static IEnumerable<BattleAction> DefendAndFlush(EnemyUnit opponent)
	{
		int num = 6 + Math.Min(6, ((Unit)opponent).TurnCounter);
		yield return (BattleAction)new CastBlockShieldAction((Unit)(object)opponent, num, 0, (BlockShieldType)1, true);
		if (((Unit)opponent).TurnCounter % 2 == 0)
		{
			yield return (BattleAction)new ApplyStatusEffectAction(typeof(Firepower), (Unit)(object)opponent, (int?)1, (int?)null, (int?)null, (int?)null, 0f, true);
		}
		QueuedEffect[] array = Pending.Where((QueuedEffect effect) => !effect.Submitted).ToArray();
		foreach (QueuedEffect queuedEffect in array)
		{
			queuedEffect.Submitted = true;
			_battle.RequestDebugAction(queuedEffect.Action, "PvP deferred " + queuedEffect.Label);
		}
	}

	internal static void ShowDefensiveIntent(EnemyUnit opponent)
	{
		if (IsOpponent(opponent))
		{
			opponent.Intentions.Clear();
			AddIntention(opponent, Intention.Defend());
			if ((((Unit)opponent).TurnCounter + 1) % 2 == 0)
			{
				AddIntention(opponent, Intention.PositiveEffect());
			}
			opponent.NotifyIntentionsChanged();
		}
	}

	private static void AddIntention(EnemyUnit opponent, Intention intention)
	{
		SetIntentionSource.Invoke(intention, new object[1] { opponent });
		opponent.Intentions.Add(intention);
	}

	internal static void DrawIntent()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected Obj, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected Obj, but got Unknown
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerIntentDisplay.Ready || _battle == null || Pending.Count == 0 || (Object)(object)GameDirector.Player == (Object)null || (Object)(object)Camera.main == (Object)null)
		{
			return;
		}
		Vector3 val = Camera.main.WorldToScreenPoint(((Component)GameDirector.Player).transform.position + Vector3.up * 2.4f);
		if (!(val.z <= 0f))
		{
			if (_intentStyle == null)
			{
				_intentStyle = new GUIStyle(GUI.skin.box)
				{
					alignment = (TextAnchor)4,
					fontSize = 12
				};
			}
			if (_totalStyle == null)
			{
				_totalStyle = new GUIStyle(GUI.skin.box)
				{
					alignment = (TextAnchor)4,
					fontSize = 13,
					fontStyle = (FontStyle)1
				};
			}
			int num = (Pending.Count + 8 - 1) / 8;
			float num2 = Mathf.Clamp(val.x - (float)Math.Min(8, Pending.Count) * 52f / 2f, 4f, Math.Max(4f, (float)Screen.width - 416f - 4f));
			float num3 = Mathf.Clamp((float)Screen.height - val.y - 44f - (float)num * 25f, 4f, Math.Max(4f, (float)Screen.height - 25f * (float)(num + 1)));
			for (int i = 0; i < Pending.Count; i++)
			{
				int num4 = i / 8;
				int num5 = i % 8;
				GUI.Label(new Rect(num2 + (float)num5 * 52f, num3 + (float)num4 * 25f, 50f, 23f), Pending[i].Label, _intentStyle);
			}
			float num6 = Pending.Sum((QueuedEffect effect) => effect.Damage);
			GUI.Label(new Rect(num2, num3 + (float)num * 25f, (float)Math.Min(8, Pending.Count) * 52f - 2f, 25f), $"Queued damage {num6:0}", _totalStyle);
		}
	}
}
