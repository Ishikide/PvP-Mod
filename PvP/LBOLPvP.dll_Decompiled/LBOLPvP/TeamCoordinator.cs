using System;
using System.Collections.Generic;
using System.Linq;
using LBOLMP.Api;
using UnityEngine;

namespace LBOLPvP;

internal static class TeamCoordinator
{
	[Serializable]
	private sealed class TeamRequest
	{
		public int PlayerId;

		public int Team;
	}

	[Serializable]
	private sealed class TeamState
	{
		public int[] PlayerIds;

		public int[] Teams;
	}

	private const string TeamRequestKey = "lbol.pvp.team.request.v1";

	private const string TeamStateKey = "lbol.pvp.team.state.v1";

	private static readonly Dictionary<int, int> Teams = new Dictionary<int, int>();

	private static IDisposable _requestSubscription;

	private static IDisposable _stateSubscription;

	private static float _nextSnapshot;

	internal static bool IsReady
	{
		get
		{
			if (TeamRules.HasBothTeams(Teams.Values))
			{
				return MpApi.Players.All((MpApiPlayer player) => Teams.TryGetValue(player.Id, out var value) && TeamRules.IsValidTeam(value));
			}
			return false;
		}
	}

	internal static int TeamOf(int playerId)
	{
		if (!Teams.TryGetValue(playerId, out var value))
		{
			return -1;
		}
		return value;
	}

	internal static int CountOnTeam(int team)
	{
		return Enumerable.Count(Teams.Values, (int value) => value == team);
	}

	internal static void EnsureSubscriptions()
	{
		if (_requestSubscription == null)
		{
			_requestSubscription = MpApi.Subscribe<TeamRequest>("lbol.pvp.team.request.v1", (Action<TeamRequest, int>)OnRequest);
		}
		if (_stateSubscription == null)
		{
			_stateSubscription = MpApi.Subscribe<TeamState>("lbol.pvp.team.state.v1", (Action<TeamState, int>)OnState);
		}
	}

	internal static void ReconcilePlayers()
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!MpApi.IsOnline)
		{
			Teams.Clear();
			return;
		}
		EnsureSubscriptions();
		if (!MpApi.IsHost)
		{
			return;
		}
		IReadOnlyList<MpApiPlayer> players = MpApi.Players;
		HashSet<int> activeIds = new HashSet<int>(players.Select((MpApiPlayer player) => player.Id));
		int[] array = Teams.Keys.Where((int id) => !activeIds.Contains(id)).ToArray();
		foreach (int key in array)
		{
			Teams.Remove(key);
		}
		foreach (MpApiPlayer item in players)
		{
			MpApiPlayer current = item;
			if (!Teams.ContainsKey(current.Id))
			{
				Teams[current.Id] = -1;
			}
		}
		if (Time.unscaledTime >= _nextSnapshot)
		{
			_nextSnapshot = Time.unscaledTime + 2f;
			BroadcastState();
		}
	}

	internal static void RequestTeam(int team)
	{
		if (TeamRules.IsValidTeam(team) && MpApi.LocalPlayerId >= 0)
		{
			if (MpApi.IsHost)
			{
				TrySetTeam(MpApi.LocalPlayerId, team);
				return;
			}
			MpApi.Send<TeamRequest>("lbol.pvp.team.request.v1", new TeamRequest
			{
				PlayerId = MpApi.LocalPlayerId,
				Team = team
			}, false);
		}
	}

	private static void OnRequest(TeamRequest request, int senderId)
	{
		if (MpApi.IsHost && request.PlayerId == senderId && MpApi.Players.Any((MpApiPlayer player) => player.Id == senderId))
		{
			TrySetTeam(senderId, request.Team);
		}
	}

	private static void OnState(TeamState state, int senderId)
	{
		if (senderId != 0 || state.PlayerIds == null || state.Teams == null || state.PlayerIds.Length != state.Teams.Length)
		{
			return;
		}
		Teams.Clear();
		for (int i = 0; i < state.PlayerIds.Length; i++)
		{
			if (TeamRules.IsValidTeam(state.Teams[i]))
			{
				Teams[state.PlayerIds[i]] = state.Teams[i];
			}
		}
	}

	private static void TrySetTeam(int playerId, int team)
	{
		if (Teams.ContainsKey(playerId) && TeamRules.IsValidTeam(team))
		{
			Teams[playerId] = team;
			BroadcastState();
		}
	}

	private static void BroadcastState()
	{
		int[] array = Teams.Keys.OrderBy((int id) => id).ToArray();
		TeamState teamState = new TeamState
		{
			PlayerIds = array,
			Teams = array.Select((int id) => Teams[id]).ToArray()
		};
		MpApi.Send<TeamState>("lbol.pvp.team.state.v1", teamState, false);
		OnState(teamState, 0);
	}
}
