using UnityEngine;

namespace Innersloth.DebugTool;

public readonly struct DebugLogMessage(string logString, string stacktrace, LogType logType)
{
	public readonly string LogString = logString;

	public readonly string Stacktrace = stacktrace;

	public readonly LogType LogType = logType;
}
