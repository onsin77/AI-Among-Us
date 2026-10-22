using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LobbyTimerExtensionUI : MonoBehaviour
{
	[SerializeField]
	private TimerTextTMP timerText;

	[SerializeField]
	private InfoTextBox popup;

	[SerializeField]
	private TimerTextTMP popupTimerText;

	private string timerTextTemplate;

	private string popupTitleText;

	private string popupBodyTextTemplate;

	private void Awake()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected Obj, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected Obj, but got Unknown
		string text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyTimerExpiringUnit);
		string text2 = "<color=red>{0}" + text + "</color>";
		timerTextTemplate = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyTimerExpiringMsg2, text2);
		string text3 = "<color=green>{0}" + text + "</color>";
		popupTitleText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyTimerExpiringMsg, text3);
		popupBodyTextTemplate = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyTimerExpiringMsg2, text2);
		((TMP_Text)popup.button1Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyTimerExpiringNo), true);
		((TMP_Text)popup.button2Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyTimerExpiringOk), true);
		((UnityEvent)popup.button1.OnClick).AddListener((UnityAction)(() =>
		{
			((UnityEventBase)popup.button1.OnClick).RemoveAllListeners();
			((UnityEventBase)popup.button2.OnClick).RemoveAllListeners();
			popup.Close();
		}));
		((UnityEvent)popup.button2.OnClick).AddListener((UnityAction)(() =>
		{
			((UnityEventBase)popup.button1.OnClick).RemoveAllListeners();
			((UnityEventBase)popup.button2.OnClick).RemoveAllListeners();
			LobbyBehaviour.Instance.RpcExtendLobbyTimer();
			popup.Close();
		}));
	}

	public void HideAll()
	{
		HideLobbyTimer();
		popup.Close();
	}

	public void ShowLobbyTimer(int timeRemainingSeconds)
	{
		timerText.SetTimer(timeRemainingSeconds, timerTextTemplate);
		((Component)timerText).gameObject.SetActive(true);
	}

	public void ShowLobbyTimerPopup(int timeRemainingSeconds, int timeGrantedSeconds)
	{
		popupTimerText.SetTimer(timeRemainingSeconds, popupBodyTextTemplate);
		((TMP_Text)popup.titleTexxt).SetText(string.Format(popupTitleText, timeGrantedSeconds), true);
		((Component)popup).gameObject.SetActive(true);
	}

	private void HideLobbyTimer()
	{
		((Component)timerText).gameObject.SetActive(false);
	}
}
