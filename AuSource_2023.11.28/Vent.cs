using System.Collections;
using PowerTools;
using UnityEngine;

public class Vent : MonoBehaviour, IUsable
{
	private const float VIBRATION_INTENSITY = 0.4f;

	private const float VIBRATION_FALLOFF_DISTANCE = 3.5f;

	private const float VIBRATION_DURATION = 0.2f;

	public static Vent currentVent;

	public int Id;

	public Vent Left;

	public Vent Right;

	public Vent Center;

	public ButtonBehavior[] Buttons;

	public GameObject[] CleaningIndicators;

	public AnimationClip EnterVentAnim;

	public AnimationClip ExitVentAnim;

	public Vector3 Offset = new Vector3(0f, 0.3636057f, 0f);

	public float spreadAmount;

	public float spreadShift;

	[SerializeField]
	private SpriteRenderer myRend;

	[SerializeField]
	private SpriteAnim myAnim;

	[SerializeField]
	private int numFramesUntilPlayerDisappearsOnEnter;

	[SerializeField]
	private int numFramesUntilPlayerReappearsOnExit;

	[SerializeField]
	private GameObject additionalExitAnimation;

	public ImageNames UseIcon => ImageNames.VentButton;

	public float UsableDistance => 0.75f;

	public float PercentCool => 0f;

	public int NumFramesUntilPlayerDisappears => numFramesUntilPlayerDisappearsOnEnter;

	private Vent[] NearbyVents => new Vent[3] { Right, Left, Center };

	private void Start()
	{
		SetButtons(enabled: false);
		if ((Object)(object)additionalExitAnimation != (Object)null)
		{
			additionalExitAnimation.SetActive(false);
		}
		if ((Object)(object)myRend == (Object)null)
		{
			myRend = ((Component)this).GetComponent<SpriteRenderer>();
		}
		if ((Object)(object)myAnim == (Object)null)
		{
			myAnim = ((Component)this).GetComponent<SpriteAnim>();
		}
	}

	public void SetButtons(bool enabled)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		Vent[] nearbyVents = NearbyVents;
		Vector2 vector = ((!Object.op_Implicit((Object)(object)Right) || !Object.op_Implicit((Object)(object)Left)) ? Vector2.zero : Vector2.op_Implicit((((Component)Right).transform.position + ((Component)Left).transform.position) / 2f - ((Component)this).transform.position));
		for (int i = 0; i < Buttons.Length; i++)
		{
			ButtonBehavior buttonBehavior = Buttons[i];
			if (enabled)
			{
				Vent vent = nearbyVents[i];
				if (Object.op_Implicit((Object)(object)vent))
				{
					bool ventBeingCleaned = ShipStatus.Instance.Systems[SystemTypes.Ventilation] is VentilationSystem ventilationSystem && ventilationSystem.IsVentCurrentlyBeingCleaned(vent.Id);
					((Component)buttonBehavior).gameObject.SetActive(true);
					ToggleNeighborVentBeingCleaned(ventBeingCleaned, buttonBehavior, CleaningIndicators[i]);
					Vector3 val = ((Component)vent).transform.position - ((Component)this).transform.position;
					Vector3 val2 = val.normalized * (0.7f + spreadShift);
					val2.x *= Mathf.Sign(((Component)ShipStatus.Instance).transform.localScale.x);
					val2.y -= 0.08f;
					val2.z = -10f;
					((Component)buttonBehavior).transform.localPosition = val2;
					((Component)buttonBehavior).transform.LookAt2d(((Component)vent).transform);
					val2 = val2.RotateZ((vector.AngleSigned(Vector2.op_Implicit(val)) > 0f) ? spreadAmount : (0f - spreadAmount));
					((Component)buttonBehavior).transform.localPosition = val2;
					((Component)buttonBehavior).transform.Rotate(0f, 0f, (vector.AngleSigned(Vector2.op_Implicit(val)) > 0f) ? spreadAmount : (0f - spreadAmount));
				}
				else
				{
					((Component)buttonBehavior).gameObject.SetActive(false);
				}
			}
			else
			{
				((Component)buttonBehavior).gameObject.SetActive(false);
			}
		}
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		couldUse = pc.Role.CanVent && GameManager.Instance.LogicUsables.CanUse(this, playerControl) && pc.Role.CanUse(this) && (!playerControl.MustCleanVent(Id) || (playerControl.inVent && (Object)(object)currentVent == (Object)(object)this)) && !pc.IsDead && (playerControl.CanMove || playerControl.inVent);
		if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var value) && value is VentilationSystem ventilationSystem && ventilationSystem.IsVentCurrentlyBeingCleaned(Id))
		{
			couldUse = false;
		}
		canUse = couldUse;
		if (canUse)
		{
			Bounds bounds = playerControl.Collider.bounds;
			Vector3 center = bounds.center;
			Vector3 position = ((Component)this).transform.position;
			num = Vector2.Distance(Vector2.op_Implicit(center), Vector2.op_Implicit(position));
			canUse &= num <= UsableDistance && !PhysicsHelpers.AnythingBetween(playerControl.Collider, Vector2.op_Implicit(center), Vector2.op_Implicit(position), Constants.ShipOnlyMask, useTriggers: false);
		}
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Color val = (PlayerControl.LocalPlayer.Data.Role.IsImpostor ? Palette.ImpostorRed : Palette.CrewmateBlue);
		((Renderer)myRend).material.SetFloat("_Outline", (float)(on ? 1 : 0));
		((Renderer)myRend).material.SetColor("_OutlineColor", val);
		((Renderer)myRend).material.SetColor("_AddColor", mainTarget ? val : Color.clear);
	}

	public void ClickRight()
	{
		if (!TryMoveToVent(Right, out var error))
		{
			Logger.GlobalInstance.Warning("Local Player failed to move to Right vent because of " + error);
		}
	}

	public void ClickLeft()
	{
		if (!TryMoveToVent(Left, out var error))
		{
			Logger.GlobalInstance.Warning("Local Player failed to move to Left vent because of " + error);
		}
	}

	public void ClickCenter()
	{
		if (!TryMoveToVent(Center, out var error))
		{
			Logger.GlobalInstance.Warning("Local Player failed to move to Center vent because of " + error);
		}
	}

	public void Use()
	{
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			DestroyableSingleton<AchievementManager>.Instance.OnConsoleUse(this);
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if (localPlayer.inVent && !localPlayer.walkingToVent)
			{
				localPlayer.MyPhysics.RpcExitVent(Id);
				SetButtons(enabled: false);
			}
			else if (!localPlayer.walkingToVent)
			{
				localPlayer.MyPhysics.RpcEnterVent(Id);
				SetButtons(enabled: true);
			}
		}
	}

	public void UpdateArrows(VentilationSystem ventSystem)
	{
		if ((Object)(object)this != (Object)(object)currentVent || ventSystem == null)
		{
			return;
		}
		Vent[] nearbyVents = NearbyVents;
		for (int i = 0; i < nearbyVents.Length; i++)
		{
			Vent vent = nearbyVents[i];
			if (Object.op_Implicit((Object)(object)vent))
			{
				bool ventBeingCleaned = ventSystem.IsVentCurrentlyBeingCleaned(vent.Id);
				ButtonBehavior b = Buttons[i];
				GameObject c = CleaningIndicators[i];
				ToggleNeighborVentBeingCleaned(ventBeingCleaned, b, c);
			}
		}
	}

	internal void EnterVent(PlayerControl pc)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (pc.AmOwner)
		{
			currentVent = this;
			ConsoleJoystick.SetMode_Vent();
		}
		if (Object.op_Implicit((Object)(object)EnterVentAnim))
		{
			myAnim.Play(EnterVentAnim);
			if (pc.AmOwner && Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
				SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
				VibrationManager.Vibrate(0.4f, Vector2.op_Implicit(((Component)this).transform.position), 3.5f, 0.2f);
			}
		}
	}

	internal IEnumerator ExitVent(PlayerControl pc)
	{
		if (pc.AmOwner)
		{
			currentVent = null;
		}
		if (!Object.op_Implicit((Object)(object)ExitVentAnim) || myAnim.IsPlaying(ExitVentAnim))
		{
			yield break;
		}
		if ((Object)(object)additionalExitAnimation != (Object)null)
		{
			additionalExitAnimation.SetActive(false);
		}
		myAnim.Play(ExitVentAnim);
		while (myAnim.FrameTime < numFramesUntilPlayerReappearsOnExit)
		{
			yield return null;
		}
		if ((Object)(object)additionalExitAnimation != (Object)null)
		{
			additionalExitAnimation.SetActive(true);
		}
		if (pc.AmOwner && Constants.ShouldPlaySfx())
		{
			AudioClip clip = ShipStatus.Instance.VentEnterSound;
			if (Object.op_Implicit((Object)(object)ShipStatus.Instance.VentExitSound))
			{
				clip = ShipStatus.Instance.VentExitSound;
			}
			else
			{
				SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
			}
			SoundManager.Instance.PlaySound(clip, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
			VibrationManager.Vibrate(0.4f, Vector2.op_Implicit(((Component)this).transform.position), 3.5f, 0.2f);
		}
	}

	private bool TryMoveToVent(Vent otherVent, out string error)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)otherVent == (Object)null)
		{
			error = "Vent does not exist";
			return false;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (!localPlayer.inVent)
		{
			error = "Player is not currently inside a vent";
			return false;
		}
		if (localPlayer.walkingToVent || localPlayer.Visible)
		{
			error = "Player was still in the middle of animating into current vent; not allowed to move vents that fast";
			return false;
		}
		Vector3 position = ((Component)otherVent).transform.position;
		position -= Vector2.op_Implicit(localPlayer.Collider.offset);
		localPlayer.NetTransform.RpcSnapTo(Vector2.op_Implicit(position));
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(ShipStatus.Instance.VentMoveSounds.Random(), loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		SetButtons(enabled: false);
		otherVent.SetButtons(enabled: true);
		currentVent = otherVent;
		VentilationSystem.Update(VentilationSystem.Operation.Move, currentVent.Id);
		error = string.Empty;
		return true;
	}

	private void ToggleNeighborVentBeingCleaned(bool ventBeingCleaned, ButtonBehavior b, GameObject c)
	{
		((Behaviour)b).enabled = !ventBeingCleaned;
		c.SetActive(ventBeingCleaned);
	}

	private void OnDrawGizmosSelected()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Left != (Object)null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(((Component)this).transform.position, ((Component)Left).transform.position);
		}
		if ((Object)(object)Right != (Object)null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(((Component)this).transform.position, ((Component)Right).transform.position);
		}
		if ((Object)(object)Center != (Object)null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(((Component)this).transform.position, ((Component)Center).transform.position);
		}
	}

	public Vent()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
