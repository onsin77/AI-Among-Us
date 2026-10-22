using Hazel;
using UnityEngine;

namespace InnerNet;

public class UnityLogger : ILogger
{
	public void WriteVerbose(string msg)
	{
	}

	public void WriteWarning(string msg)
	{
		Debug.LogWarning((object)msg);
	}

	public void WriteError(string msg)
	{
		Debug.LogError((object)msg);
	}

	public void WriteInfo(string msg)
	{
		Debug.Log((object)msg);
	}
}
