using AmongUs.Data;
using TMPro;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatMenuPhrasesPagePhraseButton : AbstractQuickChatMenuPhrasesPageButton
{
	[SerializeField]
	[Header("Phrase Button")]
	private QuickChatFavoriteIcon favoriteIcon;

	public override string ButtonType => "Phrase";

	protected override void Awake()
	{
		favoriteIcon.OnPressEvent += OnFavoritePressed;
	}

	protected override float GetInitialWidth()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Collider.size.x + favoriteIcon.Size.x;
	}

	protected override void OnSetup()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2(Width, Height);
		Vector2 sizeDelta = TextRect.sizeDelta;
		sizeDelta.x = Width - favoriteIcon.Size.x;
		TextRect.sizeDelta = sizeDelta;
		float num = val.y - 0.2f;
		TMP_TextInfo textInfo = ((TMP_Text)Text).GetTextInfo(((TMP_Text)Text).text);
		float num2 = 0.2f * (float)textInfo.lineCount + num;
		float num3 = num2 - val.y;
		val.y = num2;
		SpriteRenderer[] array = BackgroundSprites;
		foreach (SpriteRenderer obj in array)
		{
			Vector2 size = obj.size;
			size.y += num3;
			obj.size = size;
			Vector3 localPosition = ((Component)obj).transform.localPosition;
			localPosition.y -= num3 * 0.5f;
			((Component)obj).transform.localPosition = localPosition;
		}
		favoriteIcon.Setup(DataManager.Settings.Multiplayer.HasQuickChatFavorite(Phrase.PhraseKey));
		favoriteIcon.SetHeight(val.y);
		Collider.size = val - new Vector2(favoriteIcon.Size.x, 0f);
		((Collider2D)Collider).offset = new Vector2((val.x + favoriteIcon.Size.x) / 2f, (0f - val.y) / 2f);
		if ((Object)(object)Text != (Object)null)
		{
			TextRect.sizeDelta = Collider.size;
			((TMP_Text)Text).fontSharedMaterial = TextMaterial;
			Vector3 localPosition2 = ((Transform)TextRect).localPosition;
			localPosition2.x = favoriteIcon.Size.x;
			((Transform)TextRect).localPosition = localPosition2;
		}
	}

	public override void SetEnabled(bool enabled)
	{
		base.SetEnabled(enabled);
		((Component)favoriteIcon).gameObject.SetActive(enabled);
	}

	public void ToggleFavorite()
	{
		if (Phrase.Type != QuickChatPhraseType.PlayerId)
		{
			if (DataManager.Settings.Multiplayer.HasQuickChatFavorite(Phrase.PhraseKey))
			{
				QuickChatMenu.Logger.Debug(string.Format("[{0}] Removing QuickChat Favorite: {1}", "AbstractQuickChatMenuPhrasesPageButton", Phrase.PhraseKey));
				DataManager.Settings.Multiplayer.RemoveQuickChatFavorite(Phrase.PhraseKey);
				favoriteIcon.SetFavorite(isFavorite: false);
			}
			else
			{
				QuickChatMenu.Logger.Debug(string.Format("[{0}] Adding QuickChat Favorite: {1}", "AbstractQuickChatMenuPhrasesPageButton", Phrase.PhraseKey));
				DataManager.Settings.Multiplayer.AddQuickChatFavorite(Phrase.PhraseKey);
				favoriteIcon.SetFavorite(isFavorite: true);
			}
			DataManager.Settings.Save();
		}
	}

	private void OnFavoritePressed()
	{
		ToggleFavorite();
	}
}
