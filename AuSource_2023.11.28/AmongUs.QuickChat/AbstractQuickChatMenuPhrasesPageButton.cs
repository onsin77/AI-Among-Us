using TMPro;
using UnityEngine;

namespace AmongUs.QuickChat;

public abstract class AbstractQuickChatMenuPhrasesPageButton : MonoBehaviour
{
	public delegate void ButtonClickedHandler(AbstractQuickChatMenuPhrasesPageButton button);

	private const int MASK_LAYER_INDEX_START = 145;

	[SerializeField]
	private PassiveButton button;

	[SerializeField]
	private BoxCollider2D collider;

	[SerializeField]
	private UIScrollbarHelper scrollHelper;

	[SerializeField]
	[Space(10f)]
	private TextMeshPro text;

	[SerializeField]
	private RectTransform textRect;

	[Space(10f)]
	[SerializeField]
	private SpriteRenderer arrow;

	[SerializeField]
	private SpriteRenderer[] backgroundSprites;

	private QuickChatPhrase phrase;

	private int index;

	private int buttonCount;

	private float width;

	private Material textMaterial;

	public QuickChatPhrase Phrase => phrase;

	public PassiveButton Button => button;

	public UIScrollbarHelper ScrollHelper => scrollHelper;

	public float Width => width;

	public float Height
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return collider.size.y;
		}
	}

	public int MaskLayer => buttonCount - index + 145;

	protected TextMeshPro Text => text;

	protected Material TextMaterial => textMaterial;

	protected RectTransform TextRect => textRect;

	protected BoxCollider2D Collider => collider;

	protected SpriteRenderer Arrow => arrow;

	protected SpriteRenderer[] BackgroundSprites => backgroundSprites;

	public abstract string ButtonType { get; }

	public event ButtonClickedHandler OnSelectedEvent;

	protected virtual void Awake()
	{
	}

	protected abstract void OnSetup();

	protected abstract float GetInitialWidth();

	public void Setup(int index, int buttonCount, float width, QuickChatPhrase phrase, Material textMaterial)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		this.width = width;
		this.phrase = phrase;
		this.index = index;
		this.buttonCount = buttonCount;
		this.textMaterial = textMaterial;
		((TMP_Text)text).text = phrase.ToButtonText();
		scrollHelper.index = index;
		((Object)((Component)this).gameObject).name = ButtonType + " Button: " + phrase.ToPreviewText(null);
		Button.AddOnClickListeners(OnButtonPressed);
		Button.ControllerNav.mode = ControllerNavigation.Mode.Explicit;
		OnSelectedEvent = null;
		SetWidth(width);
		Vector3 localPosition = ((Component)arrow).transform.localPosition;
		localPosition.y = (0f - Height) / 2f;
		((Component)arrow).transform.localPosition = localPosition;
	}

	public void SetWidth(float width)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2(GetInitialWidth(), Height);
		Vector2 val2 = new Vector2(val.x * 0.5f, (0f - val.y) * 0.5f);
		float[] array = new float[backgroundSprites.Length];
		for (int i = 0; i < backgroundSprites.Length; i++)
		{
			SpriteRenderer val3 = backgroundSprites[i];
			array[i] = Mathf.Abs(val3.size.x * 0.5f - val2.x);
		}
		val.x = width;
		val2.x = val.x * 0.5f;
		if (backgroundSprites != null && backgroundSprites.Length != 0)
		{
			for (int j = 0; j < backgroundSprites.Length; j++)
			{
				SpriteRenderer obj = backgroundSprites[j];
				Vector3 localPosition = ((Component)obj).transform.localPosition;
				float num = array[j];
				localPosition.x = val2.x;
				((Component)obj).transform.localPosition = localPosition;
				Vector2 size = obj.size;
				size.x = val.x + num * 2f;
				obj.size = size;
			}
		}
		OnSetup();
	}

	public virtual void SetEnabled(bool enabled)
	{
		Button.SetButtonEnableState(enabled);
	}

	private void OnButtonPressed()
	{
		QuickChatMenu.Logger.Debug("[AbstractQuickChatMenuPhrasesPageButton] Button press DETECTED for phrase: " + ((Object)((Component)this).gameObject).name);
		OnSelectedEvent?.Invoke(this);
	}
}
