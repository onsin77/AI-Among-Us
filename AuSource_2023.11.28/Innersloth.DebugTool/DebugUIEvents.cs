using System;

namespace Innersloth.DebugTool;

public static class DebugUIEvents
{
	public static Action<DebugCommand> OnBeforeCommandExecuted;

	public static Action<bool> OnDetailsToggledVisible;

	public static Action<string> OnTabClicked;
}
