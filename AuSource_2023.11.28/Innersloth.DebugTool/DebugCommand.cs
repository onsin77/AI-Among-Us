using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Innersloth.DebugTool;

public class DebugCommand
{
	public readonly string CommandPath;

	public readonly MethodInfo ActionInfo;

	public readonly object Target;

	public string Description;

	public bool CloseBeforeExecute;

	private readonly Dictionary<int, string> argumentPlaceholders = new Dictionary<int, string>();

	public string Directory
	{
		get
		{
			if (CommandPath.LastIndexOf('/') > 0)
			{
				return CommandPath.Substring(0, CommandPath.LastIndexOf('/'));
			}
			return string.Empty;
		}
	}

	public string CommandName
	{
		get
		{
			int num = CommandPath.LastIndexOf('/') + 1;
			return CommandPath.Substring(num, CommandPath.Length - num);
		}
	}

	public DebugCommand(string commandPath, object target, MethodInfo methodInfo)
	{
		CommandPath = commandPath;
		ActionInfo = methodInfo;
		Target = target;
	}

	public string GetArgumentPlaceholder(int i)
	{
		if (argumentPlaceholders.ContainsKey(i))
		{
			return argumentPlaceholders[i];
		}
		Debug.LogWarning((object)$"No argument placeholder was found for argument {i} of command {CommandPath}");
		return "...";
	}

	public void SetArgumentPlaceholder(int i, string placeholder)
	{
		argumentPlaceholders[i] = placeholder;
	}
}
