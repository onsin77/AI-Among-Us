using AmongUs.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class ChatBubble : PoolableBehavior
{
	private static readonly Vector3 PlayerMessageScale = new Vector3(0.35f, 0.35f, 1f);

	private static readonly Vector3 PlayerNotificationScale = new Vector3(0.25f, 0.25f, 1f);

	private static readonly Vector3 VotedAndDeadMarkerPosition = new Vector3(-0.72f, 0.29f, -0.1f);

	private static readonly Vector3 VotedAndDeadMarkerPositionRight = new Vector3(0.72f, 0.29f, -0.1f);

	public PoolablePlayer Player;

	public SpriteRenderer Xmark;

	public SpriteRenderer votedMark;

	public TextMeshPro NameText;

	public TextMeshPro TextArea;

	public SpriteRenderer Background;

	public SpriteRenderer MaskArea;

	public PlatformIdentifier PlatformIcon;

	public TextMeshPro ColorBlindName;

	private GameData.PlayerInfo playerInfo;

	private int maskLayer;

	public void SetLeft()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = new Vector3(-3f, 0f, 0f);
		Player.SetFlipX(flipped: false);
		((Component)Player).transform.localScale = PlayerMessageScale;
		((Component)Player).transform.localPosition = new Vector3(0.04f, -0.08f, 0f);
		((Component)Xmark).transform.localPosition = VotedAndDeadMarkerPosition;
		((Component)votedMark).transform.localPosition = VotedAndDeadMarkerPosition;
		((TMP_Text)NameText).rectTransform.pivot = new Vector2(0f, 1f);
		NameText.transform.localPosition = new Vector3(0.5f, 0.358f, 0f);
		((TMP_Text)NameText).horizontalAlignment = (HorizontalAlignmentOptions)1;
		((TMP_Text)TextArea).rectTransform.pivot = new Vector2(0f, 1f);
		TextArea.transform.localPosition = new Vector3(0.5f, 0.125f, 0f);
		((TMP_Text)TextArea).horizontalAlignment = (HorizontalAlignmentOptions)1;
		((TMP_Text)ColorBlindName).rectTransform.pivot = new Vector2(0f, 1f);
		ColorBlindName.transform.localPosition = new Vector3(-0.22f, -0.071f, -0.5f);
	}

	public void SetNotification()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = new Vector3(-2.75f, 0f, 0f);
		Player.SetFlipX(flipped: false);
		((Component)Player).transform.localScale = PlayerNotificationScale;
		((Component)Player).transform.localPosition = new Vector3(0f, 0.06f, 0f);
		((Component)Xmark).transform.localPosition = VotedAndDeadMarkerPosition;
		((Component)votedMark).transform.localPosition = VotedAndDeadMarkerPosition;
		((TMP_Text)NameText).rectTransform.pivot = new Vector2(0f, 1f);
		NameText.transform.localPosition = new Vector3(0.5f, 0.358f, 0f);
		((TMP_Text)NameText).horizontalAlignment = (HorizontalAlignmentOptions)1;
		((TMP_Text)TextArea).rectTransform.pivot = new Vector2(0f, 1f);
		TextArea.transform.localPosition = new Vector3(0.5f, 0.125f, 0f);
		((TMP_Text)TextArea).horizontalAlignment = (HorizontalAlignmentOptions)1;
		((TMP_Text)ColorBlindName).rectTransform.pivot = new Vector2(0f, 1f);
		ColorBlindName.transform.localPosition = new Vector3(-0.22f, 0.12f, -0.5f);
		((TMP_Text)TextArea).text = string.Empty;
	}

	public void SetWarning(string text)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		((Component)Player).gameObject.SetActive(false);
		((Component)NameText).gameObject.SetActive(false);
		((Component)ColorBlindName).gameObject.SetActive(false);
		((Graphic)TextArea).color = Color.red;
		SetText(text);
		((TMP_Text)TextArea).rectTransform.pivot = new Vector2(0f, 0f);
		TextArea.transform.localPosition = new Vector3(-0.25f, -0.15f, 0f);
		((TMP_Text)TextArea).horizontalAlignment = (HorizontalAlignmentOptions)1;
	}

	public void SetCosmetics(GameData.PlayerInfo playerInfo)
	{
		this.playerInfo = playerInfo;
		if (((Component)this).gameObject.activeInHierarchy)
		{
			Player.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ScrollingUI, includePet: false);
		}
		Player.ToggleName(active: false);
		maskLayer = 51 + PoolIndex;
		SetMaskLayer();
		SetColorblindText();
	}

	public void SetRight()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = new Vector3(-2.35f, 0f, 0f);
		Player.SetFlipX(flipped: true);
		((Component)Player).transform.localScale = PlayerMessageScale;
		((Component)Player).transform.localPosition = new Vector3((Player.CurrentBodyType == PlayerBodyTypes.Horse) ? 5.25f : 4.75f, -0.08f, 0f);
		((Component)Xmark).transform.localPosition = VotedAndDeadMarkerPositionRight;
		((Component)votedMark).transform.localPosition = VotedAndDeadMarkerPositionRight;
		((TMP_Text)NameText).rectTransform.pivot = new Vector2(1f, 1f);
		NameText.transform.localPosition = new Vector3(4.35f, 0.358f, 0f);
		((TMP_Text)NameText).horizontalAlignment = (HorizontalAlignmentOptions)4;
		((TMP_Text)TextArea).rectTransform.pivot = new Vector2(1f, 1f);
		TextArea.transform.localPosition = new Vector3(4.35f, 0.125f, 0f);
		((TMP_Text)TextArea).horizontalAlignment = (HorizontalAlignmentOptions)4;
		((TMP_Text)ColorBlindName).rectTransform.pivot = new Vector2(1f, 1f);
		ColorBlindName.transform.localPosition = new Vector3(4.98f, -0.071f, -0.5f);
	}

	public void SetName(string playerName, bool isDead, bool voted, Color color)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)NameText).text = playerName ?? "...";
		((Graphic)NameText).color = color;
		((TMP_Text)NameText).ForceMeshUpdate(true, true);
		if (isDead)
		{
			((Renderer)Xmark).enabled = true;
			Background.color = Palette.HalfWhite;
		}
		if (voted)
		{
			((Renderer)votedMark).enabled = true;
		}
	}

	public override void Reset()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		((Component)Player).gameObject.SetActive(true);
		((Component)NameText).gameObject.SetActive(true);
		((Renderer)Xmark).enabled = false;
		((Renderer)votedMark).enabled = false;
		Background.color = Color.white;
		((Graphic)TextArea).color = Color.black;
		playerInfo = null;
	}

	public void AlignChildren()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)Background).transform.localPosition;
		localPosition.y = NameText.transform.localPosition.y - Background.size.y / 2f + 0.05f;
		((Component)Background).transform.localPosition = localPosition;
		((Component)MaskArea).transform.localPosition = localPosition + new Vector3(0f, 0.02f, 0f);
	}

	internal void SetText(string chatText)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)TextArea).text = chatText;
		((TMP_Text)TextArea).ForceMeshUpdate(true, true);
		Background.size = new Vector2(5.52f, 0.2f + NameText.GetNotDumbRenderedHeight() + TextArea.GetNotDumbRenderedHeight());
		MaskArea.size = Background.size - new Vector2(0f, 0.03f);
	}

	private void OnEnable()
	{
		if (playerInfo != null)
		{
			Player.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ScrollingUI, includePet: false);
			SetMaskLayer();
			SetColorblindText();
		}
		DataManager.Settings.Accessibility.OnColorBlindModeChanged += SetColorblindText;
	}

	private void OnDisable()
	{
		DataManager.Settings.Accessibility.OnColorBlindModeChanged -= SetColorblindText;
	}

	private void SetColorblindText()
	{
		((Behaviour)ColorBlindName).enabled = DataManager.Settings.Accessibility.ColorBlindMode;
		((TMP_Text)ColorBlindName).text = Player.ColorBlindName;
	}

	private void SetMaskLayer()
	{
		((Renderer)MaskArea).material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		Player.SetMaskLayer(maskLayer);
		((Renderer)Background).material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
	}

	static ChatBubble()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
	}
}
