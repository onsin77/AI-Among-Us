using UnityEngine;

public class TargetFlashlight : MonoBehaviour
{
	private const float DOUBLE_CLICK_TIME = 0.3f;

	private bool isDragging;

	private float timeLastClick;

	private Controller myController = new Controller();

	private Collider2D col;

	private void Awake()
	{
		col = ((Component)this).GetComponent<Collider2D>();
	}

	private void Update()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		myController.Update();
		switch (myController.CheckDrag(col))
		{
		case DragState.TouchStart:
			isDragging = true;
			break;
		case DragState.Dragging:
		{
			Vector2 dragPosition = myController.DragPosition;
			((Component)this).transform.position = new Vector3(dragPosition.x, dragPosition.y, ((Component)this).transform.position.z);
			break;
		}
		case DragState.Released:
			isDragging = false;
			break;
		}
		if (!isDragging)
		{
			Controller.TouchState[] touches = myController.Touches;
			foreach (Controller.TouchState touchState in touches)
			{
				if (touchState.TouchStart)
				{
					if (Time.time - timeLastClick <= 0.3f)
					{
						Vector2 position = touchState.Position;
						((Component)this).transform.position = new Vector3(position.x, position.y, ((Component)this).transform.position.z);
					}
					timeLastClick = Time.time;
				}
			}
		}
		if (Input.GetMouseButtonDown(1))
		{
			Vector3 val = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			((Component)this).transform.position = new Vector3(val.x, val.y, ((Component)this).transform.position.z);
		}
	}
}
