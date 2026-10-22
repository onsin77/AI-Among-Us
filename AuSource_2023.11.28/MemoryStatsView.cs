using System.Text;
using Unity.Profiling;
using UnityEngine;

public class MemoryStatsView : MonoBehaviour
{
	private string statsText;

	private ProfilerRecorder systemMemoryRecorder;

	private ProfilerRecorder gcMemoryRecorder;

	private ProfilerRecorder textureMemoryRecorder;

	private Texture2D consoleBackground;

	private GUIStyle debugStyle;

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected Obj, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		consoleBackground = new Texture2D(1, 1, (TextureFormat)20, false);
		consoleBackground.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.25f));
		consoleBackground.Apply();
		debugStyle = new GUIStyle(GUIStyle.none);
		debugStyle.fontSize = 12;
		debugStyle.normal.textColor = Color.white;
		debugStyle.normal.background = consoleBackground;
	}

	private void OnEnable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 1, (ProfilerRecorderOptions)24);
		gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory", 1, (ProfilerRecorderOptions)24);
		textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory", 1, (ProfilerRecorderOptions)24);
	}

	private void OnDisable()
	{
		systemMemoryRecorder.Dispose();
		gcMemoryRecorder.Dispose();
		textureMemoryRecorder.Dispose();
	}

	private void Update()
	{
		StringBuilder stringBuilder = new StringBuilder(500);
		stringBuilder.AppendLine($"GC Memory: {gcMemoryRecorder.LastValue / 1048576} MB");
		stringBuilder.AppendLine($"System Memory: {systemMemoryRecorder.LastValue / 1048576} MB");
		stringBuilder.AppendLine($"Texture Memory: {textureMemoryRecorder.LastValue / 1048576} MB");
		statsText = stringBuilder.ToString();
	}

	private void OnGUI()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		GUI.Label(new Rect(10f, 30f, 250f, 50f), statsText, debugStyle);
	}
}
