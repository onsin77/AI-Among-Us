using System.Collections.Generic;
using Epic.OnlineServices.Connect;
using InnerNet;
using Steamworks;
using TMPro;
using UnityEngine;

public class FriendsListBar : MonoBehaviour
{
	public TextMeshPro SenderName;

	public List<PassiveButton> ControllerSelectable;

	public ClientData ClientData;

	public List<PassiveButton> Buttons;

	public SpriteRenderer InactiveSprite;

	public PlatformIdentifier PlatformIdentifier;

	public FriendsListManager.PlatformFriend PlatformFriendData;

	public PassiveButton PlatformProfileButton;

	protected float actionCooldown;

	protected string puid;

	protected string friendCode;

	protected FriendsListUI parentUI;

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)PlatformIdentifier))
		{
			((Renderer)PlatformIdentifier.iconRenderer).enabled = true;
		}
		if (PlatformFriendData == null && Object.op_Implicit((Object)(object)PlatformProfileButton))
		{
			((Behaviour)PlatformProfileButton).enabled = false;
		}
	}

	public virtual void SetUp(string puid, FriendsListUI parentUI, string friendCode, string playerInGameName)
	{
		this.parentUI = parentUI;
		this.puid = puid;
		this.friendCode = friendCode;
		((TMP_Text)SenderName).text = ((friendCode == string.Empty) ? playerInGameName : friendCode);
		((TMP_Text)SenderName).fontMaterial.SetFloat("_StencilComp", 3f);
		((TMP_Text)SenderName).fontMaterial.SetFloat("_Stencil", 51f);
	}

	public void GetAndSetPlatform()
	{
		DestroyableSingleton<EOSManager>.Instance.GetExternalAccountInfo(puid, Constants.GetPlatformType(), (ExternalAccountInfo info) =>
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)PlatformIdentifier))
			{
				PlatformIdentifier.SetInfo(Constants.GetPlatformType(), "");
				((Renderer)PlatformIdentifier.iconRenderer).enabled = true;
				if (info == null)
				{
					PlatformIdentifier.iconRenderer.color = Color.white;
					Transform transform = ((Component)PlatformIdentifier.iconRenderer).transform;
					transform.localScale /= 2f;
					PlatformIdentifier.iconRenderer.sprite = DestroyableSingleton<FriendsListManager>.Instance.GlobalPlayerIcon;
					if (((Component)PlatformIdentifier.iconRenderer).transform.childCount > 0)
					{
						((Component)((Component)PlatformIdentifier.iconRenderer).transform.GetChild(0)).gameObject.SetActive(false);
					}
				}
			}
		});
	}

	public void SetPlatform(Platforms p, string name)
	{
		PlatformIdentifier.SetInfo(p, name);
		((Renderer)PlatformIdentifier.iconRenderer).enabled = true;
	}

	private void Update()
	{
		if (actionCooldown > 0f)
		{
			actionCooldown -= Time.unscaledDeltaTime;
		}
	}

	public bool CanUseApi()
	{
		return actionCooldown <= 0f;
	}

	protected void GoInactive()
	{
		((Renderer)InactiveSprite).enabled = true;
		Buttons.ForEach((PassiveButton b) =>
		{
			((Behaviour)b).enabled = false;
		});
	}

	protected void GoActive()
	{
		((Renderer)InactiveSprite).enabled = false;
		Buttons.ForEach((PassiveButton b) =>
		{
			((Behaviour)b).enabled = true;
		});
	}

	public void PlatformViewProfile()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (PlatformFriendData != null)
		{
			Debug.Log((object)("Viewing profile " + PlatformFriendData.GUID));
			if (CanUseApi())
			{
				actionCooldown = 1f;
				SteamFriends.ActivateGameOverlayToUser("steamid", PlatformFriendData.CSteamID);
			}
		}
	}
}
