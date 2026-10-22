using Innersloth.DebugTool;
using UnityEngine;

public class DebugAnalytics : DestroyableSingleton<DebugAnalytics>
{
	public static bool AnalyticsEnabled;

	private Texture2D consoleBackground;

	private GUIStyle debugStyle;

	private const string analyticsText = "Analytics Enabled";

	public IAnalyticsReporter Analytics { get; private set; }

	private void OnGUI()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (AnalyticsEnabled)
		{
			GUI.Label(new Rect(10f, 10f, 150f, 25f), "Analytics Enabled", debugStyle);
		}
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected Obj, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		CreateAnalyticsReporter();
		DebugTool.RegisterCommandHandler(this);
		consoleBackground = new Texture2D(1, 1, (TextureFormat)20, false);
		consoleBackground.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.25f));
		consoleBackground.Apply();
		debugStyle = new GUIStyle(GUIStyle.none);
		debugStyle.fontSize = 16;
		debugStyle.normal.textColor = Color.red;
		debugStyle.normal.background = consoleBackground;
	}

	private new void OnDestroy()
	{
		DebugTool.UnregisterCommandHandler(this);
		base.OnDestroy();
	}

	[DebugCommand("Analytics/Toggle Playtest Analytics", "", true)]
	public static void ToggleAnalytics()
	{
	}

	private void CreateAnalyticsReporter()
	{
		_ = AnalyticsEnabled;
		Analytics = new DummyAnalytics();
	}
}
