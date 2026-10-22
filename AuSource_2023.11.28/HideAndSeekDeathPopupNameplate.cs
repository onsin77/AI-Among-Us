using TMPro;
using UnityEngine;

public class HideAndSeekDeathPopupNameplate : MonoBehaviour
{
	[SerializeField]
	[Header("Nameplate")]
	private PoolablePlayer playerIcon;

	[SerializeField]
	private SpriteRenderer background;

	[Header("Text")]
	[SerializeField]
	private TextMeshPro nameText;

	[SerializeField]
	private TextMeshPro levelText;

	public void SetPlayer(PlayerControl player)
	{
		SetPlayer(player.Data);
	}

	public void SetPlayer(GameData.PlayerInfo playerInfo)
	{
		SetMaskLayer(playerInfo.PlayerId);
		background.sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate(playerInfo.DefaultOutfit.NamePlateId).Image;
		playerIcon.UpdateFromPlayerOutfit(playerInfo.Outfits[PlayerOutfitType.Default], PlayerMaterial.MaskType.ComplexUI, isDead: false, includePet: false);
		playerIcon.ToggleName(active: false);
		((TMP_Text)nameText).text = playerInfo.PlayerName;
		((TMP_Text)levelText).text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);
	}

	private void SetMaskLayer(int maskLayer)
	{
		int num = maskLayer + 2;
		SpriteRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).material.SetInt(PlayerMaterial.MaskLayer, num);
		}
		playerIcon.SetMaskLayer(num);
	}
}
