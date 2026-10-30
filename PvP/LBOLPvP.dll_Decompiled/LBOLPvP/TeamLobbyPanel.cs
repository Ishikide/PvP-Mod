using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBOLMP.Api;
using UnityEngine;

namespace LBOLPvP;

internal static class TeamLobbyPanel
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static WindowFunction _003C0_003E__DrawWindow;
	}

	private static Rect _window = new Rect(24f, 90f, 370f, 500f);

	private static GUIStyle _row;

	internal static void Draw()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected Obj, but got Unknown
		if (_row == null)
		{
			_row = new GUIStyle(GUI.skin.label)
			{
				wordWrap = true
			};
		}
		Rect window = _window;
		WindowFunction val = _003C_003EO._003C0_003E__DrawWindow;
		if (val == null)
		{
			WindowFunction val2 = DrawWindow;
			_003C_003EO._003C0_003E__DrawWindow = val2;
			val = val2;
		}
		_window = GUI.Window(184027, window, val, "PvP Teams");
	}

	private static void DrawWindow(int id)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		bool value = BepinexPlugin.SoloTestingMode.Value;
		bool flag = GUILayout.Toggle(value, "Solo testing mode", Array.Empty<GUILayoutOption>());
		if (flag != value)
		{
			BepinexPlugin.SoloTestingMode.Value = flag;
		}
		GUILayout.Space(8f);
		if (flag)
		{
			GUILayout.Label("Team selection and opponent routes are disabled. Act progression stays in single-player mode.", _row, Array.Empty<GUILayoutOption>());
			GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
			return;
		}
		if (!MpApi.IsOnline)
		{
			GUILayout.Label("Start or join an LBOLMP session to select teams.", _row, Array.Empty<GUILayoutOption>());
			GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
			return;
		}
		GUILayout.Label("Choose a team. Both teams need at least one player before the run can begin.", _row, Array.Empty<GUILayoutOption>());
		GUILayout.Space(8f);
		int num = TeamCoordinator.TeamOf(MpApi.LocalPlayerId);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Your team: " + TeamName(num), new GUILayoutOption[1] { GUILayout.Width(100f) });
		for (int i = 0; i < 2; i++)
		{
			GUI.enabled = num != i;
			if (GUILayout.Button("Join " + (i + 1), new GUILayoutOption[1] { GUILayout.Width(78f) }))
			{
				TeamCoordinator.RequestTeam(i);
			}
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		GUILayout.Space(8f);
		foreach (MpApiPlayer item in MpApi.Players.OrderBy((MpApiPlayer player) => player.Id))
		{
			MpApiPlayer current = item;
			GUILayout.Label(current.Name + ": " + TeamName(TeamCoordinator.TeamOf(current.Id)), _row, Array.Empty<GUILayoutOption>());
		}
		GUILayout.Space(8f);
		GUILayout.Label($"Team 1: {TeamCoordinator.CountOnTeam(0)}    Team 2: {TeamCoordinator.CountOnTeam(1)}", _row, Array.Empty<GUILayoutOption>());
		GUILayout.Label(TeamCoordinator.IsReady ? "Both teams are represented." : "Each team needs at least one player before the run can begin.", _row, Array.Empty<GUILayoutOption>());
		GUILayout.Space(8f);
		bool value2 = BepinexPlugin.ShowOpponentPaths.Value;
		bool flag2 = GUILayout.Toggle(value2, "Show opponent team's route on the map", Array.Empty<GUILayoutOption>());
		if (flag2 != value2)
		{
			BepinexPlugin.ShowOpponentPaths.Value = flag2;
		}
		GUILayout.Space(10f);
		GUILayout.Label("Opponent cards this battle", _row, Array.Empty<GUILayoutOption>());
		IReadOnlyList<OpponentCardPlay> currentBattle = OpponentCardHistory.CurrentBattle;
		for (int num2 = Math.Max(0, currentBattle.Count - 8); num2 < currentBattle.Count; num2++)
		{
			OpponentCardPlay opponentCardPlay = currentBattle[num2];
			string arg = opponentCardPlay.CardId + (opponentCardPlay.IsUpgraded ? "+" : string.Empty) + (opponentCardPlay.IsToken ? " (copy)" : string.Empty);
			GUILayout.Label($"{opponentCardPlay.Sequence + 1}. {opponentCardPlay.PlayerName}: {arg}", _row, Array.Empty<GUILayoutOption>());
		}
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
	}

	private static string TeamName(int team)
	{
		if (!TeamRules.IsValidTeam(team))
		{
			return "Unassigned";
		}
		return $"Team {team + 1}";
	}

	static TeamLobbyPanel()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
	}
}
