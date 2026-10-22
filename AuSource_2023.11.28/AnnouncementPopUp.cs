using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using AmongUs.Data;
using Assets.InnerNet;
using Newtonsoft.Json;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class AnnouncementPopUp : MonoBehaviour
{
	public enum AnnounceState
	{
		NotStarted,
		Fetching,
		Failed,
		Success,
		Cached
	}

	private const float PANEL_HEIGHT = 0.8f;

	private const float SCROLL_OFFSET = 2.512f;

	private MainMenuManager mainMenuManager;

	private static AnnounceState UpdateState;

	[SerializeField]
	private GameObject ConnectIcon;

	[SerializeField]
	private AnnouncementPanel AnnouncementPanelPrefab;

	[SerializeField]
	private TextMeshPro Title;

	[SerializeField]
	private TextMeshPro SubTitle;

	[SerializeField]
	private TextMeshPro DateString;

	[SerializeField]
	private TextMeshPro AnnouncementBodyText;

	[SerializeField]
	private GameObject AnnouncementListSlider;

	[SerializeField]
	private Scroller TextScroller;

	[SerializeField]
	private Scroller ListScroller;

	[SerializeField]
	private ManualScrollHelper ManualScrollHelper;

	[SerializeField]
	private AnnouncementPanel ErrorPanel;

	[SerializeField]
	private GameObject ListStateHUD;

	[SerializeField]
	private GameObject ReadingStateHUD;

	[SerializeField]
	private NewsCountButton NewsCountButton;

	private const string READING_STATE_NAME = "Reading";

	private Vector3 panelStartPos = new Vector3(-3.1f, 0.493f, -5f);

	private int newestAnnouncement;

	private List<AnnouncementPanel> visibleAnnouncements = new List<AnnouncementPanel>();

	private AnnouncementPanel selectedPanel;

	private int selectedIndex;

	private Action OnDismissed;

	private readonly Logger logger = new Logger("Announcement", Logger.Level.Info, Logger.Category.UI);

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement ReadingBackButton;

	private List<SelectableHyperLink> selectableHyperLinks = new List<SelectableHyperLink>();

	private bool readingAnnouncement;

	public event Action ActionOnDisable;

	public event Action ActionOnEnable;

	public void SetMainMenuManager(MainMenuManager mainMenu)
	{
		mainMenuManager = mainMenu;
	}

	private static bool IsSuccess(AnnounceState state)
	{
		if (state != AnnounceState.Success)
		{
			return state == AnnounceState.Cached;
		}
		return true;
	}

	private void OnEnable()
	{
		Debug.Log((object)"Announcement Enabling!");
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton);
		SelectableHyperLinkHelper.AddSelectableUiForHyperlinks(selectableHyperLinks, ((Object)this).name);
		ActionOnEnable?.Invoke();
	}

	private void OnDisable()
	{
		Debug.Log((object)"Announcement Disabling!");
		ControllerManager.Instance.CloseOverlayMenu("Reading");
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		ActionOnDisable?.Invoke();
	}

	public static void ClearAnnouncementCache()
	{
		UpdateState = AnnounceState.NotStarted;
	}

	public IEnumerator ShowIfNew(Action onDismissed = null)
	{
		OnDismissed = onDismissed;
		yield return Init(forceOpen: false);
		if (UpdateState == AnnounceState.Success)
		{
			UpdateState = AnnounceState.Cached;
			SetMenu();
		}
		else if (OnDismissed != null)
		{
			OnDismissed();
		}
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
		((MonoBehaviour)this).StartCoroutine(Init(forceOpen: true));
		UpdateAnnouncementText(newestAnnouncement, ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}

	private void SetMenu()
	{
		if (!mainMenuManager.IsMainMenuActive())
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).gameObject.SetActive(true);
		CreateAnnouncementList();
		UpdateAnnouncementText(newestAnnouncement, ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}

	public void Close()
	{
		DataManager.Player.Save();
		((Component)this).gameObject.SetActive(false);
		if (OnDismissed != null)
		{
			OnDismissed();
		}
	}

	private IEnumerator Init(bool forceOpen)
	{
		while (UpdateState == AnnounceState.Fetching)
		{
			yield return null;
		}
		if (UpdateState > AnnounceState.Fetching && DataManager.Player.Announcements.AllAnnouncements.Count > 0)
		{
			CreateAnnouncementList();
			yield break;
		}
		string announcements = BackendEndpoints.Announcements;
		logger.Info("Requesting announcement from: " + announcements);
		Dictionary<string, object> uriParams = new Dictionary<string, object>
		{
			{
				"lang",
				(uint)DataManager.Settings.Language.CurrentLanguage
			},
			{ "id", -1 }
		};
		string uri = announcements + "?" + HttpUtils.ToUriQuery(uriParams);
		UnityWebRequest request = null;
		bool flag = false;
		int attempts = 3;
		while (!flag && attempts > 0)
		{
			UnityWebRequestAsyncOperation val;
			try
			{
				request = UnityWebRequest.Get(uri);
				request.SetRequestHeader("X-Platform", Constants.GetPlatformType().ToString());
				if (!string.IsNullOrEmpty(DestroyableSingleton<EOSManager>.Instance.UserIDToken))
				{
					request.SetRequestHeader("Authorization", "Bearer " + DestroyableSingleton<EOSManager>.Instance.UserIDToken);
				}
				logger.Debug("Sent Announcement request");
				val = request.SendWebRequest();
			}
			catch (Exception ex)
			{
				UpdateState = AnnounceState.Failed;
				Debug.LogException(ex);
				yield break;
			}
			yield return val;
			flag = (int)request.result == 1;
			attempts--;
		}
		try
		{
			if ((int)request.result == 1)
			{
				Response<AnnouncementList> response = JsonConvert.DeserializeObject<Response<AnnouncementList>>(request.downloadHandler.text);
				DataManager.Player.Announcements.SetAnnouncements(response.Data.Attributes.Announcements);
				int number = DataManager.Player.Announcements.AllAnnouncements[0].Number;
				if (DataManager.Player.Announcements.LatestAnnouncementNumber == number)
				{
					logger.Info($"Announcement doesn't seem to be new {DataManager.Player.Announcements.LatestAnnouncementNumber} == {number}");
					UpdateState = AnnounceState.Cached;
				}
				else
				{
					UpdateState = AnnounceState.Success;
				}
				if (forceOpen)
				{
					SetMenu();
				}
				DataManager.Player.Announcements.LatestAnnouncementNumber = number;
				DataManager.Player.Save();
			}
			else
			{
				logger.Error("Failed to get announcement: " + string.Join("\r\n", request.error));
				UpdateState = AnnounceState.Failed;
			}
		}
		catch (Exception ex2)
		{
			UpdateState = AnnounceState.Failed;
			Debug.LogException(ex2);
			yield break;
		}
		NewsCountButton.UpdateCountText();
		yield return null;
	}

	public void ReturnToList()
	{
		TextScroller.ScrollToTop();
		SelectableHyperLinkHelper.DestroyGOs(selectableHyperLinks, "Reading");
		ControllerManager.Instance.CloseOverlayMenu("Reading");
		readingAnnouncement = false;
		ControllerManager.Instance.SetCurrentSelected(selectedPanel.PassiveButton);
	}

	private IEnumerator DelayedUpdateHyperlinkPositions()
	{
		yield return null;
		if (((Behaviour)this).isActiveAndEnabled)
		{
			SelectableHyperLinkHelper.UpdateHyperlinkPositions(AnnouncementBodyText, selectableHyperLinks, ((Object)this).name);
		}
	}

	private void ClearAnnouncements()
	{
		foreach (AnnouncementPanel visibleAnnouncement in visibleAnnouncements)
		{
			ControllerManager.Instance.RemoveSelectableUiElement(visibleAnnouncement.PassiveButton);
			Object.Destroy((Object)(object)((Component)visibleAnnouncement).gameObject);
		}
	}

	private void CreateAnnouncementList()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected Obj, but got Unknown
		ClearAnnouncements();
		visibleAnnouncements.Clear();
		Vector3 localPosition = panelStartPos;
		bool flag = true;
		if (DataManager.Player.Announcements.AllAnnouncements.Count == 0)
		{
			ShowError();
			return;
		}
		((Component)ErrorPanel).gameObject.SetActive(false);
		foreach (Announcement a in DataManager.Player.Announcements.AllAnnouncements)
		{
			if (a.Number > 0)
			{
				AnnouncementPanel panel = Object.Instantiate<AnnouncementPanel>(AnnouncementPanelPrefab, AnnouncementListSlider.transform);
				panel.SetUp(a);
				((Component)panel).transform.localPosition = localPosition;
				panel.PassiveButton.ClickMask = ListScroller.Hitbox;
				visibleAnnouncements.Add(panel);
				ControllerManager.Instance.AddSelectableUiElement(panel.PassiveButton, flag);
				if (DataManager.Player.Announcements.AnnouncementsRead.Contains(a.Number))
				{
					panel.MarkAsRead();
				}
				if (flag)
				{
					selectedPanel = panel;
					panel.Select();
					flag = false;
					newestAnnouncement = a.Number;
					ControllerManager.Instance.SetCurrentSelected(selectedPanel.PassiveButton);
				}
				((UnityEvent)panel.PassiveButton.OnClick).AddListener((UnityAction)(() =>
				{
					selectedIndex = visibleAnnouncements.IndexOf(panel);
					selectedPanel?.UnSelect();
					selectedPanel = panel;
					selectedPanel.Select();
					UpdateAnnouncementText(a.Number, ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
				}));
				localPosition.y -= 0.8f;
			}
		}
		ListScroller.SetBoundsMax(0.8f * (float)(DataManager.Player.Announcements.AllAnnouncements.Count + 1) - 2.512f, 0f);
	}

	private void ShowError()
	{
		((TMP_Text)Title).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameTypeError);
		((TMP_Text)SubTitle).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AnnouncementErrorSubtitle);
		((TMP_Text)DateString).text = string.Empty;
		((TMP_Text)AnnouncementBodyText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AnnouncementErrorText);
		((Component)ErrorPanel).gameObject.SetActive(true);
	}

	private void UpdateAnnouncementText(int id, bool previewOnly = false)
	{
		Announcement? announcement = null;
		int num = DataManager.Player.Announcements.AllAnnouncements.FindIndex((Announcement an) => an.Number == id);
		announcement = ((num == -1) ? new Announcement?(DataManager.Player.Announcements.AllAnnouncements[0]) : new Announcement?(DataManager.Player.Announcements.AllAnnouncements[num]));
		logger.Debug($"Updating announcement text for announcement ID {id}");
		if (!IsSuccess(UpdateState) && (!announcement.HasValue || announcement.Value.Number == 0))
		{
			announcement = DataManager.Player.Announcements.AllAnnouncements[0];
		}
		if (previewOnly)
		{
			Debug.Log((object)"Announcement Setting preview");
			ListStateHUD.SetActive(true);
			SelectableHyperLinkHelper.SanitizeLinkText(announcement.Value.Text, out var result);
			((TMP_Text)AnnouncementBodyText).text = result;
		}
		else
		{
			Debug.Log((object)"Announcement Setting full text");
			ListStateHUD.SetActive(false);
			ControllerManager.Instance.CloseOverlayMenu("Reading");
			ControllerManager.Instance.OpenOverlayMenu("Reading", ReadingBackButton);
			ControllerManager.Instance.OpenSpecificMenu("Reading");
			readingAnnouncement = true;
			SelectableHyperLinkHelper.DestroyGOs(selectableHyperLinks, "Reading");
			((TMP_Text)AnnouncementBodyText).text = SelectableHyperLinkHelper.DecomposeAnnouncementText(AnnouncementBodyText, selectableHyperLinks, "Reading", announcement.Value.Text);
		}
		((TMP_Text)Title).text = announcement.Value.Title;
		((TMP_Text)SubTitle).text = announcement.Value.SubTitle;
		((TMP_Text)DateString).text = DateTime.Parse(announcement.Value.Date).ToLocalTime().ToString(DestroyableSingleton<TranslationController>.Instance.dateFormats[DataManager.Settings.Language.CurrentLanguage], CultureInfo.InvariantCulture);
		TextScroller.ScrollToTop();
		((Behaviour)ManualScrollHelper).enabled = true;
		((MonoBehaviour)this).StartCoroutine(DelayedUpdateHyperlinkPositions());
	}

	private void OnDestroy()
	{
		SelectableHyperLinkHelper.DestroyGOs(selectableHyperLinks, ((Object)this).name);
	}

	private void Update()
	{
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			if (ReInput.players.GetPlayer(0).GetButtonDown(11))
			{
				UpdateAnnouncementText(selectedPanel.AnnouncementNumber);
			}
			ReadingStateHUD.SetActive(readingAnnouncement);
			ListStateHUD.SetActive(!readingAnnouncement);
		}
		else if (ListStateHUD.activeSelf || ReadingStateHUD.activeSelf)
		{
			ListStateHUD.SetActive(false);
			ReadingStateHUD.SetActive(false);
			UpdateAnnouncementText(selectedPanel.AnnouncementNumber);
		}
		TextScroller.SetBoundsMax(Mathf.Abs(AnnouncementBodyText.GetNotDumbRenderedHeight()), 0f);
		((Behaviour)ManualScrollHelper).enabled = readingAnnouncement;
	}

	public AnnouncementPopUp()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
