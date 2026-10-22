using TMPro;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatMenuVoteTimer : MonoBehaviour
{
	private const float HEIGHT = 0.4f;

	[SerializeField]
	private TextMeshPro text;

	[SerializeField]
	private RectTransform textRect;

	[SerializeField]
	private SpriteRenderer[] backgrounds;

	public float Height => 0.4f;

	public void SetText(StringNames key, int value)
	{
		((TMP_Text)text).text = string.Format(DestroyableSingleton<TranslationController>.Instance.GetString(key), $"<b>{value}</b>");
	}

	public void SetWidth(float width)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = textRect.sizeDelta;
		textRect.sizeDelta = new Vector2(width, sizeDelta.y);
		SpriteRenderer[] array = backgrounds;
		foreach (SpriteRenderer obj in array)
		{
			Vector2 size = obj.size;
			obj.size = new Vector2(width, size.y);
			Vector3 localPosition = ((Component)obj).transform.localPosition;
			localPosition.x = width * 0.5f;
			((Component)obj).transform.localPosition = localPosition;
		}
	}
}
