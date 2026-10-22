using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridArrange : MonoBehaviour
{
	public enum StartAlign
	{
		Left,
		Right
	}

	public Vector2 CellSize;

	public StartAlign Alignment;

	public int MaxColumns = 3;

	private List<Transform> cells;

	private static List<Transform> currentChildren = new List<Transform>();

	private void Start()
	{
		cells = new List<Transform>();
		GetChildsActive();
		CheckCurrentChildren();
	}

	private void FixedUpdate()
	{
		CheckCurrentChildren();
	}

	private void CheckCurrentChildren()
	{
		GetChildsActive();
		if (cells.SequenceEqual(currentChildren))
		{
			return;
		}
		cells.Clear();
		foreach (Transform currentChild in currentChildren)
		{
			cells.Add(currentChild);
		}
		ArrangeChilds();
	}

	private void GetChildsActive()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
		currentChildren.Clear();
		foreach (Transform item in ((Component)this).transform)
		{
			Transform val = item;
			if (((Component)val).gameObject.activeSelf)
			{
				currentChildren.Add(val);
			}
		}
	}

	private void ArrangeChilds()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Alignment == StartAlign.Left) ? (0f - CellSize.x) : CellSize.x);
		for (int i = 0; i < cells.Count; i++)
		{
			float num2 = (float)(i % MaxColumns) * num;
			num2 += ((Component)this).transform.position.x;
			float num3 = Mathf.Floor((float)(i / MaxColumns)) * CellSize.y;
			num3 += ((Component)this).transform.position.y;
			Transform val = cells[i];
			val.position = new Vector3(num2, num3, val.position.z);
		}
	}
}
