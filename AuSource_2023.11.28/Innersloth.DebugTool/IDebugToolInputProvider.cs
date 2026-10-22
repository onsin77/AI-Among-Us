using System.Collections;

namespace Innersloth.DebugTool;

public interface IDebugToolInputProvider
{
	IEnumerator CoInitialize();

	bool GetToggle();

	float GetVertical();

	bool GetCopy();

	bool GetClear();

	bool GetConfirm();

	bool GetCancel();

	bool GetNavLeft();

	bool GetNavRight();
}
