using InnerNet;
using TMPro;
using UnityEngine;

public class PlayerIdentifierButton : PoolableBehavior
{
	[SerializeField]
	private TextMeshPro NameText;

	[SerializeField]
	private PlatformIdentifier PlatformIdentifier;

	[SerializeField]
	private PoolablePlayer PlayerPreview;

	[SerializeField]
	private SpriteRenderer MaskArea;

	private byte targetPlayerId;

	public void Populate(GameData.PlayerInfo player)
	{
		((TMP_Text)NameText).text = player.PlayerName;
		SetTargetPlayerId(player.PlayerId);
		PlayerPreview.UpdateFromEitherPlayerDataOrCache(player, PlayerOutfitType.Default, PlayerMaterial.MaskType.SimpleUI, includePet: false);
		PlayerPreview.ToggleName(active: false);
	}

	private void SetTargetPlayerId(byte targetId)
	{
		targetPlayerId = targetId;
		if (Object.op_Implicit((Object)(object)PlayerPreview))
		{
			SetMaskLayer(targetId);
		}
		if (Object.op_Implicit((Object)(object)PlatformIdentifier))
		{
			PlayerControl playerControl = PlayerControl.AllPlayerControls.Find((PlayerControl pc) => pc.PlayerId == targetId);
			if (Object.op_Implicit((Object)(object)playerControl))
			{
				ClientData client = AmongUsClient.Instance.GetClient(playerControl.OwnerId);
				PlatformIdentifier.SetInfo(client);
			}
			else
			{
				PlatformIdentifier.SetInfo(null);
			}
		}
	}

	private void SetMaskLayer(int maskLayer)
	{
		PlayerPreview.SetMaskLayer(maskLayer + 2);
		SpriteRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).material.SetInt(PlayerMaterial.MaskLayer, maskLayer + 2);
		}
	}

	public void DisplayProfileInfo()
	{
		PlayerControl playerControl = PlayerControl.AllPlayerControls.Find((PlayerControl pc) => pc.PlayerId == targetPlayerId);
		if (Object.op_Implicit((Object)(object)playerControl))
		{
			AmongUsClient.Instance.GetClient(playerControl.OwnerId);
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (Object.op_Implicit((Object)(object)PlatformIdentifier))
		{
			PlatformIdentifier.SetInfo(null);
		}
	}
}
