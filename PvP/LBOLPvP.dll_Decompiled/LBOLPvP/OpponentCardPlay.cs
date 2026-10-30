namespace LBOLPvP;

internal sealed class OpponentCardPlay
{
	internal int Sequence { get; set; }

	internal int PlayerId { get; set; }

	internal string PlayerName { get; set; }

	internal string CardId { get; set; }

	internal bool IsUpgraded { get; set; }

	internal bool IsToken { get; set; }

	internal string TargetEnemyId { get; set; }
}
