using System;

namespace Innersloth.DebugTool;

public interface IDebugToolKeyboardInputProvider
{
	void GetText(string originalText, int characterLimit, Action<string> onComplete);
}
