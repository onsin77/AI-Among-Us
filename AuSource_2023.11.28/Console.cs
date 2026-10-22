using Assets.CoreScripts;
using UnityEngine;

public class Console : MonoBehaviour, IUsable
{
	public float usableDistance = 1f;

	public int ConsoleId;

	public bool onlyFromBelow;

	public bool onlySameRoom;

	public bool checkWalls;

	public bool GhostsIgnored;

	public bool AllowImpostor;

	public SystemTypes Room;

	public TaskTypes[] TaskTypes;

	public TaskSet[] ValidTasks;

	public SpriteRenderer Image;

	public ImageNames UseIcon => ImageNames.UseButton;

	public float UsableDistance => usableDistance;

	public float PercentCool => 0f;

	public void SetOutline(bool on, bool mainTarget)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Image))
		{
			((Renderer)Image).material.SetFloat("_Outline", (float)(on ? 1 : 0));
			((Renderer)Image).material.SetColor("_OutlineColor", Color.yellow);
			((Renderer)Image).material.SetColor("_AddColor", mainTarget ? Color.yellow : Color.clear);
		}
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		Vector2 truePosition = playerControl.GetTruePosition();
		Vector3 position = ((Component)this).transform.position;
		couldUse = (!pc.IsDead || (GameManager.Instance.LogicOptions.GetGhostsDoTasks() && !GhostsIgnored)) && playerControl.CanMove && pc.Role.CanUse(this) && (!onlySameRoom || InRoom(truePosition)) && (!onlyFromBelow || truePosition.y < position.y) && Object.op_Implicit((Object)(object)FindTask(playerControl));
		canUse = couldUse;
		if (canUse)
		{
			num = Vector2.Distance(truePosition, Vector2.op_Implicit(((Component)this).transform.position));
			canUse &= num <= UsableDistance;
			if (checkWalls)
			{
				canUse &= !PhysicsHelpers.AnythingBetween(truePosition, Vector2.op_Implicit(position), Constants.ShadowMask, useTriggers: false);
			}
		}
		return num;
	}

	private bool InRoom(Vector2 truePos)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		PlainShipRoom plainShipRoom = ShipStatus.Instance.FastRooms[Room];
		if (!Object.op_Implicit((Object)(object)plainShipRoom) || !Object.op_Implicit((Object)(object)plainShipRoom.roomArea))
		{
			return false;
		}
		try
		{
			return plainShipRoom.roomArea.OverlapPoint(truePos);
		}
		catch
		{
			return false;
		}
	}

	private PlayerTask FindTask(PlayerControl pc)
	{
		for (int i = 0; i < pc.myTasks.Count; i++)
		{
			PlayerTask playerTask = pc.myTasks[i];
			if (!playerTask.IsComplete && playerTask.ValidConsole(this))
			{
				return playerTask;
			}
		}
		return null;
	}

	public virtual void Use()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			PlayerTask playerTask = FindTask(localPlayer);
			if (Object.op_Implicit((Object)(object)playerTask.MinigamePrefab))
			{
				Logger.GlobalInstance.Info("Using console " + ((Object)this).name + " for minigame " + ((Object)playerTask.MinigamePrefab).name);
				Minigame minigame = Object.Instantiate<Minigame>(playerTask.GetMinigamePrefab());
				((Component)minigame).transform.SetParent(((Component)Camera.main).transform, false);
				((Component)minigame).transform.localPosition = new Vector3(0f, 0f, -50f);
				minigame.Console = this;
				minigame.Begin(playerTask);
				DestroyableSingleton<UnityTelemetry>.Instance.WriteUse(localPlayer.PlayerId, playerTask.TaskType, ((Component)this).transform.position);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Color color = Gizmos.color;
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(((Component)this).transform.position, usableDistance);
		Gizmos.color = color;
	}
}
