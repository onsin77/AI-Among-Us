using AmongUs.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FriendsListButton : MonoBehaviour
{
	public GameObject Button;

	public AspectPosition AspectPosition;

	public Vector3 NormalPosition;

	public Vector3 LobbyPosition;

	public TextMeshPro NotifCount;

	public GameObject NotifCircle;

	public GameObject Tab;

	public Collider2D TabCollider;

	public Collider2D NormalCollider;

	[SerializeField]
	private ActionMapGlyphDisplay gamepadButton;

	private bool showInScene;

	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		showInScene = scene.name == "MainMenu" || scene.name == "MatchMaking" || scene.name == "MMOnline" || scene.name == "OnlineGame";
		if (scene.name == "OnlineGame")
		{
			Tab.SetActive(false);
			((Behaviour)NormalCollider).enabled = true;
			((Behaviour)TabCollider).enabled = false;
			((Component)this).gameObject.transform.SetParent(((Component)DestroyableSingleton<HudManager>.Instance).transform);
			((Component)DestroyableSingleton<FriendsListManager>.Instance.Ui).gameObject.transform.SetParent(((Component)DestroyableSingleton<HudManager>.Instance).transform);
			((Component)DestroyableSingleton<FriendsListManager>.Instance.ConfirmationScreen).gameObject.transform.SetParent(((Component)DestroyableSingleton<HudManager>.Instance).transform);
			AspectPosition.DistanceFromEdge = LobbyPosition;
			AspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
		}
		else
		{
			((Behaviour)NormalCollider).enabled = false;
			((Behaviour)TabCollider).enabled = true;
			Tab.SetActive(true);
			AspectPosition.DistanceFromEdge = NormalPosition;
			AspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
		}
		((Component)this).gameObject.transform.position = Vector3.zero;
	}

	private void Update()
	{
		if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.Offline || !showInScene || (Object)(object)ShipStatus.Instance != (Object)null)
		{
			Button.SetActive(false);
		}
		else
		{
			Button.SetActive(true);
		}
	}

	public void CheckNotifCount()
	{
		int num = DestroyableSingleton<FriendsListManager>.Instance.ReceivedLobbyInvites.Count + DestroyableSingleton<FriendsListManager>.Instance.ReceivedRequests.Count;
		if (num > 0 && DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
		{
			NotifCircle.SetActive(true);
			((TMP_Text)NotifCount).text = ((num > 9) ? "9+" : num.ToString());
		}
		else
		{
			NotifCircle.SetActive(false);
		}
	}

	public void SetGlyphColor(bool isGrayedOut)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (isGrayedOut)
		{
			gamepadButton.SetColor(Palette.DisabledClear);
		}
		else
		{
			gamepadButton.SetColor(Palette.EnabledColor);
		}
	}
}
