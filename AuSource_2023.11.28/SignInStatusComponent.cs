using AmongUs.Data;
using TMPro;
using UnityEngine;

public class SignInStatusComponent : MonoBehaviour
{
	private const string FRIEND_CODE_HIDDEN_TEXT = "XXXXXXXXXX";

	[SerializeField]
	private SpriteRenderer statusSprite;

	[SerializeField]
	private SpriteRenderer glowSprite;

	[SerializeField]
	private TextMeshPro friendCodeText;

	[SerializeField]
	private SpriteRenderer FriendCodeHiddenIcon;

	[SerializeField]
	private GameObject friendCodeShowToggle;

	[SerializeField]
	private GameObject friendsButton;

	[Header("Lights")]
	[SerializeField]
	private Sprite onlineSprite;

	[SerializeField]
	private Sprite pendingSprite;

	[SerializeField]
	private Sprite guestSprite;

	[SerializeField]
	private Sprite offlineSprite;

	[Header("Glows")]
	[SerializeField]
	private Sprite onlineGlow;

	[SerializeField]
	private Sprite pendingGlow;

	[SerializeField]
	private Sprite guestGlow;

	public void SetOnline()
	{
		statusSprite.sprite = onlineSprite;
		glowSprite.sprite = onlineGlow;
		friendsButton.SetActive(true);
	}

	public void SetPending()
	{
		statusSprite.sprite = pendingSprite;
		glowSprite.sprite = pendingGlow;
		friendsButton.SetActive(true);
	}

	public void SetGuest()
	{
		statusSprite.sprite = guestSprite;
		glowSprite.sprite = guestGlow;
		friendsButton.SetActive(true);
	}

	public void SetOffline()
	{
		statusSprite.sprite = offlineSprite;
		glowSprite.sprite = null;
		friendsButton.SetActive(false);
	}

	public void ToggleFriendCode()
	{
		if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn)
		{
			DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden = !DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden;
		}
	}

	private void Update()
	{
		UpdateFriendCodeUI();
	}

	private void UpdateFriendCodeUI()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
		{
			((Renderer)FriendCodeHiddenIcon).enabled = DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden;
			friendCodeShowToggle.SetActive(true);
			((Behaviour)friendCodeText).enabled = true;
			((TMP_Text)friendCodeText).text = DestroyableSingleton<EOSManager>.Instance.FriendCode;
			if (DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden)
			{
				((TMP_Text)friendCodeText).text = "XXXXXXXXXX";
			}
			else
			{
				((TMP_Text)friendCodeText).text = DestroyableSingleton<EOSManager>.Instance.FriendCode;
			}
		}
		else
		{
			((Renderer)FriendCodeHiddenIcon).enabled = false;
			friendCodeShowToggle.SetActive(false);
			((Behaviour)friendCodeText).enabled = false;
		}
	}
}
