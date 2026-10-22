using System;
using System.Collections;
using AmongUs.Data;
using UnityEngine;

public class EndGameNavigation : MonoBehaviour
{
	public ProgressionScreen ProgressionScreen;

	public EndGameManager EndGameManager;

	[Header("Buttons")]
	public GameObject ContinueButton;

	public SpriteRenderer PlayAgainButton;

	public SpriteRenderer ExitButton;

	public void ShowProgression()
	{
		ProgressionScreen.Activate();
		((Component)EndGameManager).gameObject.SetActive(false);
		ContinueButton.SetActive(false);
		ShowDefaultNavigation();
	}

	public void ShowDefaultNavigation()
	{
		((Component)PlayAgainButton).gameObject.SetActive(true);
		((Component)ExitButton).gameObject.SetActive(true);
	}

	public void ShowNavigationToProgressionScreen()
	{
		ContinueButton.SetActive(true);
	}

	public void HideButtons()
	{
		ContinueButton.SetActive(false);
	}

	public void NextGame()
	{
		((Component)PlayAgainButton).gameObject.SetActive(false);
		((Component)ExitButton).gameObject.SetActive(false);
		if (TempData.showAd && !DataManager.Player.Ads.HasPurchasedAdRemoval)
		{
			TempData.showAd = false;
		}
		GameManager.DestroyInstance();
		((MonoBehaviour)this).StartCoroutine(CoJoinGame());
	}

	public void Exit()
	{
		((Component)PlayAgainButton).gameObject.SetActive(false);
		((Component)ExitButton).gameObject.SetActive(false);
		GameManager.DestroyInstance();
		AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
	}

	private static IEnumerator WaitWithTimeout(Func<bool> success)
	{
		for (float timer = 0f; timer < 5f; timer += Time.deltaTime)
		{
			if (success())
			{
				break;
			}
			yield return null;
		}
	}

	private IEnumerator CoJoinGame()
	{
		AmongUsClient.Instance.JoinGame();
		yield return WaitWithTimeout(() => AmongUsClient.Instance.ClientId >= 0);
		if (AmongUsClient.Instance.ClientId < 0)
		{
			AmongUsClient.Instance.ExitGame(AmongUsClient.Instance.LastDisconnectReason);
		}
	}
}
