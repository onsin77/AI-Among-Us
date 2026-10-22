using System;
using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Innersloth.DebugTool;

public class DebugLogMessageLine : MonoBehaviour, ICell
{
	[SerializeField]
	private Button expandButton;

	[SerializeField]
	private Image fader;

	[SerializeField]
	private TextMeshProUGUI logLine;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image warningLogIcon;

	[SerializeField]
	private Image regularLogIcon;

	[SerializeField]
	private Image errorLogIcon;

	[SerializeField]
	private Color fadeColor;

	[SerializeField]
	private Color evenColor;

	[SerializeField]
	private Color oddColor;

	[SerializeField]
	private Color highlightColor;

	public int Index { get; private set; }

	public void Initialize(DebugLogMessage debugLog, Action<int, DebugLogMessage> onClick, int index)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected Obj, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Invalid comparison between Unknown and I4
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Invalid comparison between Unknown and I4
		((TMP_Text)logLine).SetText(debugLog.LogString, true);
		((UnityEventBase)expandButton.onClick).RemoveAllListeners();
		((UnityEvent)expandButton.onClick).AddListener((UnityAction)(() =>
		{
			onClick(index, debugLog);
		}));
		((Graphic)background).color = ((index % 2 == 0) ? evenColor : oddColor);
		Index = index;
		ToggleFade(faded: false);
		((Component)errorLogIcon).gameObject.SetActive(false);
		((Component)regularLogIcon).gameObject.SetActive(false);
		((Component)warningLogIcon).gameObject.SetActive(false);
		LogType logType = debugLog.LogType;
		if ((int)logType != 2)
		{
			if ((int)logType == 3)
			{
				((Component)regularLogIcon).gameObject.SetActive(true);
			}
			else
			{
				((Component)errorLogIcon).gameObject.SetActive(true);
			}
		}
		else
		{
			((Component)warningLogIcon).gameObject.SetActive(true);
		}
	}

	public void ToggleFade(bool faded)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)fader).color = (faded ? fadeColor : Color.clear);
	}

	public void ToggleHighlight(bool highlighted)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (highlighted)
		{
			((Graphic)background).color = highlightColor;
		}
		else
		{
			((Graphic)background).color = ((Index % 2 == 0) ? evenColor : oddColor);
		}
	}
}
