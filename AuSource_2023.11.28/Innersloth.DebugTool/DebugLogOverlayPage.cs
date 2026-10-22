using System.Collections;
using System.Collections.Generic;
using System.Text;
using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Innersloth.DebugTool;

public class DebugLogOverlayPage : DebugOverlayPage, IRecyclableScrollRectDataSource
{
	private readonly Dictionary<LogType, DebugLogFilterButton> logButtons = new Dictionary<LogType, DebugLogFilterButton>();

	[SerializeField]
	private RecyclableScrollRect logScrollList;

	[SerializeField]
	private RectTransform idealLogPosition;

	[Header("Bottom Section")]
	[SerializeField]
	private Button clearButton;

	[SerializeField]
	private Button copyButton;

	[SerializeField]
	private DebugLogFilterButton logsButton;

	[SerializeField]
	private DebugLogFilterButton warningsButton;

	[SerializeField]
	private DebugLogFilterButton errorsButton;

	[Header("Log Details")]
	[SerializeField]
	private GameObject detailParent;

	[SerializeField]
	private TextMeshProUGUI detailsText;

	[SerializeField]
	private GameObject notListeningWarning;

	private string detailsToCopy;

	private readonly List<DebugLogMessage> logMessageList = new List<DebugLogMessage>();

	private bool initialized;

	private int selectedLogIndex;

	public override string Name => "Debug Logs";

	public override bool ConsumingNavActions => false;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		((UnityEvent)clearButton.onClick).AddListener((UnityAction)ClearLogs);
		((UnityEvent)copyButton.onClick).AddListener((UnityAction)CopyLogOrLogs);
	}

	private void Update()
	{
		foreach (IDebugToolInputProvider inputProvider in InputProviders)
		{
			int num = selectedLogIndex;
			float vertical = inputProvider.GetVertical();
			if (vertical < -0.9f)
			{
				selectedLogIndex = Mathf.Clamp(selectedLogIndex + 1, 0, logMessageList.Count - 1);
			}
			else if (vertical > 0.9f)
			{
				selectedLogIndex = Mathf.Clamp(selectedLogIndex - 1, 0, logMessageList.Count - 1);
			}
			if (selectedLogIndex != num)
			{
				HighlightLog(selectedLogIndex);
				if (detailParent.gameObject.activeSelf)
				{
					ShowLogDetails(logMessageList[selectedLogIndex]);
					FadeOtherLogs(selectedLogIndex);
				}
				UpdateScrollRectPosition(num, selectedLogIndex);
			}
			if (inputProvider.GetConfirm())
			{
				HighlightLog(selectedLogIndex);
				ShowLogDetails(logMessageList[selectedLogIndex]);
			}
			if (inputProvider.GetCancel())
			{
				HideDetails();
			}
			if (inputProvider.GetCopy())
			{
				CopyLogOrLogs();
			}
			if (inputProvider.GetClear())
			{
				ClearLogs();
			}
		}
	}

	private void UpdateScrollRectPosition(int originalSelectedIndex, int newlySelectedIndex)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		float num = newlySelectedIndex - originalSelectedIndex;
		Rect rect = logScrollList.PrototypeCell.rect;
		float num2 = num * rect.height;
		if (num2 < 0f)
		{
			num2 = Mathf.Clamp(num2, 0f - ((ScrollRect)logScrollList).content.anchoredPosition.y, 0f);
		}
		if (num2 > 0f)
		{
			float num3 = num2;
			rect = ((ScrollRect)logScrollList).viewport.rect;
			num2 = Mathf.Clamp(num3, 0f, rect.height / 2f - ((ScrollRect)logScrollList).content.anchoredPosition.y);
		}
		Vector3[] array = new Vector3[4];
		idealLogPosition.GetWorldCorners(array);
		Rect val = new Rect(Vector2.op_Implicit(array[0]), Vector2.op_Implicit(array[2] - array[0]));
		DebugLogMessageLine debugLogMessageLine = FindLogObject(newlySelectedIndex);
		Vector3[] array2 = new Vector3[4];
		((RectTransform)((Component)debugLogMessageLine).transform).GetWorldCorners(array2);
		Rect val2 = new Rect(Vector2.op_Implicit(array2[0]), Vector2.op_Implicit(array2[2] - array2[0]));
		if (Mathf.Abs(num2) > 0.1f && (newlySelectedIndex >= originalSelectedIndex || val.Overlaps(val2) || !(val2.yMax < val.yMin)) && (newlySelectedIndex <= originalSelectedIndex || val.Overlaps(val2) || !(val2.yMin > val.yMax)))
		{
			RectTransform content = ((ScrollRect)logScrollList).content;
			content.anchoredPosition += new Vector2(0f, num2);
			logScrollList.OnValueChangedListener(Vector2.zero);
		}
	}

	public override void Show()
	{
		bool flag = false;
		if (!initialized)
		{
			logButtons[(LogType)3] = logsButton;
			logButtons[(LogType)2] = warningsButton;
			logButtons[(LogType)0] = errorsButton;
			logsButton.Initialize(() =>
			{
				Filter((LogType)3);
			});
			warningsButton.Initialize(() =>
			{
				Filter((LogType)2);
			});
			errorsButton.Initialize(() =>
			{
				Filter((LogType)0);
			});
			logScrollList.Initialize(this);
			initialized = true;
			flag = true;
		}
		HideDetails();
		RefreshLogList();
		((MonoBehaviour)this).StartCoroutine(CoScrollToBottom());
		if (!flag)
		{
			logScrollList.ReloadData();
		}
		notListeningWarning.SetActive(!DebugTool.ListeningToLogs);
	}

	public override void OnBackButtonPressed()
	{
		HideDetails();
	}

	public int GetItemCount()
	{
		return logMessageList.Count;
	}

	public void SetCell(ICell cell, int index)
	{
		(cell as DebugLogMessageLine)?.Initialize(logMessageList[index], (int idx, DebugLogMessage log) =>
		{
			FadeOtherLogs(index);
			HighlightLog(idx);
			ShowLogDetails(log);
		}, index);
	}

	private void HideDetails()
	{
		detailParent.SetActive(false);
		detailsToCopy = null;
		DebugUIEvents.OnDetailsToggledVisible(obj: false);
		UnfadeLogs();
	}

	private IEnumerator CoScrollToBottom()
	{
		yield return (object)new WaitForEndOfFrame();
		yield return (object)new WaitForEndOfFrame();
		logScrollList.ScrollToBottom();
		HighlightLog(logMessageList.Count - 1);
	}

	private void RefreshLogList()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		IEnumerator<DebugLogMessage> logs = DebugTool.Logs;
		logMessageList.Clear();
		Dictionary<LogType, int> dictionary = new Dictionary<LogType, int>();
		Dictionary<LogType, bool> dictionary2 = new Dictionary<LogType, bool>
		{
			{
				(LogType)3,
				logButtons[(LogType)3].Enabled
			},
			{
				(LogType)2,
				logButtons[(LogType)2].Enabled
			},
			{
				(LogType)0,
				logButtons[(LogType)0].Enabled
			},
			{
				(LogType)4,
				logButtons[(LogType)0].Enabled
			}
		};
		while (logs.MoveNext())
		{
			DebugLogMessage current = logs.Current;
			if (!dictionary.ContainsKey(current.LogType))
			{
				dictionary[current.LogType] = 0;
			}
			dictionary[current.LogType]++;
			dictionary2.TryGetValue(current.LogType, out var value);
			if (value)
			{
				logMessageList.Add(current);
			}
		}
		SetFilterButtonCounts(dictionary);
	}

	private void SetFilterButtonCounts(Dictionary<LogType, int> logTypeCounts)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<LogType, DebugLogFilterButton> logButton in logButtons)
		{
			logTypeCounts.TryGetValue(logButton.Key, out var value);
			if ((int)logButton.Key == 0)
			{
				logTypeCounts.TryGetValue((LogType)4, out var value2);
				value += value2;
			}
			logButton.Value.SetCount(value);
		}
	}

	private void Filter(LogType logType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		DebugLogFilterButton debugLogFilterButton = logButtons[logType];
		debugLogFilterButton.ToggleEnabled(!debugLogFilterButton.Enabled);
		RefreshLogList();
		logScrollList.ReloadData();
	}

	private void CopyLogOrLogs()
	{
		if (detailsToCopy != null)
		{
			GUIUtility.systemCopyBuffer = detailsToCopy;
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		IEnumerator<DebugLogMessage> logs = DebugTool.Logs;
		while (logs.MoveNext())
		{
			DebugLogMessage current = logs.Current;
			stringBuilder.AppendLine(current.LogString);
			stringBuilder.AppendLine(current.Stacktrace);
		}
		GUIUtility.systemCopyBuffer = stringBuilder.ToString();
	}

	private void ClearLogs()
	{
		DebugTool.ClearLogs();
		RefreshLogList();
		logScrollList.ReloadData();
		HideDetails();
	}

	private void ShowLogDetails(DebugLogMessage debugLogMsg)
	{
		detailParent.SetActive(true);
		string text = debugLogMsg.LogString + "\n" + debugLogMsg.Stacktrace;
		((TMP_Text)detailsText).SetText(text, true);
		detailsToCopy = text;
		DebugUIEvents.OnDetailsToggledVisible(obj: true);
	}

	private void HighlightLog(int i)
	{
		selectedLogIndex = i;
		IEnumerator<ICell> cells = logScrollList.GetCells();
		while (cells.MoveNext())
		{
			DebugLogMessageLine debugLogMessageLine = cells.Current as DebugLogMessageLine;
			debugLogMessageLine.ToggleHighlight(i == debugLogMessageLine.Index);
		}
	}

	private void FadeOtherLogs(int index)
	{
		IEnumerator<ICell> cells = logScrollList.GetCells();
		while (cells.MoveNext())
		{
			DebugLogMessageLine debugLogMessageLine = cells.Current as DebugLogMessageLine;
			debugLogMessageLine.ToggleFade(index != debugLogMessageLine.Index);
		}
	}

	private DebugLogMessageLine FindLogObject(int index)
	{
		IEnumerator<ICell> cells = logScrollList.GetCells();
		while (cells.MoveNext())
		{
			DebugLogMessageLine debugLogMessageLine = cells.Current as DebugLogMessageLine;
			if (debugLogMessageLine.Index == index)
			{
				return debugLogMessageLine;
			}
		}
		return null;
	}

	private void UnfadeLogs()
	{
		IEnumerator<ICell> cells = logScrollList.GetCells();
		while (cells.MoveNext())
		{
			(cells.Current as DebugLogMessageLine).ToggleFade(faded: false);
		}
	}
}
