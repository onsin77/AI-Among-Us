using UnityEngine;

namespace Innersloth.DebugTool;

public static class DebugColorExtensions
{
	public static Color SetAlpha(this Color c, float alpha)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new Color(c.r, c.g, c.b, alpha);
	}
}
