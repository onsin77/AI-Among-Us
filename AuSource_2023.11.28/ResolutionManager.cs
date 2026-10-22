using UnityEngine;

public static class ResolutionManager
{
	public delegate void ResolutionChangedHandler(float aspectRatio, int width, int height, bool fullscreen);

	public static event ResolutionChangedHandler ResolutionChanged;

	public static void SetResolution(int width, int height, bool fullscreen)
	{
		Screen.SetResolution(width, height, fullscreen);
		DestroyableSingleton<CoroutineHelper>.Instance.DelayByFrame(() =>
		{
			ResolutionChanged?.Invoke((float)width / (float)height, width, height, fullscreen);
		});
	}

	public static void ToggleFullscreen()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !Screen.fullScreen;
		int width;
		int height;
		if (flag)
		{
			Resolution[] resolutions = Screen.resolutions;
			Resolution val = resolutions[0];
			for (int i = 0; i < resolutions.Length; i++)
			{
				if (val.height < resolutions[i].height)
				{
					val = resolutions[i];
				}
			}
			width = val.width;
			height = val.height;
		}
		else
		{
			width = 711;
			height = 400;
		}
		SetResolution(width, height, flag);
	}
}
