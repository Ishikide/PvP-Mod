using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBOLPvP;

[HarmonyPatch(typeof(BattleStation), "OnEnter")]
internal static class Act2TestBattlePatch
{
	private static readonly string[] OpponentNames = new string[6] { "Reimu", "Marisa", "Cirno", "Sakuya", "Koishi", "Alice" };

	[HarmonyPrefix]
	private static void BeforeStationEnter(BattleStation __instance)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected Obj, but got Unknown
		ConfigEntry<bool> soloTestingMode = BepinexPlugin.SoloTestingMode;
		if (soloTestingMode == null || !soloTestingMode.Value || !(__instance is BossStation))
		{
			return;
		}
		Stage stage = ((Station)__instance).Stage;
		if (stage == null || stage.Index != 1)
		{
			return;
		}
		Type type = PickOpponentType(((Station)__instance).GameRun);
		EnemyGroupEntry val = __instance.EnemyGroupEntry ?? ((Station)__instance).Stage.GetBoss();
		if (type == null || val == null)
		{
			return;
		}
		EnemyGroupEntry val2 = new EnemyGroupEntry(val.Config);
		if (!(AccessTools.Field(typeof(EnemyGroupEntry), "_entries")?.GetValue(val2) is List<EntrySource> list))
		{
			ManualLogSource log = BepinexPlugin.Log;
			if (log != null)
			{
				log.LogWarning((object)"Could not access the Act 2 boss entries; keeping the original boss.");
			}
			return;
		}
		list.Clear();
		val2.Add(type);
		FieldInfo fieldInfo = AccessTools.Field(typeof(BattleStation), "<EnemyGroupEntry>k__BackingField");
		if (fieldInfo == null)
		{
			ManualLogSource log2 = BepinexPlugin.Log;
			if (log2 != null)
			{
				log2.LogWarning((object)"Could not assign the Act 2 test opponent; using the original boss.");
			}
			return;
		}
		fieldInfo.SetValue(__instance, val2);
		ManualLogSource log3 = BepinexPlugin.Log;
		if (log3 != null)
		{
			log3.LogInfo((object)("Act 2 solo test opponent: " + type.Name + " replaces " + val.Id + " before station preload."));
		}
	}

	private static Type PickOpponentType(GameRunController gameRun)
	{
		List<Type> list = new List<Type>(OpponentNames.Length);
		string[] opponentNames = OpponentNames;
		foreach (string text in opponentNames)
		{
			Type type = Type.GetType("LBoL.EntityLib.EnemyUnits.Opponent." + text + ", LBoL.EntityLib");
			if (type != null && typeof(EnemyUnit).IsAssignableFrom(type))
			{
				list.Add(type);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		int index = (int)(gameRun.RootSeed % (ulong)list.Count);
		return list[index];
	}
}
