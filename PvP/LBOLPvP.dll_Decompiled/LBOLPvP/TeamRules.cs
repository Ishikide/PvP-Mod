using System;
using System.Collections.Generic;

namespace LBOLPvP;

internal static class TeamRules
{
	internal const int TeamCount = 2;

	internal const int Unassigned = -1;

	internal static bool IsValidTeam(int team)
	{
		if (team >= 0)
		{
			return team < 2;
		}
		return false;
	}

	internal static int[] Counts(IEnumerable<int> assignments)
	{
		int[] array = new int[2];
		foreach (int assignment in assignments)
		{
			if (IsValidTeam(assignment))
			{
				array[assignment]++;
			}
		}
		return array;
	}

	internal static bool HasBothTeams(IEnumerable<int> assignments)
	{
		int[] array = Counts(assignments);
		if (array[0] > 0)
		{
			return array[1] > 0;
		}
		return false;
	}

	internal static bool CanAssign(IReadOnlyDictionary<int, int> assignments, int playerId, int team)
	{
		if (!IsValidTeam(team))
		{
			return false;
		}
		if (!assignments.TryGetValue(playerId, out var value) || value == -1 || value == team)
		{
			return true;
		}
		int[] array = Counts(assignments.Values);
		if (array[value] <= 1)
		{
			return array[1 - value] == 0;
		}
		return true;
	}

	internal static float DamageMultiplier(int teamSize, int opposingTeamSize)
	{
		if (teamSize <= 0 || opposingTeamSize <= 0)
		{
			return 1f;
		}
		if (teamSize > opposingTeamSize)
		{
			return (float)opposingTeamSize / (float)teamSize;
		}
		return 1f;
	}

	internal static int ScaleDamage(int damage, int teamSize, int opposingTeamSize)
	{
		if (damage <= 0)
		{
			return damage;
		}
		return Math.Max(1, (int)Math.Floor((float)damage * DamageMultiplier(teamSize, opposingTeamSize)));
	}

	internal static bool MustSharePath(int localTeam, int otherTeam)
	{
		if (IsValidTeam(localTeam))
		{
			return localTeam == otherTeam;
		}
		return false;
	}
}
