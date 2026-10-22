using System.Collections;
using UnityEngine;

public class ShadowCollab : MonoBehaviour
{
	public Camera ShadowCamera;

	public MeshRenderer ShadowQuad;

	private float oldAspect;

	public void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine(Run());
	}

	private IEnumerator Run()
	{
		Camera cam = Camera.main;
		while (true)
		{
			if (oldAspect != cam.aspect)
			{
				oldAspect = cam.aspect;
				ShadowCamera.aspect = cam.aspect;
				ShadowCamera.orthographicSize = cam.orthographicSize;
				((Component)ShadowQuad).transform.localScale = new Vector3(cam.orthographicSize * cam.aspect, cam.orthographicSize) * 2f;
			}
			yield return Effects.Wait(1f);
		}
	}
}
