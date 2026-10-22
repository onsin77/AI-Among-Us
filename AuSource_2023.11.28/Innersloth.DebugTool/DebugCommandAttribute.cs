using System;

namespace Innersloth.DebugTool;

[AttributeUsage(AttributeTargets.Method)]
public class DebugCommandAttribute : Attribute
{
	public string Path;

	public string Description;

	public bool CloseBeforeExecute;

	public DebugCommandAttribute(string path, string description = "", bool closeBeforeExecute = false)
	{
		Path = path;
		Description = description;
		CloseBeforeExecute = closeBeforeExecute;
	}
}
