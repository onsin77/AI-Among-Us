using System.ComponentModel;
using System.Text.RegularExpressions;
using Rewired.Platforms;
using Rewired.Utils;
using Rewired.Utils.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rewired;

[AddComponentMenu("Rewired/Input Manager")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class InputManager : InputManager_Base
{
	private bool ignoreRecompile;

	protected override void OnInitialized()
	{
		SubscribeEvents();
	}

	protected override void OnDeinitialized()
	{
		UnsubscribeEvents();
	}

	protected override void DetectPlatform()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.scriptingBackend = (ScriptingBackend)0;
		base.scriptingAPILevel = (ScriptingAPILevel)0;
		base.editorPlatform = (EditorPlatform)0;
		base.platform = (Platform)0;
		base.webplayerPlatform = (WebplayerPlatform)0;
		base.isEditor = false;
		if (SystemInfo.deviceName == null)
		{
			_ = string.Empty;
		}
		if (SystemInfo.deviceModel == null)
		{
			_ = string.Empty;
		}
		base.platform = (Platform)1;
		base.scriptingBackend = (ScriptingBackend)2;
		base.scriptingAPILevel = (ScriptingAPILevel)2;
	}

	protected override void CheckRecompile()
	{
	}

	protected override IExternalTools GetExternalTools()
	{
		return (IExternalTools)(object)new ExternalTools();
	}

	private bool CheckDeviceName(string searchPattern, string deviceName, string deviceModel)
	{
		if (!Regex.IsMatch(deviceName, searchPattern, RegexOptions.IgnoreCase))
		{
			return Regex.IsMatch(deviceModel, searchPattern, RegexOptions.IgnoreCase);
		}
		return true;
	}

	private void SubscribeEvents()
	{
		UnsubscribeEvents();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void UnsubscribeEvents()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		((InputManager_Base)this).OnSceneLoaded();
	}
}
