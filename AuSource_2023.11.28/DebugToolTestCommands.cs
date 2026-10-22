using System;
using Innersloth.DebugTool;
using UnityEngine;

public class DebugToolTestCommands : MonoBehaviour
{
	[SerializeField]
	private DebugOverlay overlay;

	[SerializeField]
	private bool testLogsOnStartup;

	private void Start()
	{
		overlay.RegisterInputProvider(new KeyboardDebugToolInputProvider());
		overlay.RegisterKeyboardInputProvider(new DebugNullKeyboardInputProvider());
		DebugTool.RegisterCommandHandler(this);
		DebugTool.ListenToLogs();
		if (testLogsOnStartup)
		{
			LogABunchOfRandomStuff();
		}
	}

	private void LogABunchOfRandomStuff()
	{
		for (int i = 0; i < 100; i++)
		{
			switch (Random.Range(0, 4))
			{
			case 0:
				Debug.Log((object)"This is a test log ~~~");
				break;
			case 1:
				Debug.LogWarning((object)"Omg Warning Warning.");
				break;
			case 2:
				Debug.LogError((object)"This is super bad~~~! Maybe you should investigate!");
				break;
			case 3:
				Debug.LogException(new Exception("I am an exception I bet"));
				break;
			}
		}
	}

	private void OnDestroy()
	{
		DebugTool.UnregisterCommandHandler(this);
	}

	[DebugCommand("Apple/Car/Person", "", false)]
	public static void AppleCarPerson()
	{
		Debug.Log((object)"Apple, car, person");
	}

	[DebugCommand("Apple/Car/Melon", "", false)]
	public static void AppleCarMelon()
	{
		Debug.Log((object)"Apple, car, melon");
	}

	[DebugCommand("Apple/Cat/Melon", "", false)]
	public static void AppleCat()
	{
		Debug.Log((object)"Apple, cat");
	}

	[DebugCommand("Banana/Dog/Mercury", "", false)]
	public static void BananaDogMercury()
	{
		Debug.Log((object)"Banana, dog, mercury");
	}

	[DebugCommand("Rainbow", "", false)]
	public static void Rainbow()
	{
		Debug.Log((object)"Rainbow");
	}

	[DebugCommand("Bacon and Eggs++", "This is a breakfast food that many enjoy.", true)]
	public static void BaconAndEggs([DebugParameter("Bacon!!")] int bacon, [DebugParameter("Eggs!!")] string eggs, [DebugParameter("Additional Breakfast 1")] string moreBreakfast, [DebugParameter("Avocado Toast")] string avocadoToast)
	{
		Debug.Log((object)$"Bacon: {bacon} and eggs: {eggs}");
		if (avocadoToast != "Yes")
		{
			throw new Exception("You must always have avocado toast. Enter 'Yes' for that field.");
		}
	}

	[DebugCommand("Throw an exception", "", false)]
	public static void ThrowAnException()
	{
		throw new Exception("I'm a fun exception");
	}

	[DebugCommand("More/Commands/Here", "", false)]
	public static void MoreCommandsHere()
	{
		Debug.Log((object)"More commands here");
	}

	[DebugCommand("More/Commands/There", "", false)]
	public static void MoreCommandsThere()
	{
		Debug.Log((object)"More commands there");
	}

	[DebugCommand("More/Commands/Everywhere", "", false)]
	public static void MoreCommandsEverywhere()
	{
		Debug.Log((object)"More commands everywhere");
	}
}
