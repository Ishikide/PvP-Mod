using System.Collections.Generic;
using System.Collections.ObjectModel;
using LBOLMP.Api;
using LBoL.Core;

namespace LBOLPvP;

internal static class OpponentCardHistory
{
	private static readonly List<OpponentCardPlay> Plays = new List<OpponentCardPlay>();

	private static readonly ReadOnlyCollection<OpponentCardPlay> ReadOnlyPlays = Plays.AsReadOnly();

	internal static IReadOnlyList<OpponentCardPlay> CurrentBattle => ReadOnlyPlays;

	internal static void Clear()
	{
		Plays.Clear();
	}

	internal static void Record(MpPartnerCardEventArgs args)
	{
		if (!BepinexPlugin.SoloTestingMode.Value && MpApi.IsOnline)
		{
			int num = TeamCoordinator.TeamOf(MpApi.LocalPlayerId);
			int num2 = TeamCoordinator.TeamOf(args.PlayerId);
			if (TeamRules.IsValidTeam(num) && TeamRules.IsValidTeam(num2) && num != num2)
			{
				Plays.Add(new OpponentCardPlay
				{
					Sequence = Plays.Count,
					PlayerId = args.PlayerId,
					PlayerName = args.PlayerName,
					CardId = args.CardId,
					IsUpgraded = args.IsUpgraded,
					IsToken = args.IsToken,
					TargetEnemyId = ((args.TargetEnemy == null) ? string.Empty : ((GameEntity)args.TargetEnemy).Id)
				});
			}
		}
	}
}
