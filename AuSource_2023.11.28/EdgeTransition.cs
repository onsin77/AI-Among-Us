using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EdgeTransition : MonoBehaviour
{
	public float Duration = 0.2f;

	public Vector3 OpenPosition;

	public Vector3 ClosedPosition;

	public AspectPosition.EdgeAlignments Alignment;

	public ButtonClickedEvent OnClose = new ButtonClickedEvent();

	public void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = AspectPosition.ComputePosition(Alignment, ClosedPosition);
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(CoOpen());
	}

	private IEnumerator CoOpen()
	{
		Vector3 sourcePos = ((Component)this).transform.localPosition;
		Vector3 targetPos = AspectPosition.ComputePosition(Alignment, OpenPosition);
		for (float timer = 0f; timer < Duration; timer += Time.deltaTime)
		{
			Vector3 localPosition = Vector3.Lerp(sourcePos, targetPos, timer / Duration);
			((Component)this).transform.localPosition = localPosition;
			yield return null;
		}
		((Component)this).transform.localPosition = targetPos;
	}

	public void Close()
	{
		if (((Behaviour)this).isActiveAndEnabled)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CoClose());
		}
	}

	private IEnumerator CoClose()
	{
		Vector3 sourcePos = ((Component)this).transform.localPosition;
		Vector3 targetPos = AspectPosition.ComputePosition(Alignment, ClosedPosition);
		for (float timer = 0f; timer < Duration; timer += Time.deltaTime)
		{
			Vector3 localPosition = Vector3.Lerp(sourcePos, targetPos, timer / Duration);
			((Component)this).transform.localPosition = localPosition;
			yield return null;
		}
		((Component)this).transform.localPosition = targetPos;
		((UnityEvent)OnClose).Invoke();
	}

	public EdgeTransition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
	}
}
