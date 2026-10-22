using System;

namespace Innersloth.DebugTool;

public class DebugNullKeyboardInputProvider : IDebugToolKeyboardInputProvider
{
	public void GetText(string originalText, int characterLimit, Action<string> onComplete)
	{
		onComplete(originalText);
	}
}
