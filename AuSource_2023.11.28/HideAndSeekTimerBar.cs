using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class HideAndSeekTimerBar : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro timeText;

	[SerializeField]
	private Transform timerBar;

	[SerializeField]
	private MeshRenderer timerBarRenderer;

	[SerializeField]
	private Transform chunkBar;

	private float targetBarSize;

	private bool freezeChunk;

	private Coroutine chunkCoroutine;

	private void Update()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.deltaTime * 10f;
		Vector3 localScale = timerBar.localScale;
		localScale.x = Mathf.Lerp(localScale.x, targetBarSize, num);
		timerBar.localScale = localScale;
		if (!freezeChunk)
		{
			localScale = chunkBar.localScale;
			localScale.x = Mathf.Lerp(localScale.x, targetBarSize, num);
			chunkBar.localScale = localScale;
		}
	}

	public void UpdateTimer(float time, float maxTime)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		((TMP_Text)timeText).text = timeSpan.ToString("m\\:ss");
		targetBarSize = Mathf.Clamp01(time / maxTime);
	}

	public void StartFinalHide()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)timerBarRenderer).material = new Material(((Renderer)timerBarRenderer).material);
		((Renderer)timerBarRenderer).material.SetColor("_Color", Palette.ImpostorRed);
		StopChunkCoroutine();
		((Component)chunkBar).gameObject.SetActive(false);
	}

	public void TaskComplete()
	{
		StartChunkCoroutine();
	}

	private void StartChunkCoroutine()
	{
		StopChunkCoroutine();
		chunkCoroutine = ((MonoBehaviour)this).StartCoroutine(ChunkCoroutine());
	}

	private void StopChunkCoroutine()
	{
		if (chunkCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(chunkCoroutine);
			chunkCoroutine = null;
		}
	}

	private IEnumerator ChunkCoroutine()
	{
		freezeChunk = true;
		yield return (object)new WaitForSeconds(1f);
		freezeChunk = false;
		chunkCoroutine = null;
	}
}
