using System;
using System.Collections.Generic;
using AmongUs.Data;
using AmongUs.GameOptions;
using InnerNet;
using TMPro;
using UnityEngine;

public class FindAGameManager : DestroyableSingleton<FindAGameManager>, IGameListHandler
{
	private class GameSorter : IComparer<GameListing>
	{
		public static readonly GameSorter Instance = new GameSorter();

		public int Compare(GameListing x, GameListing y)
		{
			return -x.PlayerCount.CompareTo(y.PlayerCount);
		}
	}

	private const float RefreshTime = 5f;

	private float timer;

	public ObjectPoolBehavior buttonPool;

	public SpinAnimator RefreshSpinner;

	public Scroller TargetArea;

	public TextMeshPro TotalText;

	public float ButtonStart = 1.75f;

	public float ButtonHeight = 0.6f;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public void ResetTimer()
	{
		timer = 5f;
		RefreshSpinner.Appear();
		RefreshSpinner.StartPulse();
	}

	public void Start()
	{
		AmongUsClient.Instance.GameListHandlers.Add(this);
		ResetTimer();
		RefreshList();
		ControllerManager.Instance.NewScene(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
	}

	public void Update()
	{
		timer += Time.deltaTime;
		IGameOptions gameSearchOptions = GameOptionsManager.Instance.GameSearchOptions;
		if ((timer < 0f || timer > 5f) && gameSearchOptions.MapId != 0)
		{
			RefreshSpinner.Appear();
		}
		else
		{
			RefreshSpinner.Disappear();
		}
		if (Input.GetKeyUp((KeyCode)27))
		{
			ExitGame();
		}
	}

	public void RefreshList()
	{
		if (timer >= 5f)
		{
			timer = -5f;
			RefreshSpinner.Play();
			if (DestroyableSingleton<ServerManager>.Instance.IsHttp)
			{
				((MonoBehaviour)this).StartCoroutine(DestroyableSingleton<HttpMatchmakerManager>.Instance.CoRequestGameList(GameOptionsManager.Instance.GameSearchOptions, DataManager.Settings.Multiplayer.SearchGameFilterOptions, HandleList));
			}
			else
			{
				AmongUsClient.Instance.RequestGameList(GameOptionsManager.Instance.GameSearchOptions, DataManager.Settings.Multiplayer.SearchGameFilterOptions);
			}
		}
	}

	public override void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			AmongUsClient.Instance.GameListHandlers.Remove(this);
		}
		base.OnDestroy();
	}

	public void HandleList(InnerNetClient.TotalGameData totalGames, List<GameListing> availableGames)
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		if (totalGames.PerMapTotals != null)
		{
			string text = string.Empty;
			for (int i = 0; i < AmongUsClient.Instance.ShipPrefabs.Count; i++)
			{
				text += $"Total {Constants.MapNames[i]} Games: {totalGames.PerMapTotals[i]}        ";
			}
			text.TrimEnd();
			((TMP_Text)TotalText).text = text;
		}
		try
		{
			RefreshSpinner.Disappear();
			timer = 0f;
			availableGames.Sort(GameSorter.Instance);
			while (buttonPool.activeChildren.Count > availableGames.Count)
			{
				PoolableBehavior poolableBehavior = buttonPool.activeChildren[buttonPool.activeChildren.Count - 1];
				poolableBehavior.OwnerPool.Reclaim(poolableBehavior);
			}
			while (buttonPool.activeChildren.Count < availableGames.Count)
			{
				((Component)buttonPool.Get<PoolableBehavior>()).transform.SetParent(TargetArea.Inner);
			}
			Vector3 val = new Vector3(0f, ButtonStart, -1f);
			for (int j = 0; j < buttonPool.activeChildren.Count; j++)
			{
				MatchMakerGameButton matchMakerGameButton = (MatchMakerGameButton)buttonPool.activeChildren[j];
				matchMakerGameButton.SetGame(availableGames[j]);
				((Component)matchMakerGameButton).transform.localPosition = val;
				val.y -= ButtonHeight;
				PassiveButton component = ((Component)matchMakerGameButton).gameObject.GetComponent<PassiveButton>();
				ControllerManager.Instance.AddSelectableUiElement(component);
			}
			TargetArea.SetBoundsMax(Mathf.Max(0f, 0f - val.y - ButtonStart), 0f);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)"FindAGameManager::HandleList: Exception: ");
			Debug.LogException(ex, (Object)(object)this);
		}
	}

	public void ExitGame()
	{
		AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
	}
}
