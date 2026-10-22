using AmongUs.Data;
using TMPro;
using UnityEngine;

public class LogEntryBubble : PoolableBehavior
{
	[SerializeField]
	private SpriteRenderer background;

	[SerializeField]
	private SpriteRenderer maskArea;

	[SerializeField]
	private PoolablePlayer player;

	[SerializeField]
	private TextMeshPro text;

	public void DecorateLog(SecurityLogBehaviour.SecurityLogEntry entry, Sprite backgroundSprite)
	{
		GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(entry.PlayerId);
		if (playerById == null)
		{
			Debug.LogError((object)$"Couldn't find player {entry.PlayerId} for log");
			return;
		}
		SetCosmetics(playerById);
		SetText(entry, playerById);
		SetBackground(backgroundSprite);
	}

	public void SetMaskLayer(int maskLayer)
	{
		((Renderer)maskArea).material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		player.SetMaskLayer(maskLayer);
		((Renderer)background).material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
	}

	private void SetBackground(Sprite backgroundSprite)
	{
		background.sprite = backgroundSprite;
	}

	private void SetCosmetics(GameData.PlayerInfo playerData)
	{
		player.UpdateFromPlayerData(playerData, PlayerOutfitType.Default, PlayerMaterial.MaskType.ScrollingUI, includePet: false, null, forceAlive: true);
		player.ToggleName(active: false);
	}

	private void SetText(SecurityLogBehaviour.SecurityLogEntry entry, GameData.PlayerInfo playerData)
	{
		string text = DestroyableSingleton<TranslationController>.Instance.GetString((StringNames)(201 + entry.Location));
		int colorId = playerData.DefaultOutfit.ColorId;
		string value = ((TMP_Text)this.text).text;
		if (DataManager.Settings.Accessibility.ColorBlindMode)
		{
			string text2 = DestroyableSingleton<TranslationController>.Instance.GetString(Palette.ColorNames[colorId]).ToLower();
			((TMP_Text)this.text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecLogEntryColorblind, playerData.PlayerName, text2, text);
		}
		else
		{
			((TMP_Text)this.text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecLogEntry, playerData.PlayerName, text);
		}
		if (!((TMP_Text)this.text).text.Equals(value))
		{
			((TMP_Text)this.text).ForceMeshUpdate(false, false);
		}
	}
}
