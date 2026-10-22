using System.Collections.Generic;
using UnityEngine;

public static class GameDebugCommands
{
	private const string ANIMATION_KILL_PATH = "Animation/Kill/";

	private static readonly List<string> AddedCommands = new List<string>();

	private static Coroutine WalkInCircleCoroutines;

	public static void AddCommands()
	{
	}

	public static void RemoveCommands()
	{
	}
}
