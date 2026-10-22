using System.Collections;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

public class MovingPlatformBehaviour : MonoBehaviour, ISystemType
{
	public Vector3 LeftPosition;

	public Vector3 RightPosition;

	public Vector3 DisabledPosition;

	public Vector3 LeftUsePosition;

	public Vector3 RightUsePosition;

	public AudioClip MovingSound;

	private bool IsLeft;

	private PlayerControl Target;

	private byte useId;

	public bool InUse => Object.op_Implicit((Object)(object)Target);

	public bool IsDirty { get; private set; }

	public void Use()
	{
		PlayerControl.LocalPlayer.RpcUsePlatform();
	}

	public void Use(PlayerControl player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - ((Component)player).transform.position;
		if (!player.Data.IsDead && !player.Data.Disconnected && !Object.op_Implicit((Object)(object)Target) && !(val.magnitude > 3f))
		{
			IsDirty = true;
			((MonoBehaviour)this).StartCoroutine(UsePlatform(player));
		}
	}

	public void Start()
	{
		SetSide(isLeft: true);
	}

	private void SetSide(bool isLeft)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		IsLeft = isLeft;
		((Component)this).transform.localPosition = (IsLeft ? LeftPosition : RightPosition);
		if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek && DisabledPosition != Vector3.zero)
		{
			((Component)this).transform.localPosition = DisabledPosition;
		}
		IsDirty = true;
	}

	private void SetTarget(uint playerNetId, bool isLeft)
	{
		if (Object.op_Implicit((Object)(object)Target))
		{
			MeetingCalled();
		}
		PlayerControl playerControl = AmongUsClient.Instance.FindObjectByNetId<PlayerControl>(playerNetId);
		if (!Object.op_Implicit((Object)(object)playerControl))
		{
			SetSide(isLeft);
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(UsePlatform(playerControl));
		}
	}

	private IEnumerator UsePlatform(PlayerControl target)
	{
		float platformTime = Time.time;
		float totalTime = Time.time;
		Target = target;
		target.MyPhysics.ResetMoveState();
		if (target.AmOwner)
		{
			PlayerControl.HideCursorTemporarily();
		}
		target.moveable = false;
		target.NetTransform.SetPaused(isPaused: true);
		target.NetTransform.ClearPositionQueues();
		target.SetKinematic(b: true);
		target.inMovingPlat = true;
		target.ForceKillTimerContinue = true;
		Vector3 val = (IsLeft ? LeftUsePosition : RightUsePosition);
		Vector3 val2 = ((!IsLeft) ? LeftUsePosition : RightUsePosition);
		Vector3 sourcePos = (IsLeft ? LeftPosition : RightPosition);
		Vector3 targetPos = ((!IsLeft) ? LeftPosition : RightPosition);
		Vector3 val3 = ((Component)this).transform.parent.TransformPoint(val);
		Vector3 worldUseTargetPos = ((Component)this).transform.parent.TransformPoint(val2);
		Vector3 worldSourcePos = ((Component)this).transform.parent.TransformPoint(sourcePos);
		Vector3 worldTargetPos = ((Component)this).transform.parent.TransformPoint(targetPos);
		yield return target.MyPhysics.WalkPlayerTo(Vector2.op_Implicit(val3));
		GameServerUtils.DebugTime("walkToUseSeconds", ref platformTime, Time.time);
		yield return target.MyPhysics.WalkPlayerTo(Vector2.op_Implicit(worldSourcePos));
		GameServerUtils.DebugTime("walkToSourceSeconds", ref platformTime, Time.time);
		yield return Effects.Wait(0.1f);
		GameServerUtils.DebugTime("waitSeconds", ref platformTime, Time.time);
		worldSourcePos -= Vector2.op_Implicit(target.Collider.offset);
		worldTargetPos -= Vector2.op_Implicit(target.Collider.offset);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("PlatformMoving", MovingSound, loop: true, SoundDynamics, SoundManager.Instance.SfxChannel);
		}
		IsLeft = !IsLeft;
		yield return Effects.All(Effects.Slide2D(((Component)this).transform, Vector2.op_Implicit(sourcePos), Vector2.op_Implicit(targetPos), target.MyPhysics.Speed), Effects.Slide2DWorld(((Component)target).transform, Vector2.op_Implicit(worldSourcePos), Vector2.op_Implicit(worldTargetPos), target.MyPhysics.Speed));
		GameServerUtils.DebugTime("platformTravelSeconds", ref platformTime, Time.time);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.StopNamedSound("PlatformMoving");
		}
		if ((Object)(object)target == (Object)null)
		{
			ResetPlatform();
			yield break;
		}
		yield return target.MyPhysics.WalkPlayerTo(Vector2.op_Implicit(worldUseTargetPos));
		GameServerUtils.DebugTime("walkFromSeconds", ref platformTime, Time.time);
		GameServerUtils.DebugTime("totalPlatformSeconds (compare this with server log output)", ref totalTime, Time.time);
		target.SetPetPosition(((Component)target).transform.position);
		target.inMovingPlat = false;
		target.moveable = true;
		target.ForceKillTimerContinue = false;
		target.NetTransform.SetPaused(isPaused: false);
		target.SetKinematic(b: false);
		target.NetTransform.Halt();
		yield return Effects.Wait(0.1f);
		Target = null;
	}

	private void ResetPlatform()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.StopNamedSound("PlatformMoving");
		}
		Target = null;
	}

	private void SoundDynamics(AudioSource source, float dt)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			source.volume = 0f;
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(val, truePosition);
		if (num > 6f)
		{
			source.volume = 0f;
			return;
		}
		float num2 = 1f - num / 6f;
		source.volume = Mathf.Lerp(source.volume, num2, dt);
		VibrationManager.Vibrate(0.15f, val, 6f);
	}

	public void MeetingCalled()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		VibrationManager.ClearAllVibration();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.StopNamedSound("PlatformMoving");
		}
		if (Object.op_Implicit((Object)(object)Target))
		{
			((Behaviour)Target.MyPhysics).enabled = true;
			((Behaviour)Target.Collider).enabled = true;
			Target.moveable = true;
			((Behaviour)Target.NetTransform).enabled = true;
			Target.SetKinematic(b: false);
			Target.inMovingPlat = false;
			Target = null;
		}
		SetSide(IsLeft);
	}

	public void Deteriorate(float deltaTime)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		useId++;
		writer.Write(useId);
		writer.Write((uint)(((int?)Target?.NetId) ?? (-1)));
		writer.Write(IsLeft);
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			useId = reader.ReadByte();
			SetTarget(reader.ReadUInt32(), reader.ReadBoolean());
			return;
		}
		byte newSid = reader.ReadByte();
		if (NetHelpers.SidGreaterThan(newSid, useId))
		{
			useId = newSid;
			SetTarget(reader.ReadUInt32(), reader.ReadBoolean());
		}
	}
}
