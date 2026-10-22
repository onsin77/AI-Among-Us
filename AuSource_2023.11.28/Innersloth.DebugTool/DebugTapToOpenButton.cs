using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Innersloth.DebugTool;

public class DebugTapToOpenButton : MonoBehaviour
{
	[SerializeField]
	private float maxSecondsBetweenTaps;

	[SerializeField]
	private DebugOverlay debugOverlay;

	[SerializeField]
	private Button tapButton;

	private float lastTapTime = -1f;

	private int numTapsSoFar;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		((UnityEvent)tapButton.onClick).AddListener((UnityAction)Tap);
		debugOverlay.AddToggleActiveCallback((bool visible) =>
		{
			((Component)tapButton).gameObject.SetActive(!visible);
		});
	}

	private void Tap()
	{
		float time = Time.time;
		if (numTapsSoFar > 0 && time - lastTapTime <= maxSecondsBetweenTaps)
		{
			numTapsSoFar++;
		}
		else
		{
			numTapsSoFar = 1;
		}
		lastTapTime = time;
		if (numTapsSoFar > 2)
		{
			numTapsSoFar = 0;
			debugOverlay.ToggleUIVisible(visible: true);
		}
	}
}
