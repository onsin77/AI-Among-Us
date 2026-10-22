using System.Collections.Generic;
using UnityEngine;

public class CounterArea : MonoBehaviour
{
	public SystemTypes RoomType;

	public ObjectPoolBehavior pool;

	private List<PoolableBehavior> myIcons = new List<PoolableBehavior>();

	public float XOffset = 0.3f;

	public float YOffset = 0.3f;

	public int MaxWidth = 5;

	public int MaxColumns = 5;

	public void UpdateCount(int cnt)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = myIcons.Count != cnt;
		while (myIcons.Count < cnt)
		{
			PoolableBehavior item = pool.Get<PoolableBehavior>();
			myIcons.Add(item);
		}
		while (myIcons.Count > cnt)
		{
			PoolableBehavior poolableBehavior = myIcons[myIcons.Count - 1];
			myIcons.RemoveAt(myIcons.Count - 1);
			poolableBehavior.OwnerPool.Reclaim(poolableBehavior);
		}
		if (flag)
		{
			for (int i = 0; i < myIcons.Count; i++)
			{
				int num = i % MaxColumns;
				int num2 = i / MaxColumns;
				float num3 = (float)(Mathf.Min(cnt - num2 * MaxColumns, MaxColumns) - 1) * XOffset / -2f;
				((Component)myIcons[i]).transform.position = ((Component)this).transform.position + new Vector3(num3 + (float)num * XOffset, (float)num2 * YOffset, -1f);
			}
		}
	}
}
