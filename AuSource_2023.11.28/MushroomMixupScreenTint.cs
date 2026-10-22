using System.Collections;
using UnityEngine;

public class MushroomMixupScreenTint : MonoBehaviour
{
	private static readonly int Opacity = Shader.PropertyToID("_Opacity");

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private float maxOpacity;

	private void Awake()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		((Renderer)meshRenderer).material = new Material(((Renderer)meshRenderer).sharedMaterial);
	}

	public void Activate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Component)this).transform.parent != (Object)(object)((Component)DestroyableSingleton<HudManager>.Instance).transform)
		{
			((Component)this).transform.SetParent(((Component)DestroyableSingleton<HudManager>.Instance).transform, false);
			((Component)this).transform.localPosition = new Vector3(0f, 0f, 10f);
		}
		((Renderer)meshRenderer).enabled = true;
		((MonoBehaviour)this).StartCoroutine(CoAnimateOpacity(0f, maxOpacity));
	}

	public void Deactivate()
	{
		((MonoBehaviour)this).StartCoroutine(CoAnimateOpacity(maxOpacity, 0f));
	}

	private IEnumerator CoAnimateOpacity(float startValue, float finalValue)
	{
		float currentValue = startValue;
		if (startValue < finalValue)
		{
			while (currentValue < finalValue)
			{
				((Renderer)meshRenderer).material.SetFloat(Opacity, currentValue);
				currentValue += Time.deltaTime;
				yield return null;
			}
			yield break;
		}
		while (currentValue > finalValue)
		{
			((Renderer)meshRenderer).material.SetFloat(Opacity, currentValue);
			currentValue -= Time.deltaTime;
			yield return null;
		}
		((Renderer)meshRenderer).enabled = false;
	}
}
