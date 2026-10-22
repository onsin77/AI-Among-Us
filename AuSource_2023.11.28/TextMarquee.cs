using System.Collections;
using TMPro;
using UnityEngine;

public class TextMarquee : MonoBehaviour
{
	public TextMeshPro Target;

	private MeshRenderer[] allRenderers;

	private int lastChildCount;

	private string targetText;

	public float ScrollSpeed = 1f;

	public float PauseTime = 1f;

	public float AreaWidth = 3f;

	public bool IgnoreTextChanges;

	public void Start()
	{
		((MonoBehaviour)this).StartCoroutine(Run());
	}

	private void UpdateRendererList()
	{
		lastChildCount = Target.transform.childCount;
		allRenderers = ((Component)Target).gameObject.GetComponentsInChildren<MeshRenderer>();
		allRenderers.ForEach((MeshRenderer render) =>
		{
			((Renderer)render).material.SetInt("_Mask", 4);
		});
	}

	private void Update()
	{
		if (Target.transform.childCount != lastChildCount)
		{
			UpdateRendererList();
		}
	}

	private IEnumerator Run()
	{
		yield return null;
		UpdateRendererList();
		int i = 0;
		while (i < 1000)
		{
			Vector4 temp = default;
			targetText = ((TMP_Text)Target).text;
			allRenderers.ForEach((MeshRenderer render) =>
			{
				((Renderer)render).material.SetFloat("_VertexOffsetX", temp.x);
				((Renderer)render).material.SetFloat("_VertexOffsetY", temp.y);
			});
			for (float timer = 0f; timer < PauseTime; timer += Time.deltaTime)
			{
				if (!IgnoreTextChanges && targetText != ((TMP_Text)Target).text)
				{
					break;
				}
				yield return null;
			}
			for (float timer = 0f; timer < 100f; timer += Time.deltaTime)
			{
				if (!IgnoreTextChanges && targetText != ((TMP_Text)Target).text)
				{
					break;
				}
				temp.x -= ScrollSpeed * Time.deltaTime;
				allRenderers.ForEach((MeshRenderer render) =>
				{
					((Renderer)render).material.SetFloat("_VertexOffsetX", temp.x);
					((Renderer)render).material.SetFloat("_VertexOffsetY", temp.y);
				});
				Bounds bounds = ((TMP_Text)Target).bounds;
				if (bounds.size.x + temp.x < AreaWidth)
				{
					break;
				}
				yield return null;
			}
			for (float timer = 0f; timer < PauseTime; timer += Time.deltaTime)
			{
				if (!IgnoreTextChanges && targetText != ((TMP_Text)Target).text)
				{
					break;
				}
				yield return null;
			}
			yield return null;
			int num = i + 1;
			i = num;
		}
	}
}
