using AmongUs.Data;
using TMPro;
using UnityEngine;

public class VitalsPanel : MonoBehaviour
{
	public PoolablePlayer PlayerIcon;

	public SpriteRenderer MaskingArea;

	public SpriteRenderer Background;

	public VertLineBehaviour Cardio;

	public Sprite VitalBgDead;

	public Sprite VitalBgDiscon;

	public IntRange BeatRange;

	public bool IsDead;

	public bool IsDiscon;

	public TextMeshPro ColorBlindName;

	public GameData.PlayerInfo PlayerInfo { get; set; }

	public void SetPlayer(int index, GameData.PlayerInfo playerInfo)
	{
		PlayerInfo = playerInfo;
		PlayerIcon.SetFlipX(flipped: false);
		PlayerIcon.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, includePet: false);
		PlayerIcon.ToggleName(active: false);
		((TMP_Text)ColorBlindName).text = PlayerIcon.ColorBlindName;
		((Behaviour)ColorBlindName).enabled = DataManager.Settings.Accessibility.ColorBlindMode;
		((Renderer)MaskingArea).material.SetInt(PlayerMaterial.MaskLayer, index + 2);
		PlayerIcon.SetMaskLayer(index + 2);
	}

	public void SetDisconnected()
	{
		IsDead = false;
		IsDiscon = true;
		Background.sprite = VitalBgDiscon;
		((Component)Cardio).gameObject.SetActive(false);
	}

	public void SetDead()
	{
		IsDiscon = false;
		IsDead = true;
		Background.sprite = VitalBgDead;
		Cardio.SetDead();
	}

	public void SetAlive()
	{
		Cardio.Offset = IntRange.Next(0, 64);
		Cardio.beats = BeatRange.Next();
		Cardio.SetAlive();
	}
}
