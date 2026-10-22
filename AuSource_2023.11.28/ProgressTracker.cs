using System.Linq;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
	public MeshRenderer TileParent;

	private float curValue;

	public void Start()
	{
		if (GameManager.Instance.IsHideAndSeek())
		{
			((Component)this).gameObject.SetActive(false);
		}
		((Renderer)TileParent).material.SetFloat("_Buckets", 1f);
		((Renderer)TileParent).material.SetFloat("_FullBuckets", 0f);
	}

	public void FixedUpdate()
	{
		if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			((Renderer)TileParent).enabled = false;
			return;
		}
		if (!((Renderer)TileParent).enabled)
		{
			((Renderer)TileParent).enabled = true;
		}
		GameData instance = GameData.Instance;
		if (!Object.op_Implicit((Object)(object)instance) || instance.TotalTasks <= 0)
		{
			return;
		}
		int num = (DestroyableSingleton<TutorialManager>.InstanceExists ? 1 : (instance.AllPlayers.Count - GameManager.Instance.LogicOptions.NumImpostors));
		num -= instance.AllPlayers.Count((GameData.PlayerInfo p) => p.Disconnected);
		switch (GameManager.Instance.LogicOptions.GetTaskBarMode())
		{
		case TaskBarMode.Normal:
		{
			float num2 = (float)instance.CompletedTasks / (float)instance.TotalTasks * (float)num;
			curValue = Mathf.Lerp(curValue, num2, Time.fixedDeltaTime * 2f);
			break;
		}
		case TaskBarMode.MeetingOnly:
			if (!Object.op_Implicit((Object)(object)MeetingHud.Instance))
			{
				break;
			}
			goto case TaskBarMode.Normal;
		case TaskBarMode.Invisible:
			((Component)this).gameObject.SetActive(false);
			break;
		}
		((Renderer)TileParent).material.SetFloat("_Buckets", (float)num);
		((Renderer)TileParent).material.SetFloat("_FullBuckets", curValue);
	}
}
