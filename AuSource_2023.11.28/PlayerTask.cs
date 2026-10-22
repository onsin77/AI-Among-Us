using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class PlayerTask : MonoBehaviour
{
	public SystemTypes StartAt;

	public TaskTypes TaskType;

	public Minigame MinigamePrefab;

	public bool HasLocation;

	public bool LocationDirty = true;

	public int Index { get; internal set; }

	public uint Id { get; internal set; }

	public PlayerControl Owner { get; internal set; }

	public abstract int TaskStep { get; }

	public abstract bool IsComplete { get; }

	public List<Vector2> Locations
	{
		get
		{
			LocationDirty = false;
			return FindConsolesPos();
		}
	}

	public abstract void Initialize();

	public virtual void OnRemove()
	{
	}

	public abstract bool ValidConsole(Console console);

	public abstract void Complete();

	public abstract void AppendTaskText(StringBuilder sb);

	internal static bool TaskIsEmergency(PlayerTask arg)
	{
		if (!(arg is NoOxyTask) && !(arg is IHudOverrideTask) && !(arg is ReactorTask) && !(arg is HeliCharlesTask))
		{
			return arg is ElectricTask;
		}
		return true;
	}

	protected List<Console> FindConsoles()
	{
		List<Console> list = new List<Console>();
		Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (ValidConsole(allConsoles[i]))
			{
				list.Add(allConsoles[i]);
			}
		}
		return list;
	}

	public static T GetOrCreateTask<T>(PlayerControl localPlayer, int insertAt = 0) where T : PlayerTask
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < localPlayer.myTasks.Count; i++)
		{
			PlayerTask playerTask = localPlayer.myTasks[i];
			if (playerTask is T)
			{
				return (T)playerTask;
			}
		}
		T val = new GameObject("T").AddComponent<T>();
		((Component)val).transform.SetParent(((Component)localPlayer).transform, false);
		localPlayer.myTasks.Insert(0, val);
		return val;
	}

	public static bool DestroyTasksOfType<T>(PlayerControl localPlayer)
	{
		bool result = false;
		for (int num = localPlayer.myTasks.Count - 1; num >= 0; num--)
		{
			PlayerTask playerTask = localPlayer.myTasks[num];
			if (playerTask is T)
			{
				result = true;
				Object.Destroy((Object)(object)((Component)playerTask).gameObject);
				localPlayer.myTasks.RemoveAt(num);
			}
		}
		return result;
	}

	public static bool TryGetTaskOfType<T>(PlayerControl localPlayer, out T t)
	{
		if (!Object.op_Implicit((Object)(object)localPlayer))
		{
			t = default;
			return true;
		}
		foreach (PlayerTask myTask in localPlayer.myTasks)
		{
			if (myTask is T val)
			{
				t = val;
				return true;
			}
		}
		t = default;
		return false;
	}

	public static bool PlayerHasTaskOfType<T>(PlayerControl localPlayer)
	{
		T t;
		return TryGetTaskOfType<T>(localPlayer, out t);
	}

	public List<Vector2> FindValidConsolesPositions()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		List<Vector2> list = new List<Vector2>();
		Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (ValidConsole(allConsoles[i]))
			{
				list.Add(Vector2.op_Implicit(((Component)allConsoles[i]).transform.position));
			}
		}
		return list;
	}

	protected Console FindSpecialConsole(Func<Console, bool> func)
	{
		Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (func(allConsoles[i]))
			{
				return allConsoles[i];
			}
		}
		return null;
	}

	protected Console FindObjectPos()
	{
		Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (ValidConsole(allConsoles[i]))
			{
				return allConsoles[i];
			}
		}
		Debug.LogError((object)("Couldn't find location for task: " + ((Object)this).name));
		return null;
	}

	protected List<Vector2> FindConsolesPos()
	{
		List<Vector2> list = FindValidConsolesPositions();
		if (list.Count == 0)
		{
			throw new Exception($"Could not find console positions for this task {((Object)this).name} {TaskType}");
		}
		return list;
	}

	public virtual Minigame GetMinigamePrefab()
	{
		return MinigamePrefab;
	}

	protected static bool AllTasksCompleted(PlayerControl player)
	{
		for (int i = 0; i < player.myTasks.Count; i++)
		{
			PlayerTask playerTask = player.myTasks[i];
			if (playerTask is NormalPlayerTask && !playerTask.IsComplete)
			{
				return false;
			}
		}
		return true;
	}
}
