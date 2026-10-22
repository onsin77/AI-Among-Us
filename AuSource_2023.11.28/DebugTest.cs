using System.Text;
using Rewired;
using Rewired.Internal;
using Rewired.Utils;
using UnityEngine;

public class DebugTest : MonoBehaviour
{
	private GUIText _guiText;

	private StringBuilder _sb;

	private byte[] deviceIdBuffer = new byte[32];

	private void Awake()
	{
		_guiText = ((Component)this).gameObject.AddComponent<GUIText>();
		_guiText.anchor = (TextAnchor)0;
		_guiText.fontSize = 20;
		_sb = new StringBuilder();
	}

	private void Update()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_sb.Clear();
		LogLine("Rewired Initialized", ReInput.isReady);
		LogLine("Platform", UnityTools.platform);
		LogLine("Using Unity Input", ReInput.usingUnityInput);
		LogLine();
		LogLine("Joysticks", ReInput.controllers.joystickCount);
		for (int i = 0; i < ReInput.controllers.joystickCount; i++)
		{
			Joystick joy = ReInput.controllers.Joysticks[i];
			Log(joy);
		}
		_guiText.text = _sb.ToString();
	}

	private void Log(object text)
	{
		if (text != null)
		{
			_sb.Append(text);
		}
	}

	private void Log(byte[] buffer)
	{
		if (buffer != null)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				_sb.Append(buffer[i].ToString("X2"));
			}
		}
	}

	private void LogLine(object text = null)
	{
		if (text != null)
		{
			_sb.Append(text);
		}
		_sb.Append("\n");
	}

	private void LogLine(string prefix, object text)
	{
		_sb.Append(prefix);
		_sb.Append(": ");
		if (text != null)
		{
			_sb.Append(text);
		}
		_sb.Append("\n");
	}

	private void Log(Joystick joy)
	{
		LogLine("Name", ((Controller)joy).name);
		LogLine("Hardware Name", ((Controller)joy).hardwareName);
		LogLine("Instance Guid", ((Controller)joy).deviceInstanceGuid);
		LogLine("Hardware Identifier", ((Controller)joy).hardwareIdentifier);
		LogLine("Button Count", ((Controller)joy).buttonCount);
		LogLine("Axis Count", ((ControllerWithAxes)joy).axisCount);
		LogLine("Has Extension", ((Controller)joy).extension != null);
		string text = "";
		foreach (Player allPlayer in ReInput.players.AllPlayers)
		{
			if (allPlayer.controllers.ContainsController((Controller)(object)joy))
			{
				text = text + allPlayer.id + ", ";
			}
		}
		LogLine("Assigned to Players", text);
		string text2 = "";
		for (int i = 0; i < ((Controller)joy).buttonCount; i++)
		{
			if (((Controller)joy).Buttons[i].value)
			{
				text2 = text2 + i + ", ";
			}
		}
		LogLine("Buttons", text2);
		LogLine();
	}
}
