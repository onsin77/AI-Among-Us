using System;

namespace Innersloth.DebugTool;

[AttributeUsage(AttributeTargets.Parameter)]
public class DebugParameterAttribute : Attribute
{
	public string Placeholder;

	public DebugParameterAttribute(string placeholder)
	{
		Placeholder = placeholder;
	}
}
