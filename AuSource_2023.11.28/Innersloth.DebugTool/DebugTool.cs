using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Innersloth.DebugTool;

public static class DebugTool
{
	private static readonly List<DebugCommand> AllCommands = new List<DebugCommand>();

	private static readonly DebugCircleBuffer<DebugLogMessage> AllLogMessages = new DebugCircleBuffer<DebugLogMessage>(1000);

	private static readonly Dictionary<Type, Func<string, object>> CustomParsers = new Dictionary<Type, Func<string, object>>();

	public static IReadOnlyList<DebugCommand> Commands => AllCommands;

	public static IEnumerator<DebugLogMessage> Logs => AllLogMessages.GetEnumerator();

	public static bool ListeningToLogs { get; private set; }

	public static void RegisterCommandHandler(object obj)
	{
		RegisterCommandHandler(obj.GetType(), obj);
	}

	public static void RegisterCommandHandler(Type t, object target = null)
	{
	}

	public static void UnregisterCommandHandler(Type t)
	{
		MethodInfo[] methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < methods.Length; i++)
		{
			DebugCommandAttribute customAttribute = methods[i].GetCustomAttribute<DebugCommandAttribute>();
			if (customAttribute != null)
			{
				RemoveCommand(customAttribute.Path);
			}
		}
	}

	public static void UnregisterCommandHandler(object obj)
	{
		UnregisterCommandHandler(obj.GetType());
	}

	public static DebugCommand AddCommand(string commandPath, Action commandAction, bool closeBeforeExecute = false)
	{
		DebugCommand debugCommand = new DebugCommand(commandPath, commandAction.Target, commandAction.Method)
		{
			CloseBeforeExecute = closeBeforeExecute
		};
		AddCommand(debugCommand);
		return debugCommand;
	}

	public static DebugCommand AddCommand<T>(string commandPath, Action<T> commandAction, bool closeBeforeExecute = false)
	{
		DebugCommand debugCommand = new DebugCommand(commandPath, commandAction.Target, commandAction.Method)
		{
			CloseBeforeExecute = closeBeforeExecute
		};
		AddCommand(debugCommand);
		return debugCommand;
	}

	public static DebugCommand AddCommand<T1, T2>(string commandPath, Action<T1, T2> commandAction, bool closeBeforeExecute = false)
	{
		DebugCommand debugCommand = new DebugCommand(commandPath, commandAction.Target, commandAction.Method)
		{
			CloseBeforeExecute = closeBeforeExecute
		};
		AddCommand(debugCommand);
		return debugCommand;
	}

	public static void AddCommand(DebugCommand command)
	{
	}

	public static void RemoveCommand(string commandPath)
	{
	}

	public static void AddCustomParser<T>(Func<string, T> parser)
	{
		Type typeFromHandle = typeof(T);
		if (!CustomParsers.ContainsKey(typeFromHandle))
		{
			CustomParsers.Add(typeFromHandle, (string str) => parser(str));
		}
		else
		{
			Debug.LogError((object)$"DebugTool already contained a parser for type {typeFromHandle}");
		}
	}

	public static object ParseInputToType(string rawValue, string fieldName, Type type)
	{
		try
		{
			if (CustomParsers.ContainsKey(type))
			{
				return CustomParsers[type](rawValue);
			}
			if (type == typeof(int))
			{
				return int.Parse(rawValue);
			}
			if (type == typeof(float))
			{
				return float.Parse(rawValue);
			}
			if (type == typeof(bool))
			{
				return bool.Parse(rawValue);
			}
		}
		catch (FormatException)
		{
			Debug.LogError((object)string.Format("[{0}] Format error: Expected type {1} in field '{2}' and was not able to parse it from the raw value: '{3}'", "DebugTool", type, fieldName, rawValue));
			return null;
		}
		if (type == typeof(string))
		{
			return rawValue;
		}
		Debug.LogError((object)string.Format("{0} Not implemented: Command parameters of type {1} are not supported.", "DebugTool", type));
		return null;
	}

	public static void ListenToLogs()
	{
	}

	private static void LogMessageReceived(string log, string stacktrace, LogType type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		AllLogMessages.Add(new DebugLogMessage(log, stacktrace, type));
	}

	public static void ClearLogs()
	{
		AllLogMessages.Clear();
	}
}
