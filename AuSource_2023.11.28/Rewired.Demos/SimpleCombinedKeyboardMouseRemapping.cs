using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class SimpleCombinedKeyboardMouseRemapping : MonoBehaviour
{
	private class Row
	{
		public InputAction action;

		public AxisRange actionRange;

		public Button button;

		public Text text;
	}

	private struct TargetMapping
	{
		public ControllerMap controllerMap;

		public int actionElementMapId;
	}

	private const string category = "Default";

	private const string layout = "Default";

	private const string uiCategory = "UI";

	private InputMapper inputMapper_keyboard = new InputMapper();

	private InputMapper inputMapper_mouse = new InputMapper();

	public GameObject buttonPrefab;

	public GameObject textPrefab;

	public RectTransform fieldGroupTransform;

	public RectTransform actionGroupTransform;

	public Text controllerNameUIText;

	public Text statusUIText;

	private List<Row> rows = new List<Row>();

	private TargetMapping _replaceTargetMapping;

	private Player player => ReInput.players.GetPlayer(0);

	private void OnEnable()
	{
		if (ReInput.isReady)
		{
			inputMapper_keyboard.options.timeout = 5f;
			inputMapper_mouse.options.timeout = 5f;
			inputMapper_mouse.options.ignoreMouseXAxis = true;
			inputMapper_mouse.options.ignoreMouseYAxis = true;
			inputMapper_keyboard.options.allowButtonsOnFullAxisAssignment = false;
			inputMapper_mouse.options.allowButtonsOnFullAxisAssignment = false;
			inputMapper_keyboard.InputMappedEvent += OnInputMapped;
			inputMapper_keyboard.StoppedEvent += OnStopped;
			inputMapper_mouse.InputMappedEvent += OnInputMapped;
			inputMapper_mouse.StoppedEvent += OnStopped;
			InitializeUI();
		}
	}

	private void OnDisable()
	{
		inputMapper_keyboard.Stop();
		inputMapper_mouse.Stop();
		inputMapper_keyboard.RemoveAllEventListeners();
		inputMapper_mouse.RemoveAllEventListeners();
	}

	private void RedrawUI()
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected Obj, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		controllerNameUIText.text = "Keyboard/Mouse";
		for (int i = 0; i < rows.Count; i++)
		{
			Row row = rows[i];
			InputAction action = rows[i].action;
			string text = string.Empty;
			int actionElementMapId = -1;
			for (int j = 0; j < 2; j++)
			{
				ControllerType val = (ControllerType)(j != 0);
				foreach (ActionElementMap item in player.controllers.maps.GetMap(val, 0, "Default", "Default").ElementMapsWithAction(action.id))
				{
					if (item.ShowInField(row.actionRange))
					{
						text = item.elementIdentifierName;
						actionElementMapId = item.id;
						break;
					}
				}
				if (actionElementMapId >= 0)
				{
					break;
				}
			}
			row.text.text = text;
			((UnityEventBase)row.button.onClick).RemoveAllListeners();
			int index = i;
			((UnityEvent)row.button.onClick).AddListener((UnityAction)(() =>
			{
				OnInputFieldClicked(index, actionElementMapId);
			}));
		}
	}

	private void ClearUI()
	{
		controllerNameUIText.text = string.Empty;
		for (int i = 0; i < rows.Count; i++)
		{
			rows[i].text.text = string.Empty;
		}
	}

	private void InitializeUI()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Invalid comparison between Unknown and I4
		foreach (Transform item in (Transform)actionGroupTransform)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
		foreach (Transform item2 in (Transform)fieldGroupTransform)
		{
			Object.Destroy((Object)(object)((Component)item2).gameObject);
		}
		foreach (InputAction item3 in ReInput.mapping.ActionsInCategory("Default"))
		{
			if ((int)item3.type == 0)
			{
				CreateUIRow(item3, (AxisRange)0, item3.descriptiveName);
				CreateUIRow(item3, (AxisRange)1, (!string.IsNullOrEmpty(item3.positiveDescriptiveName)) ? item3.positiveDescriptiveName : (item3.descriptiveName + " +"));
				CreateUIRow(item3, (AxisRange)2, (!string.IsNullOrEmpty(item3.negativeDescriptiveName)) ? item3.negativeDescriptiveName : (item3.descriptiveName + " -"));
			}
			else if ((int)item3.type == 1)
			{
				CreateUIRow(item3, (AxisRange)1, item3.descriptiveName);
			}
		}
		RedrawUI();
	}

	private void CreateUIRow(InputAction action, AxisRange actionRange, string label)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(textPrefab);
		val.transform.SetParent((Transform)(object)actionGroupTransform);
		val.transform.SetAsLastSibling();
		val.GetComponent<Text>().text = label;
		GameObject val2 = Object.Instantiate<GameObject>(buttonPrefab);
		val2.transform.SetParent((Transform)(object)fieldGroupTransform);
		val2.transform.SetAsLastSibling();
		rows.Add(new Row
		{
			action = action,
			actionRange = actionRange,
			button = val2.GetComponent<Button>(),
			text = val2.GetComponentInChildren<Text>()
		});
	}

	private void OnInputFieldClicked(int index, int actionElementMapToReplaceId)
	{
		if (index >= 0 && index < rows.Count)
		{
			ControllerMap map = player.controllers.maps.GetMap((ControllerType)0, 0, "Default", "Default");
			ControllerMap map2 = player.controllers.maps.GetMap((ControllerType)1, 0, "Default", "Default");
			ControllerMap controllerMap;
			if (map.ContainsElementMap(actionElementMapToReplaceId))
			{
				controllerMap = map;
			}
			else
			{
				controllerMap = ((!map2.ContainsElementMap(actionElementMapToReplaceId)) ? null : map2);
			}
			_replaceTargetMapping = new TargetMapping
			{
				actionElementMapId = actionElementMapToReplaceId,
				controllerMap = controllerMap
			};
			((MonoBehaviour)this).StartCoroutine(StartListeningDelayed(index, map, map2, actionElementMapToReplaceId));
		}
	}

	private IEnumerator StartListeningDelayed(int index, ControllerMap keyboardMap, ControllerMap mouseMap, int actionElementMapToReplaceId)
	{
		yield return (object)new WaitForSeconds(0.1f);
		inputMapper_keyboard.Start(new Context
		{
			actionId = rows[index].action.id,
			controllerMap = keyboardMap,
			actionRange = rows[index].actionRange,
			actionElementMapToReplace = keyboardMap.GetElementMap(actionElementMapToReplaceId)
		});
		inputMapper_mouse.Start(new Context
		{
			actionId = rows[index].action.id,
			controllerMap = mouseMap,
			actionRange = rows[index].actionRange,
			actionElementMapToReplace = mouseMap.GetElementMap(actionElementMapToReplaceId)
		});
		player.controllers.maps.SetMapsEnabled(false, "UI");
		statusUIText.text = "Listening...";
	}

	private void OnInputMapped(InputMappedEventData data)
	{
		inputMapper_keyboard.Stop();
		inputMapper_mouse.Stop();
		if (_replaceTargetMapping.controllerMap != null && data.actionElementMap.controllerMap != _replaceTargetMapping.controllerMap)
		{
			_replaceTargetMapping.controllerMap.DeleteElementMap(_replaceTargetMapping.actionElementMapId);
		}
		RedrawUI();
	}

	private void OnStopped(StoppedEventData data)
	{
		statusUIText.text = string.Empty;
		player.controllers.maps.SetMapsEnabled(true, "UI");
	}

	public SimpleCombinedKeyboardMouseRemapping()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
	}
}
