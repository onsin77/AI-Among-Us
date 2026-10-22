using System.Collections;
using UnityEngine;

public abstract class OverlayAnimation : MonoBehaviour
{
	protected const float TwoFramesDelay = 1f / 12f;

	public abstract IEnumerator CoShow(KillOverlay parent);
}
