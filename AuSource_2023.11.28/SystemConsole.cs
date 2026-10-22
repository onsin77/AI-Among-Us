using UnityEngine;

public class SystemConsole : MonoBehaviour, IUsable
{
	public ImageNames useIcon = ImageNames.UseButton;

	public Vector2 SafePositionLocal;

	public float usableDistance = 1f;

	public bool FreeplayOnly;

	public bool onlyFromBelow;

	public SpriteRenderer Image;

	public Minigame MinigamePrefab;

	public ImageNames UseIcon => useIcon;

	public float UsableDistance => usableDistance;

	public float PercentCool => 0f;

	public void Start()
	{
		if (FreeplayOnly && !DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
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

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		Vector2 truePosition = playerControl.GetTruePosition();
		couldUse = playerControl.CanMove && ((Behaviour)this).enabled && (!pc.IsDead || !(MinigamePrefab is EmergencyMinigame));
		canUse = couldUse && (!onlyFromBelow || truePosition.y < ((Component)this).transform.position.y);
		if (canUse)
		{
			num = Vector2.Distance(truePosition, Vector2.op_Implicit(((Component)this).transform.position));
			canUse &= num <= UsableDistance;
		}
		return num;
	}

	public void Use()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			PlayerControl.LocalPlayer.NetTransform.Halt();
			Minigame minigame = Object.Instantiate<Minigame>(MinigamePrefab);
			if (minigame is TaskAdderGame taskAdderGame)
			{
				taskAdderGame.SafePositionWorld = SafePositionLocal + Vector2.op_Implicit(((Component)this).transform.position);
			}
			((Component)minigame).transform.SetParent(((Component)Camera.main).transform, false);
			((Component)minigame).transform.localPosition = new Vector3(0f, 0f, -50f);
			minigame.Begin(null);
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
