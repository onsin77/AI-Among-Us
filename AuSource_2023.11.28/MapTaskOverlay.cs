using System.Collections.Generic;
using UnityEngine;

public class MapTaskOverlay : MonoBehaviour
{
	public ObjectPoolBehavior icons;

	private Dictionary<string, PooledMapIcon> data = new Dictionary<string, PooledMapIcon>();

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
		if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			return;
		}
		for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
		{
			PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[i];
			if (playerTask.HasLocation && !playerTask.IsComplete)
			{
				SetIconLocation(playerTask);
			}
		}
	}

	public void Update()
	{
		if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			return;
		}
		for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
		{
			PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[i];
			if (playerTask.HasLocation && !playerTask.IsComplete && playerTask.LocationDirty)
			{
				SetIconLocation(playerTask);
			}
		}
	}

	private void SetIconLocation(PlayerTask task)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		List<Vector2> locations = task.Locations;
		for (int i = 0; i < locations.Count; i++)
		{
			Vector3 localPosition = Vector2.op_Implicit(locations[i] / ShipStatus.Instance.MapScale);
			localPosition.z = -1f;
			PooledMapIcon pooledMapIcon = icons.Get<PooledMapIcon>();
			((Component)pooledMapIcon).transform.localScale = new Vector3(pooledMapIcon.NormalSize, pooledMapIcon.NormalSize, pooledMapIcon.NormalSize);
			if (PlayerTask.TaskIsEmergency(task))
			{
				pooledMapIcon.rend.color = Color.red;
				((Behaviour)pooledMapIcon.alphaPulse).enabled = true;
				((Renderer)pooledMapIcon.rend).material.SetFloat("_Outline", 1f);
			}
			else
			{
				pooledMapIcon.rend.color = Color.yellow;
			}
			((Object)pooledMapIcon).name = ((Object)task).name;
			pooledMapIcon.lastMapTaskStep = task.TaskStep;
			((Component)pooledMapIcon).transform.localPosition = localPosition;
			if (task.TaskStep > 0)
			{
				((Behaviour)pooledMapIcon.alphaPulse).enabled = true;
				((Renderer)pooledMapIcon.rend).material.SetFloat("_Outline", 1f);
			}
			string name = ((Object)task).name;
			name += i;
			if (!data.ContainsKey(name))
			{
				data.Add(name, pooledMapIcon);
			}
		}
	}

	public void Hide()
	{
		foreach (KeyValuePair<string, PooledMapIcon> datum in data)
		{
			datum.Value.OwnerPool.Reclaim(datum.Value);
		}
		data.Clear();
		((Component)this).gameObject.SetActive(false);
	}
}
