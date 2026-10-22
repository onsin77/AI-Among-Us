using System.Collections.Generic;
using UnityEngine;

public class SlicedAspectScaler : MonoBehaviour
{
	private const float NORMAL_ASPECT = 1.777f;

	[SerializeField]
	private List<AspectScaledAsset> objectsToScale;

	[SerializeField]
	private bool updateAlways;

	private Camera parentCam;

	private void Start()
	{
		SetParentCam();
		float aspectDiff = parentCam.aspect / 1.777f;
		objectsToScale.ForEach((AspectScaledAsset ob) =>
		{
			ob.ScaleObject(aspectDiff);
		});
	}

	private void SetParentCam()
	{
		parentCam = (Object.op_Implicit((Object)(object)parentCam) ? parentCam : Camera.main);
	}

	private void Update()
	{
		if (updateAlways)
		{
			if ((Object)(object)parentCam == (Object)null)
			{
				SetParentCam();
			}
			float aspectDiff = parentCam.aspect / 1.777f;
			objectsToScale.ForEach((AspectScaledAsset ob) =>
			{
				ob.ScaleObject(aspectDiff);
			});
		}
	}
}
