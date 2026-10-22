using System.Linq;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatMenuLandingPage : AbstractQuickChatMenuPage
{
	public delegate void OnButtonPressedHandler(QuickChatMenuLandingPageButton button);

	[SerializeField]
	private QuickChatMenuLandingPageButton favoritesButton;

	[SerializeField]
	private QuickChatMenuLandingPageButton remarksButton;

	[SerializeField]
	private Transform contentBlocker;

	[SerializeField]
	[Header("List")]
	private Transform listContainer;

	[SerializeField]
	private QuickChatMenuScroller scroller;

	[SerializeField]
	private QuickChatMenuLandingPageButton buttonTemplate;

	private QuickChatMenuLandingPageButton[] dynamicButtons;

	public override PassiveButton DefaultSelection => remarksButton.Button;

	public event OnButtonPressedHandler OnButtonPressed;

	public override void Initialize(QuickChatMenu menu, float width)
	{
		base.Initialize(menu, width);
		remarksButton.Button.AddOnClickListeners(OnRemarksButtonClicked);
		favoritesButton.Button.AddOnClickListeners(OnFavoritesButtonClicked);
		SetupContent();
	}

	public override void Show(float height)
	{
		base.Show(height);
		ControllerManager.Instance.AddSelectableUiElement(remarksButton.Button);
		ControllerManager.Instance.AddSelectableUiElement(favoritesButton.Button);
		SetupContent();
		QuickChatMenuLandingPageButton[] array = dynamicButtons;
		foreach (QuickChatMenuLandingPageButton quickChatMenuLandingPageButton in array)
		{
			ControllerManager.Instance.AddSelectableUiElement(quickChatMenuLandingPageButton.Button);
		}
		ControllerManager.Instance.SetCurrentSelected(remarksButton.Button);
	}

	public override void Hide()
	{
		base.Hide();
		ControllerManager.Instance.RemoveSelectableUiElement(remarksButton.Button);
		ControllerManager.Instance.RemoveSelectableUiElement(favoritesButton.Button);
		QuickChatMenuLandingPageButton[] array = dynamicButtons;
		foreach (QuickChatMenuLandingPageButton quickChatMenuLandingPageButton in array)
		{
			ControllerManager.Instance.RemoveSelectableUiElement(quickChatMenuLandingPageButton.Button);
		}
	}

	private void SetupContent()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		favoritesButton.Setup(0, Menu.Data.Tags.Favorite);
		remarksButton.Setup(0, Menu.Data.Tags.Remark);
		Vector3 localPosition = ((Component)remarksButton).transform.localPosition;
		localPosition.x = favoritesButton.Width + 0.1f;
		((Component)remarksButton).transform.localPosition = localPosition;
		float num = Width - localPosition.x;
		remarksButton.SetWidth(num);
		remarksButton.Button.ControllerNav.selectOnLeft = favoritesButton.Button;
		remarksButton.Button.ControllerNav.selectOnDown = null;
		favoritesButton.Button.ControllerNav.selectOnRight = remarksButton.Button;
		float num2 = ((Component)remarksButton).transform.localPosition.y - remarksButton.Height - 0.25f;
		float num3 = Mathf.Abs(0f - Height + 0.25f) - Mathf.Abs(num2);
		Vector3 localPosition2 = listContainer.localPosition;
		localPosition2.y = num2;
		listContainer.localPosition = localPosition2;
		Vector3 localScale = contentBlocker.localScale;
		localScale.x = Width + Mathf.Abs(contentBlocker.localPosition.x) * 2f;
		localScale.y = Mathf.Abs(localPosition2.y);
		contentBlocker.localScale = localScale;
		QuickChatMenuLandingPageButton[] array;
		if (dynamicButtons != null && dynamicButtons.Length != 0)
		{
			array = dynamicButtons;
			foreach (QuickChatMenuLandingPageButton quickChatMenuLandingPageButton in array)
			{
				ControllerManager.Instance.RemoveSelectableUiElement(quickChatMenuLandingPageButton.Button);
				Object.Destroy((Object)(object)((Component)quickChatMenuLandingPageButton).gameObject);
			}
		}
		QuickChatTag[] visibleTags = Menu.Data.GetVisibleTags();
		dynamicButtons = new QuickChatMenuLandingPageButton[visibleTags.Length];
		for (int j = 0; j < visibleTags.Length; j++)
		{
			QuickChatMenuLandingPageButton button = CreateDynamicButton();
			button.Setup(j, visibleTags[j]);
			button.Button.AddOnClickListeners(() =>
			{
				OnDynamicButtonClicked(button);
			});
			Vector3 localPosition3 = ((Component)button).transform.localPosition;
			localPosition3.y = (button.Height + 0.1f) * (float)j * -1f;
			((Component)button).transform.localPosition = localPosition3;
			dynamicButtons[j] = button;
			if (j == 0)
			{
				button.Button.ControllerNav.selectOnUp = remarksButton.Button;
				remarksButton.Button.ControllerNav.selectOnDown = button.Button;
				favoritesButton.Button.ControllerNav.selectOnDown = button.Button;
			}
			else
			{
				dynamicButtons[j - 1].Button.ControllerNav.selectOnDown = button.Button;
				button.Button.ControllerNav.selectOnUp = dynamicButtons[j - 1].Button;
			}
		}
		float num4 = num3;
		if (dynamicButtons != null && dynamicButtons.Length != 0)
		{
			QuickChatMenuLandingPageButton quickChatMenuLandingPageButton2 = dynamicButtons.Last();
			num4 = Mathf.Abs(((Component)quickChatMenuLandingPageButton2).transform.localPosition.y) + quickChatMenuLandingPageButton2.Height;
		}
		scroller.SetSize(Width, Mathf.Min(num3, num4));
		scroller.SetContentHeight(num4);
		float num5 = Width;
		if (scroller.IsVisible)
		{
			num5 -= 0.2f;
		}
		array = dynamicButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetWidth(num5);
		}
	}

	private QuickChatMenuLandingPageButton CreateDynamicButton()
	{
		((Component)buttonTemplate).gameObject.SetActive(true);
		QuickChatMenuLandingPageButton result = Object.Instantiate<QuickChatMenuLandingPageButton>(buttonTemplate, ((Component)buttonTemplate).transform.parent, true);
		((Component)buttonTemplate).gameObject.SetActive(false);
		return result;
	}

	private void OnRemarksButtonClicked()
	{
		OnButtonPressed?.Invoke(remarksButton);
	}

	private void OnFavoritesButtonClicked()
	{
		OnButtonPressed?.Invoke(favoritesButton);
	}

	private void OnDynamicButtonClicked(QuickChatMenuLandingPageButton button)
	{
		OnButtonPressed?.Invoke(button);
	}
}
