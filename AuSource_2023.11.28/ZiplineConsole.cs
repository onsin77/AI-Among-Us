using UnityEngine;

public class ZiplineConsole : MonoBehaviour, IUsableCoolDown, IUsable
{
	[SerializeField]
	private float usableDistance = 0.5f;

	[SerializeField]
	private SpriteRenderer image;

	[SerializeField]
	private ZiplineBehaviour zipline;

	[SerializeField]
	private bool atTop;

	[SerializeField]
	private ZiplineConsole destination;

	public float UsableDistance => usableDistance;

	public float PercentCool => CoolDown / 3f;

	public ImageNames UseIcon => ImageNames.UseButton;

	public float CoolDown { get; set; }

	public float MaxCoolDown => 3f;

	public bool IsCoolingDown()
	{
		return CoolDown > 0f;
	}

	private void Update()
	{
		CoolDown = Mathf.Max(CoolDown - Time.deltaTime, 0f);
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)image).material.SetFloat("_Outline", (float)(on ? 1 : 0));
		((Renderer)image).material.SetColor("_OutlineColor", Color.white);
		((Renderer)image).material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		couldUse = !pc.IsDead && playerControl.CanMove && !playerControl.isKilling;
		canUse = couldUse;
		if (canUse)
		{
			Vector2 truePosition = playerControl.GetTruePosition();
			Vector3 position = ((Component)this).transform.position;
			num = Vector2.Distance(truePosition, Vector2.op_Implicit(position));
			canUse &= num <= UsableDistance && !PhysicsHelpers.AnythingBetween(truePosition, Vector2.op_Implicit(position), Constants.ShipOnlyMask, useTriggers: false);
		}
		return num;
	}

	public void Use()
	{
		if (!IsCoolingDown())
		{
			CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
			if (canUse)
			{
				zipline.Use(atTop, this);
				CoolDown = MaxCoolDown;
			}
		}
	}

	public void SetDestinationCooldown()
	{
		destination.CoolDown = MaxCoolDown;
	}
}
