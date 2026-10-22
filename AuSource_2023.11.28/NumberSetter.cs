using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NumberSetter : MonoBehaviour
{
	public enum DateType
	{
		DAYS,
		YEARS,
		MONTHS
	}

	public NumberButton ButtonPrefab;

	public Scroller ButtonParent;

	public AgeGateScreen ParentAgeGateScreen;

	public float ButtonStart = 0.5f;

	public float ButtonHeight = 0.5f;

	private NumberButton[] AllButtons;

	public NumberMenu parentMenu;

	public NumberSetter dayNumberSetter;

	public DateType dateType = DateType.MONTHS;

	private List<UiElement> selectableObjects = new List<UiElement>();

	public UiElement backButton;

	private UiElement defaultButton;

	private void OnEnable()
	{
		UpdateDays();
		if (AllButtons != null)
		{
			selectableObjects.Clear();
			NumberButton[] allButtons = AllButtons;
			foreach (NumberButton numberButton in allButtons)
			{
				selectableObjects.Add(numberButton.Button);
			}
			if (((Behaviour)this).isActiveAndEnabled)
			{
				UiElement defaultSelection = (((Object)(object)defaultButton != (Object)null) ? defaultButton : AllButtons[0].Button);
				ControllerManager.Instance.OpenOverlayMenu(((Object)((Component)this).gameObject).name, backButton, defaultSelection, selectableObjects);
			}
		}
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)((Component)this).gameObject).name);
		defaultButton = null;
	}

	public void SetData(NumberButton selected)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		parentMenu.SetValue(((TMP_Text)selected.Text).text);
		for (int i = 0; i < AllButtons.Length; i++)
		{
			((Graphic)AllButtons[i].Text).color = Color.white;
		}
		((Graphic)selected.Text).color = Color.green;
		switch (dateType)
		{
		case DateType.MONTHS:
			ParentAgeGateScreen.Month = selected.monthNum;
			dayNumberSetter.UpdateDays();
			break;
		case DateType.DAYS:
			ParentAgeGateScreen.Day = int.Parse(((TMP_Text)selected.Text).text);
			break;
		case DateType.YEARS:
			ParentAgeGateScreen.Year = int.Parse(((TMP_Text)selected.Text).text);
			dayNumberSetter.UpdateDays();
			break;
		}
		parentMenu.Close();
	}

	public void UpdateDays()
	{
		UpdateButtons();
		if (!ParentAgeGateScreen.IsDefaultDayText(((TMP_Text)parentMenu.text).text) && ParentAgeGateScreen.Day != 0 && dateType == DateType.DAYS)
		{
			int daysInMonth = GetDaysInMonth();
			ParentAgeGateScreen.Day = Mathf.Clamp(ParentAgeGateScreen.Day, 0, daysInMonth);
			((TMP_Text)parentMenu.text).text = ParentAgeGateScreen.Day.ToString();
		}
	}

	private int GetDaysInMonth()
	{
		int year = ((ParentAgeGateScreen.Year == 0) ? DateTime.UtcNow.Year : ParentAgeGateScreen.Year);
		int month = ((ParentAgeGateScreen.Month == 0) ? 1 : ParentAgeGateScreen.Month);
		return DateTime.DaysInMonth(year, month);
	}

	private void UpdateButtons()
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected Obj, but got Unknown
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		Collider2D component = ((Component)ButtonParent).GetComponent<Collider2D>();
		_ = AllButtons;
		string[] array = new string[0];
		switch (dateType)
		{
		case DateType.MONTHS:
		{
			array = new string[12];
			for (int i = 0; i < 12; i++)
			{
				array[i] = (i + 1).ToString();
			}
			break;
		}
		case DateType.DAYS:
		{
			int daysInMonth = GetDaysInMonth();
			array = new string[daysInMonth];
			for (int j = 0; j < daysInMonth; j++)
			{
				array[j] = (j + 1).ToString();
			}
			break;
		}
		case DateType.YEARS:
		{
			int year = DateTime.Now.Year;
			array = new string[year - 1900];
			for (int num = year; num > 1900; num--)
			{
				array[year - num] = num.ToString();
			}
			break;
		}
		}
		Vector3 localPosition = new Vector3(0f, ButtonStart, -0.5f);
		TryDestroyExistingButtons();
		AllButtons = new NumberButton[array.Length];
		for (int k = 0; k < array.Length; k++)
		{
			NumberButton button = Object.Instantiate<NumberButton>(ButtonPrefab, ButtonParent.Inner);
			AllButtons[k] = button;
			if (dateType == DateType.MONTHS)
			{
				((TMP_Text)button.Text).text = DestroyableSingleton<TranslationController>.Instance.GetMonthStringViaNumber(k + 1);
			}
			else
			{
				((TMP_Text)button.Text).text = array[k];
			}
			((UnityEvent)button.Button.OnClick).AddListener((UnityAction)(() =>
			{
				SetData(button);
			}));
			button.Button.ClickMask = component;
			switch (dateType)
			{
			case DateType.MONTHS:
				button.monthNum = k + 1;
				if (button.monthNum == ParentAgeGateScreen.Month)
				{
					((Graphic)button.Text).color = Color.green;
					defaultButton = button.Button;
				}
				break;
			case DateType.DAYS:
				if (array[k] == ParentAgeGateScreen.Day.ToString())
				{
					((Graphic)button.Text).color = Color.green;
					defaultButton = button.Button;
				}
				break;
			case DateType.YEARS:
				if (array[k] == ParentAgeGateScreen.Year.ToString())
				{
					((Graphic)button.Text).color = Color.green;
					defaultButton = button.Button;
				}
				break;
			}
			((Component)button).transform.localPosition = localPosition;
			localPosition.y -= ButtonHeight;
		}
		ButtonParent.SetBoundsMax((float)array.Length * ButtonHeight - 2f * ButtonStart - 0.1f, 0f);
	}

	private void TryDestroyExistingButtons()
	{
		if (AllButtons == null)
		{
			return;
		}
		NumberButton[] allButtons = AllButtons;
		foreach (NumberButton numberButton in allButtons)
		{
			if (Object.op_Implicit((Object)(object)numberButton))
			{
				Object.Destroy((Object)(object)((Component)numberButton).gameObject);
			}
		}
	}
}
