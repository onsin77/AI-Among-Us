using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class SimpleControlRemapping : MonoBehaviour
{
	private class Row
	{
		public InputAction action;

		public AxisRange actionRange;

		public Button button;

		public Text text;
	}

	private const string category = "Default";

	private const string layout = "Default";

	private const string uiCategory = "UI";

	private InputMapper inputMapper = new InputMapper();

	public GameObject buttonPrefab;

	public GameObject textPrefab;

	public RectTransform fieldGroupTransform;

	public RectTransform actionGroupTransform;

	public Text controllerNameUIText;

	public Text statusUIText;

	private ControllerType selectedControllerType;

	private int selectedControllerId;

	private List<Row> rows = new List<Row>();

	private Player player => ReInput.players.GetPlayer(0);

	private ControllerMap controllerMap
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if (controller == null)
			{
				return null;
			}
			return player.controllers.maps.GetMap(controller.type, controller.id, "Default", "Default");
		}
	}

	private Controller controller
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return player.controllers.GetController(selectedControllerType, selectedControllerId);
		}
	}

	private void OnEnable()
	{
		if (ReInput.isReady)
		{
			inputMapper.options.timeout = 5f;
			inputMapper.options.ignoreMouseXAxis = true;
			inputMapper.options.ignoreMouseYAxis = true;
			ReInput.ControllerConnectedEvent += OnControllerChanged;
			ReInput.ControllerDisconnectedEvent += OnControllerChanged;
			inputMapper.InputMappedEvent += OnInputMapped;
			inputMapper.StoppedEvent += OnStopped;
			InitializeUI();
		}
	}

	private void OnDisable()
	{
		inputMapper.Stop();
		inputMapper.RemoveAllEventListeners();
		ReInput.ControllerConnectedEvent -= OnControllerChanged;
		ReInput.ControllerDisconnectedEvent -= OnControllerChanged;
	}

	private void RedrawUI()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected Obj, but got Unknown
		if (controller == null)
		{
			ClearUI();
			return;
		}
		controllerNameUIText.text = controller.name;
		for (int i = 0; i < rows.Count; i++)
		{
			Row row = rows[i];
			InputAction action = rows[i].action;
			string text = string.Empty;
			int actionElementMapId = -1;
			foreach (ActionElementMap item in controllerMap.ElementMapsWithAction(action.id))
			{
				if (item.ShowInField(row.actionRange))
				{
					text = item.elementIdentifierName;
					actionElementMapId = item.id;
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)selectedControllerType == 2)
		{
			controllerNameUIText.text = "No joysticks attached";
		}
		else
		{
			controllerNameUIText.text = string.Empty;
		}
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

	private void SetSelectedController(ControllerType controllerType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (controllerType != selectedControllerType)
		{
			selectedControllerType = controllerType;
			flag = true;
		}
		int num = selectedControllerId;
		if ((int)selectedControllerType == 2)
		{
			if (player.controllers.joystickCount > 0)
			{
				selectedControllerId = ((Controller)player.controllers.Joysticks[0]).id;
			}
			else
			{
				selectedControllerId = -1;
			}
		}
		else
		{
			selectedControllerId = 0;
		}
		if (selectedControllerId != num)
		{
			flag = true;
		}
		if (flag)
		{
			inputMapper.Stop();
			RedrawUI();
		}
	}

	public void OnControllerSelected(int controllerType)
	{
		SetSelectedController((ControllerType)controllerType);
	}

	private void OnInputFieldClicked(int index, int actionElementMapToReplaceId)
	{
		if (index >= 0 && index < rows.Count && controller != null)
		{
			((MonoBehaviour)this).StartCoroutine(StartListeningDelayed(index, actionElementMapToReplaceId));
		}
	}

	private IEnumerator StartListeningDelayed(int index, int actionElementMapToReplaceId)
	{
		yield return (object)new WaitForSeconds(0.1f);
		inputMapper.Start(new Context
		{
			actionId = rows[index].action.id,
			controllerMap = controllerMap,
			actionRange = rows[index].actionRange,
			actionElementMapToReplace = controllerMap.GetElementMap(actionElementMapToReplaceId)
		});
		player.controllers.maps.SetMapsEnabled(false, "UI");
		statusUIText.text = "Listening...";
	}

	private void OnControllerChanged(ControllerStatusChangedEventArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetSelectedController(selectedControllerType);
	}

	private void OnInputMapped(InputMappedEventData data)
	{
		RedrawUI();
	}

	private void OnStopped(StoppedEventData data)
	{
		statusUIText.text = string.Empty;
		player.controllers.maps.SetMapsEnabled(true, "UI");
	}

	public SimpleControlRemapping()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
	}
}
