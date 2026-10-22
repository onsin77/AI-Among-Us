using System.Runtime.CompilerServices;
using UnityEngine;

public static class CameraSafeArea
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float GetSafeOrthographicSize(Camera cam)
	{
		return cam.orthographicSize;
	}
}
