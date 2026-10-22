using Rewired;
using UnityEngine;

[RequireComponent(typeof(Scroller))]
public class ManualScrollHelper : MonoBehaviour
{
	public bool doVertical = true;

	public RewiredConstsEnum.Action verticalAxis;

	public bool doHorizontal;

	public RewiredConstsEnum.Action horizontalAxis;

	public float scrollSpeed = 3f;

	private ScrollToSelection scrollToSelection;

	private Scroller scroller;

	private void Start()
	{
		scroller = ((Component)this).GetComponent<Scroller>();
		scrollToSelection = ((Component)this).GetComponent<ScrollToSelection>();
		if (Object.op_Implicit((Object)(object)scrollToSelection))
		{
			scrollToSelection.Toggle(on: false);
		}
		if (!Object.op_Implicit((Object)(object)scroller))
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(0);
		Vector2 dragDelta = default;
		float axisRaw = player.GetAxisRaw((int)verticalAxis);
		float axisRaw2 = player.GetAxisRaw((int)horizontalAxis);
		if (doVertical)
		{
			dragDelta.y = (0f - axisRaw) * Time.deltaTime * scrollSpeed;
		}
		if (doHorizontal)
		{
			dragDelta.x = (0f - axisRaw2) * Time.deltaTime * scrollSpeed;
		}
		if (axisRaw != 0f || axisRaw2 != 0f)
		{
			if ((Object)(object)scrollToSelection != (Object)null)
			{
				scrollToSelection.Toggle(on: false);
			}
			scroller.ScrollRelative(dragDelta);
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)scrollToSelection))
		{
			scrollToSelection.ClearSelections();
			((Behaviour)scrollToSelection).enabled = false;
			scrollToSelection.Toggle(on: false);
		}
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)scrollToSelection))
		{
			scrollToSelection.ClearSelections();
			((Behaviour)scrollToSelection).enabled = true;
			scrollToSelection.Toggle(on: false);
		}
	}
}
