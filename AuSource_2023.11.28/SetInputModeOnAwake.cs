using UnityEngine;

public class SetInputModeOnAwake : MonoBehaviour
{
	public ConsoleJoystick.ConsoleInputState inputMode;

	private void Awake()
	{
		switch (inputMode)
		{
		case ConsoleJoystick.ConsoleInputState.Gameplay:
			ConsoleJoystick.SetMode_Gameplay();
			break;
		case ConsoleJoystick.ConsoleInputState.Menu:
			ConsoleJoystick.SetMode_Menu();
			break;
		case ConsoleJoystick.ConsoleInputState.Sabotage:
			ConsoleJoystick.SetMode_Sabotage();
			break;
		case ConsoleJoystick.ConsoleInputState.Task:
			ConsoleJoystick.SetMode_Task();
			break;
		case ConsoleJoystick.ConsoleInputState.Vent:
			ConsoleJoystick.SetMode_Vent();
			break;
		case ConsoleJoystick.ConsoleInputState.QuickChat:
			ConsoleJoystick.SetMode_QuickChat();
			break;
		}
	}
}
