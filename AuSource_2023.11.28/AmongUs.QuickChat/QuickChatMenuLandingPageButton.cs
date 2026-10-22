using TMPro;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatMenuLandingPageButton : MonoBehaviour
{
	[SerializeField]
	private PassiveButton button;

	[SerializeField]
	private BoxCollider2D collider;

	[SerializeField]
	private UIScrollbarHelper scrollHelper;

	[SerializeField]
	[Space(10f)]
	private TMP_Text text;

	[SerializeField]
	private RectTransform textRect;

	[SerializeField]
	private TextTranslatorTMP translator;

	[Space(10f)]
	[SerializeField]
	private SpriteRenderer[] backgroundSprites;

	[SerializeField]
	private SpriteRenderer[] shineSprites;

	private QuickChatTag tag;

	public QuickChatTag Tag => tag;

	public PassiveButton Button => button;

	public UIScrollbarHelper ScrollHelper => scrollHelper;

	public float Width
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return collider.size.x;
		}
	}

	public float Height
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return collider.size.y;
		}
	}

	public StringNames TranslationKey => translator.TargetText;

	public virtual void Setup(int index, QuickChatTag tag)
	{
		this.tag = tag;
		((Object)((Component)this).gameObject).name = $"Category Button: {tag.TagNameKey}";
		SetStringName(tag.TagNameKey);
		Button.ControllerNav.mode = ControllerNavigation.Mode.Explicit;
		if ((Object)(object)scrollHelper != (Object)null)
		{
			scrollHelper.index = index;
		}
	}

	public void SetStringName(StringNames key)
	{
		translator.TargetText = key;
		translator.ResetText();
	}

	public void SetWidth(float width)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		Vector2 size = collider.size;
		Vector2 offset = ((Collider2D)collider).offset;
		float[] array = new float[backgroundSprites.Length];
		for (int i = 0; i < backgroundSprites.Length; i++)
		{
			SpriteRenderer val = backgroundSprites[i];
			array[i] = Mathf.Abs(val.size.x * 0.5f - offset.x);
		}
		size.x = width;
		offset.x = size.x * 0.5f;
		if (shineSprites != null && shineSprites.Length != 0)
		{
			SpriteRenderer[] array2 = shineSprites;
			foreach (SpriteRenderer obj in array2)
			{
				Vector3 localPosition = ((Component)obj).transform.localPosition;
				localPosition.x = width * 0.5f;
				((Component)obj).transform.localPosition = localPosition;
			}
		}
		if (backgroundSprites != null && backgroundSprites.Length != 0)
		{
			for (int k = 0; k < backgroundSprites.Length; k++)
			{
				SpriteRenderer obj2 = backgroundSprites[k];
				Vector3 localPosition2 = ((Component)obj2).transform.localPosition;
				float num = array[k];
				localPosition2.x = offset.x;
				((Component)obj2).transform.localPosition = localPosition2;
				Vector2 size2 = obj2.size;
				size2.x = size.x + num * 2f;
				obj2.size = size2;
			}
		}
		if ((Object)(object)text != (Object)null)
		{
			Vector2 sizeDelta = textRect.sizeDelta;
			sizeDelta.x = width;
			textRect.sizeDelta = sizeDelta;
		}
		collider.size = size;
		((Collider2D)collider).offset = offset;
	}
}
