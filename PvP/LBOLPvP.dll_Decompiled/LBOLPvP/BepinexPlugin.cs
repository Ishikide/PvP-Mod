using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace LBOLPvP;

[BepInPlugin("lbol.pvp.teams", "LBOL PvP", "0.1.6")]
[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInProcess("LBoL.exe")]
public sealed class BepinexPlugin : BaseUnityPlugin
{
	public const string Guid = "lbol.pvp.teams";

	private Harmony _harmony;

	private float _nextReconcile;

	private bool _showTeamPanel;

	internal static ManualLogSource Log { get; private set; }

	internal static ConfigEntry<bool> ShowOpponentPaths { get; private set; }

	internal static ConfigEntry<bool> SoloTestingMode { get; private set; }

	internal static ConfigEntry<KeyCode> PanelHotkey { get; private set; }

	private void Awake()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected Obj, but got Unknown
		Log = ((BaseUnityPlugin)this).Logger;
		ShowOpponentPaths = ((BaseUnityPlugin)this).Config.Bind<bool>("Map", "ShowOpponentPaths", true, "Show the opposing team's route on the map.");
		SoloTestingMode = ((BaseUnityPlugin)this).Config.Bind<bool>("Testing", "SoloTestingMode", true, "Run without team selection, opponent routes, or multiplayer card recording.");
		PanelHotkey = ((BaseUnityPlugin)this).Config.Bind<KeyCode>("Interface", "PanelHotkey", (KeyCode)287, "Key used to open the team panel.");
		_harmony = new Harmony("lbol.pvp.teams");
		_harmony.PatchAll(typeof(BepinexPlugin).Assembly);
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		((BaseUnityPlugin)this).Logger.LogInfo((object)"LBOL PvP battle layer loaded.");
	}

	private void Update()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown(PanelHotkey.Value))
		{
			_showTeamPanel = !_showTeamPanel;
		}
		if (!(Time.unscaledTime < _nextReconcile))
		{
			_nextReconcile = Time.unscaledTime + 1f;
			if (!SoloTestingMode.Value)
			{
				TeamCoordinator.ReconcilePlayers();
			}
		}
	}

	private void OnGUI()
	{
		Act2SoloBattle.DrawIntent();
		if (_showTeamPanel)
		{
			TeamLobbyPanel.Draw();
		}
	}

	private void LateUpdate()
	{
		PlayerIntentDisplay.Tick();
	}

	private void OnDestroy()
	{
		Act2SoloBattle.Clear();
		Harmony harmony = _harmony;
		if (harmony != null)
		{
			harmony.UnpatchSelf();
		}
	}
}
