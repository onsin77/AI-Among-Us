using System.Collections;
using System.Linq;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatMenuPhrasesPage : AbstractQuickChatMenuPage
{
	public delegate void PhraseSelectedHandler(QuickChatPhrase phrase);

	private const float BOTTOM_PADDING = 0.1f;

	private static readonly Color32 TEXT_COLOR_ENABLED = Color32.op_Implicit(Color.white);

	private static readonly Color32 TEXT_COLOR_DISABLED = new Color32((byte)123, (byte)123, (byte)123, byte.MaxValue);

	[SerializeField]
	[Header("UI")]
	private QuickChatMenuScroller scroller;

	[Header("Button Templates")]
	[SerializeField]
	private QuickChatMenuPhrasesPagePhraseButton phraseButtonTemplate;

	[SerializeField]
	private QuickChatMenuPhrasesPageCrewmateButton crewmateButtonTemplate;

	[SerializeField]
	private Material defaultButtonTextMaterial;

	[Header("Input")]
	[SerializeField]
	private ControllerButtonBehavior favoriteButton;

	private AbstractQuickChatMenuPhrasesPageButton[] dynamicButtons;

	private QuickChatTag[] tags;

	private QuickChatPhrase[] phrases;

	private Material buttonTextMaterial;

	public QuickChatTag[] Tags => tags;

	public QuickChatPhrase[] Phrases => phrases;

	public event PhraseSelectedHandler OnPhraseSelectedEvent;

	private void Awake()
	{
		((Component)phraseButtonTemplate).gameObject.SetActive(false);
		((Component)crewmateButtonTemplate).gameObject.SetActive(false);
		favoriteButton.OnPressedEvent += OnFavoriteButtonPressed;
	}

	public void ShowTagPage(float height, params QuickChatTag[] tags)
	{
		Show(height - 0.1f);
		((Component)this).gameObject.SetActive(true);
		this.tags = tags;
		phrases = (from p in Menu.Data.GetPhrases(QuickChatContext.Current(Menu.Data), tags)
			where Menu.CanAddPhraseToBuilder(p)
			select p).ToArray();
		SetupContent();
		if (dynamicButtons != null && dynamicButtons.Length != 0)
		{
			AbstractQuickChatMenuPhrasesPageButton[] array = dynamicButtons;
			foreach (AbstractQuickChatMenuPhrasesPageButton abstractQuickChatMenuPhrasesPageButton in array)
			{
				ControllerManager.Instance.AddSelectableUiElement(abstractQuickChatMenuPhrasesPageButton.Button);
			}
		}
		((MonoBehaviour)this).StartCoroutine(CoDelayedSelectFirstButton());
	}

	public override void Hide()
	{
		base.Hide();
		((Component)this).gameObject.SetActive(false);
		ClearDynamicButtons();
	}

	public void SetPhrasesEnableState(bool enabled)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		buttonTextMaterial.SetColor("_FaceColor", Color32.op_Implicit(enabled ? TEXT_COLOR_ENABLED : TEXT_COLOR_DISABLED));
		AbstractQuickChatMenuPhrasesPageButton[] array = dynamicButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetEnabled(enabled);
		}
	}

	private void SetupContent()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		ClearDynamicButtons();
		dynamicButtons = new AbstractQuickChatMenuPhrasesPageButton[Phrases.Length];
		buttonTextMaterial = new Material(defaultButtonTextMaterial);
		float num = Width - 0.2f;
		float num2 = 0f;
		for (int i = 0; i < Phrases.Length; i++)
		{
			QuickChatPhrase phrase = Phrases[i];
			AbstractQuickChatMenuPhrasesPageButton abstractQuickChatMenuPhrasesPageButton = CreatePhraseButton(i, Phrases.Length, num, phrase);
			abstractQuickChatMenuPhrasesPageButton.OnSelectedEvent += OnButtonSelected;
			dynamicButtons[i] = abstractQuickChatMenuPhrasesPageButton;
			Vector3 localPosition = ((Component)abstractQuickChatMenuPhrasesPageButton).transform.localPosition;
			localPosition.y = num2;
			localPosition.z = -i;
			((Component)abstractQuickChatMenuPhrasesPageButton).transform.localPosition = localPosition;
			num2 = ((Component)abstractQuickChatMenuPhrasesPageButton).transform.localPosition.y - abstractQuickChatMenuPhrasesPageButton.Height - 0.1f;
			if (i > 0)
			{
				dynamicButtons[i - 1].Button.ControllerNav.selectOnDown = abstractQuickChatMenuPhrasesPageButton.Button;
				abstractQuickChatMenuPhrasesPageButton.Button.ControllerNav.selectOnUp = dynamicButtons[i - 1].Button;
			}
		}
		if (dynamicButtons.Length > 1)
		{
			dynamicButtons.First().Button.ControllerNav.selectOnUp = dynamicButtons.Last().Button;
			dynamicButtons.Last().Button.ControllerNav.selectOnDown = dynamicButtons.First().Button;
		}
		scroller.SetSize(Width, Height);
		scroller.SetContentHeight(Mathf.Abs(num2));
	}

	private void OnButtonSelected(AbstractQuickChatMenuPhrasesPageButton button)
	{
		QuickChatMenu.Logger.Debug("[QuickChatMenuPhrasesPage] Button press RECEIVED for phrase: " + ((Object)((Component)button).gameObject).name);
		OnPhraseSelectedEvent?.Invoke(button.Phrase);
	}

	private void ClearDynamicButtons()
	{
		if (dynamicButtons == null || dynamicButtons.Length < 1)
		{
			return;
		}
		AbstractQuickChatMenuPhrasesPageButton[] array = dynamicButtons;
		foreach (AbstractQuickChatMenuPhrasesPageButton abstractQuickChatMenuPhrasesPageButton in array)
		{
			if (!((Object)(object)abstractQuickChatMenuPhrasesPageButton == (Object)null))
			{
				if ((Object)(object)ControllerManager.Instance != (Object)null && (Object)(object)abstractQuickChatMenuPhrasesPageButton.Button != (Object)null)
				{
					ControllerManager.Instance.RemoveSelectableUiElement(abstractQuickChatMenuPhrasesPageButton.Button);
				}
				if ((Object)(object)((Component)abstractQuickChatMenuPhrasesPageButton).gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)abstractQuickChatMenuPhrasesPageButton).gameObject);
				}
			}
		}
		dynamicButtons = null;
	}

	private AbstractQuickChatMenuPhrasesPageButton CreatePhraseButton(int index, int buttonCount, float width, QuickChatPhrase phrase)
	{
		AbstractQuickChatMenuPhrasesPageButton abstractQuickChatMenuPhrasesPageButton = null;
		abstractQuickChatMenuPhrasesPageButton = ((phrase.Type != QuickChatPhraseType.PlayerId) ? ((AbstractQuickChatMenuPhrasesPageButton)CreatePhraseButtonOfType(phraseButtonTemplate)) : ((AbstractQuickChatMenuPhrasesPageButton)CreatePhraseButtonOfType(crewmateButtonTemplate)));
		abstractQuickChatMenuPhrasesPageButton.Setup(index, buttonCount, width, phrase, buttonTextMaterial);
		return abstractQuickChatMenuPhrasesPageButton;
	}

	private T CreatePhraseButtonOfType<T>(T template) where T : AbstractQuickChatMenuPhrasesPageButton
	{
		((Component)template).gameObject.SetActive(true);
		T result = Object.Instantiate<T>(template, ((Component)template).transform.parent, true);
		((Component)template).gameObject.SetActive(false);
		return result;
	}

	private void OnFavoriteButtonPressed()
	{
		AbstractQuickChatMenuPhrasesPageButton[] array = dynamicButtons;
		foreach (AbstractQuickChatMenuPhrasesPageButton abstractQuickChatMenuPhrasesPageButton in array)
		{
			if (abstractQuickChatMenuPhrasesPageButton is QuickChatMenuPhrasesPagePhraseButton quickChatMenuPhrasesPagePhraseButton && (Object)(object)ControllerManager.Instance.CurrentUiState.CurrentSelection == (Object)(object)abstractQuickChatMenuPhrasesPageButton.Button)
			{
				quickChatMenuPhrasesPagePhraseButton.ToggleFavorite();
			}
		}
	}

	private IEnumerator CoDelayedSelectFirstButton()
	{
		if (dynamicButtons != null && dynamicButtons.Length != 0)
		{
			ControllerManager.Instance.SetCurrentSelected(dynamicButtons[0].Button);
			yield return null;
			ControllerManager.Instance.SetCurrentSelected(dynamicButtons[0].Button);
		}
	}

	static QuickChatMenuPhrasesPage()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
	}
}
