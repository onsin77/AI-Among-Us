using UnityEngine;

public class DoorConsole : MonoBehaviour, IUsable
{
	public Minigame MinigamePrefab;

	private OpenableDoor MyDoor;

	private SpriteRenderer Image;

	public ImageNames UseIcon => ImageNames.UseButton;

	public float UsableDistance => 1f;

	public float PercentCool => 0f;

	public void Awake()
	{
		MyDoor = ((Component)this).GetComponent<OpenableDoor>();
		Image = ((Component)this).GetComponent<SpriteRenderer>();
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(pc.Object.GetTruePosition(), Vector2.op_Implicit(((Component)this).transform.position));
		couldUse = !pc.IsDead && !MyDoor.IsOpen;
		canUse = couldUse && num <= UsableDistance;
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Image))
		{
			((Renderer)Image).material.SetFloat("_Outline", (float)(on ? 1 : 0));
			((Renderer)Image).material.SetColor("_OutlineColor", Color.white);
			((Renderer)Image).material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
		}
	}

	public void Use()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			PlayerControl.LocalPlayer.NetTransform.Halt();
			Minigame minigame = Object.Instantiate<Minigame>(MinigamePrefab, ((Component)Camera.main).transform);
			((Component)minigame).transform.localPosition = new Vector3(0f, 0f, -50f);
			((IDoorMinigame)minigame).SetDoor(MyDoor);
			minigame.Begin(null);
		}
	}
}
