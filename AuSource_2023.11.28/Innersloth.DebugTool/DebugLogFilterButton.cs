using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Innersloth.DebugTool;

public class DebugLogFilterButton : MonoBehaviour
{
	[SerializeField]
	private Image logIcon;

	[SerializeField]
	private Color enabledColor;

	[SerializeField]
	private TextMeshProUGUI logCountText;

	[SerializeField]
	private Button button;

	public bool Enabled { get; private set; }

	public void Initialize(Action onClick)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		ToggleEnabled(enabled: true);
		((UnityEventBase)button.onClick).RemoveAllListeners();
		((UnityEvent)button.onClick).AddListener((UnityAction)(() =>
		{
			onClick();
		}));
	}

	public void SetCount(int count)
	{
		((TMP_Text)logCountText).text = count.ToString();
	}

	public void ToggleEnabled(bool enabled)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Enabled = enabled;
		((Graphic)logIcon).color = (enabled ? enabledColor : Color.grey.SetAlpha(0.5f));
	}
}
