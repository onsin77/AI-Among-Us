using System;
using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using Rewired;
using UnityEngine;

public class PlayerPhysics : InnerNetObject
{
	private byte lastClimbLadderSid;

	public AudioClip ImpostorDiscoveredSound;

	public PlayerAnimations Animations;

	[NonSerialized]
	public SpecialInputHandler inputHandler;

	public float Speed = 4.5f;

	public float GhostSpeed = 3f;

	private readonly Logger logger = new Logger("PlayerPhysics", Logger.Level.Debug, Logger.Category.Gameplay);

	private Rigidbody2D body;

	private PlayerControl myPlayer;

	private PlayerBodyTypes bodyType;

	private Coroutine petCoroutine;

	public bool DoingCustomAnimation { get; private set; }

	public float TrueSpeed => Speed * SpeedMod;

	public float SpeedMod
	{
		get
		{
			if ((Object)(object)GameManager.Instance == (Object)null)
			{
				return 1f;
			}
			float playerSpeedMod = GameManager.Instance.LogicOptions.GetPlayerSpeedMod(myPlayer);
			if ((Object)(object)myPlayer != (Object)null && myPlayer.Data != null && myPlayer.Data.IsDead)
			{
				return playerSpeedMod * GhostSpeed / Speed;
			}
			return playerSpeedMod;
		}
	}

	private bool FlipX
	{
		get
		{
			return myPlayer.cosmetics.FlipX;
		}
		set
		{
			myPlayer.cosmetics.SetFlipXWithoutPet(value);
		}
	}

	public Vector2 Velocity
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return body.velocity;
		}
	}

	public void RpcClimbLadder(Ladder source)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			ClimbLadder(source, (byte)(lastClimbLadderSid + 1));
		}
		else
		{
			lastClimbLadderSid++;
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 31, (SendOption)1);
		val.Write(source.Id);
		val.Write(lastClimbLadderSid);
		val.EndMessage();
	}

	public void RpcEnterVent(int id)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CoEnterVent(id));
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 19, (SendOption)1);
		val.WritePacked(id);
		val.EndMessage();
	}

	public void RpcExitVent(int id)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CoExitVent(id));
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 20, (SendOption)1);
		val.WritePacked(id);
		val.EndMessage();
	}

	public void RpcBootFromVent(int ventId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			BootFromVent(ventId);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 34, (SendOption)1);
		val.WritePacked(ventId);
		val.EndMessage();
	}

	public void RpcPet(Vector2 pos, Vector2 petPos)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (AmongUsClient.Instance.AmClient)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			petCoroutine = ((MonoBehaviour)this).StartCoroutine(CoPet(pos, petPos));
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 49, (SendOption)1);
		NetHelpers.WriteVector2(pos, val);
		NetHelpers.WriteVector2(petPos, val);
		val.EndMessage();
	}

	public void RpcCancelPet()
	{
		if (AmongUsClient.Instance.AmClient)
		{
			CancelPet();
		}
		AmongUsClient.Instance.StartRpc(NetId, 50, (SendOption)1).EndMessage();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		switch ((RpcCalls)callId)
		{
		case RpcCalls.ClimbLadder:
		{
			ShipStatus instance = ShipStatus.Instance;
			byte ladderId = reader.ReadByte();
			byte climbLadderSid = reader.ReadByte();
			ClimbLadder(instance.Ladders.First((Ladder f) => f.Id == ladderId), climbLadderSid);
			break;
		}
		case RpcCalls.EnterVent:
		{
			int id2 = reader.ReadPackedInt32();
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CoEnterVent(id2));
			break;
		}
		case RpcCalls.ExitVent:
		{
			int id = reader.ReadPackedInt32();
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CoExitVent(id));
			break;
		}
		case RpcCalls.BootFromVent:
		{
			int ventId = reader.ReadPackedInt32();
			BootFromVent(ventId);
			break;
		}
		case RpcCalls.Pet:
		{
			Vector2 pos = NetHelpers.ReadVector2(reader);
			Vector2 petPos = NetHelpers.ReadVector2(reader);
			PetPet(pos, petPos);
			break;
		}
		case RpcCalls.CancelPet:
			CancelPet();
			break;
		}
	}

	public void Awake()
	{
		body = ((Component)this).GetComponent<Rigidbody2D>();
		myPlayer = ((Component)this).GetComponent<PlayerControl>();
		SetBodyType(bodyType);
		FlipX = false;
		if ((Object)(object)inputHandler == (Object)null)
		{
			inputHandler = ((Component)this).gameObject.AddComponent<SpecialInputHandler>();
			inputHandler.disableVirtualCursor = true;
			((Behaviour)inputHandler).enabled = false;
		}
	}

	public void EnableInterpolation()
	{
		body.interpolation = (RigidbodyInterpolation2D)1;
	}

	public void SetBodyType(PlayerBodyTypes bodyType)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		this.bodyType = bodyType;
		myPlayer.cosmetics.EnsureInitialized(bodyType);
		Animations.SetBodyType(bodyType, myPlayer.cosmetics.FlippedCosmeticOffset);
		Animations.PlayIdleAnimation();
	}

	private void FixedUpdate()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		bool amDead = myPlayer.Data?.IsDead ?? false;
		HandleAnimation(amDead);
		if (AmOwner)
		{
			if (myPlayer.CanMove && Object.op_Implicit((Object)(object)GameData.Instance) && DestroyableSingleton<HudManager>.InstanceExists && DestroyableSingleton<HudManager>.Instance.joystick != null)
			{
				SetNormalizedVelocity(DestroyableSingleton<HudManager>.Instance.joystick.DeltaL);
			}
			CheckCancelPetting();
		}
	}

	public Vector2 GetVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return body.velocity;
	}

	public void SetNormalizedVelocity(Vector2 direction)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		body.velocity = direction * TrueSpeed;
	}

	private void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		position.z = position.y / 1000f;
		((Component)this).transform.position = position;
	}

	public Vector3 Vec2ToPosition(Vector2 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(pos.x, pos.y, pos.y / 1000f);
	}

	public void SetSkin(string skinId, int color)
	{
		myPlayer.cosmetics.SetSkin(skinId, color, () =>
		{
			if (Animations.IsPlayingSpawnAnimation())
			{
				myPlayer.cosmetics.AnimateSkinSpawn(Animations.Time);
			}
		});
	}

	public void StartClimb(bool down)
	{
		Animations.PlayClimbAnimation(down);
		myPlayer.cosmetics.AnimateClimb(down);
		myPlayer.cosmetics.TogglePetVisible(visible: false);
		if (bodyType.GetVisorOptions().HideDuringClimb)
		{
			myPlayer.cosmetics.ToggleVisor(active: false);
			FlipX = false;
		}
	}

	private void ClimbLadder(Ladder source, byte climbLadderSid)
	{
		if (NetHelpers.SidGreaterThan(climbLadderSid, lastClimbLadderSid))
		{
			lastClimbLadderSid = climbLadderSid;
			ResetMoveState();
			((MonoBehaviour)this).StartCoroutine(CoClimbLadder(source, climbLadderSid));
		}
	}

	private void PetPet(Vector2 pos, Vector2 petPos)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		petCoroutine = ((MonoBehaviour)this).StartCoroutine(CoPet(pos, petPos));
	}

	private IEnumerator CoClimbLadder(Ladder source, byte climbLadderSid)
	{
		float ladderTime = Time.time;
		float totalTime = Time.time;
		myPlayer.moveable = false;
		myPlayer.ForceKillTimerContinue = true;
		myPlayer.onLadder = true;
		((Behaviour)myPlayer.NetTransform).enabled = false;
		myPlayer.SetKinematic(b: true);
		myPlayer.NetTransform.ClearPositionQueues();
		if (myPlayer.AmOwner)
		{
			((Behaviour)myPlayer.MyPhysics.inputHandler).enabled = true;
		}
		GameServerUtils.DebugEstimatedWalkTo("walkToSeconds (estimated)", ((Component)this).transform.position, myPlayer.Collider.offset, ((Component)source).transform.position, Speed, 0.001f);
		yield return WalkPlayerTo(Vector2.op_Implicit(((Component)source).transform.position), 0.001f);
		GameServerUtils.DebugTime("walkToSeconds", ref ladderTime, Time.time);
		yield return Effects.Wait(0.1f);
		GameServerUtils.DebugTime("waitSeconds", ref ladderTime, Time.time);
		StartClimb(source.IsTop);
		if (Constants.ShouldPlaySfx() && (Object)(object)PlayerControl.LocalPlayer == (Object)(object)myPlayer)
		{
			myPlayer.FootSteps.clip = source.UseSound;
			myPlayer.FootSteps.loop = true;
			myPlayer.FootSteps.Play();
		}
		GameServerUtils.DebugEstimatedWalkTo("climbSeconds (estimated)", ((Component)this).transform.position, myPlayer.Collider.offset, ((Component)source.Destination).transform.position, Speed * (float)((!source.IsTop) ? 1 : 2), 0.001f);
		yield return WalkPlayerTo(Vector2.op_Implicit(((Component)source.Destination).transform.position), 0.001f, (!source.IsTop) ? 1 : 2);
		GameServerUtils.DebugTime("climbSeconds", ref ladderTime, Time.time);
		myPlayer.SetPetPosition(((Component)myPlayer).transform.position);
		ResetAnimState();
		yield return Effects.Wait(0.1f);
		GameServerUtils.DebugTime("waitSeconds", ref ladderTime, Time.time);
		GameServerUtils.DebugTime("totalLadderSeconds (compare this with server log output)", ref totalTime, Time.time);
		if (myPlayer.AmOwner)
		{
			source.SetDestinationCooldown();
		}
		myPlayer.moveable = true;
		((Behaviour)myPlayer.NetTransform).enabled = true;
		myPlayer.SetKinematic(b: false);
		myPlayer.ForceKillTimerContinue = false;
		myPlayer.onLadder = false;
	}

	public void ResetMoveState(bool stopCoroutines = true)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (stopCoroutines)
		{
			PlayerControl playerControl = myPlayer;
			if (playerControl != null)
			{
				((MonoBehaviour)playerControl).StopAllCoroutines();
			}
			((MonoBehaviour)this).StopAllCoroutines();
			if (Object.op_Implicit((Object)(object)inputHandler) && ((Behaviour)inputHandler).enabled)
			{
				((Behaviour)inputHandler).enabled = false;
			}
		}
		((Behaviour)this).enabled = true;
		if (Object.op_Implicit((Object)(object)myPlayer))
		{
			myPlayer.petting = false;
			myPlayer.inVent = false;
			myPlayer.isKilling = false;
			myPlayer.walkingToVent = false;
			myPlayer.Visible = true;
			myPlayer.shapeshifting = false;
			myPlayer.waitingForShapeshiftResponse = false;
			myPlayer.cosmetics.SetScale(Animations.DefaultPlayerScale, myPlayer.defaultCosmeticsScale);
			myPlayer.cosmetics.PettingHand.StopPetting();
			((Behaviour)myPlayer.NetTransform).enabled = true;
			myPlayer.NetTransform.SetPaused(isPaused: false);
			myPlayer.SetKinematic(b: false);
			if (myPlayer.Data == null)
			{
				logger.Error(((Object)myPlayer).name + " has no PlayerInfo");
			}
			((Behaviour)myPlayer.Collider).enabled = true;
		}
		else
		{
			logger.Error("Encountered a null myPlayer while Resetting Move State");
		}
		ResetAnimState();
	}

	public void ResetAnimState()
	{
		myPlayer.FootSteps.Stop();
		myPlayer.FootSteps.loop = false;
		myPlayer.cosmetics.SetHatAndVisorIdle(myPlayer.CurrentOutfit.ColorId);
		GameData.PlayerInfo data = myPlayer.Data;
		if (data == null || !data.IsDead)
		{
			myPlayer.cosmetics.AnimateSkinIdle();
			Animations.PlayIdleAnimation();
			myPlayer.Visible = true;
			myPlayer.SetHatAndVisorAlpha(1f);
			return;
		}
		myPlayer.cosmetics.SetGhost();
		if ((Object)(object)data.Role != (Object)null)
		{
			if (data.Role.Role == RoleTypes.GuardianAngel)
			{
				Animations.PlayGuardianAngelIdleAnimation();
			}
			else
			{
				Animations.PlayGhostIdleAnimation();
			}
		}
		myPlayer.SetHatAndVisorAlpha(0.5f);
	}

	private void HandleAnimation(bool amDead)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		if (Animations.IsPlayingSpawnAnimation() || DoingCustomAnimation || !Object.op_Implicit((Object)(object)GameData.Instance))
		{
			return;
		}
		Vector2 velocity = body.velocity;
		if (Animations.IsPlayingClimbAnimation())
		{
			return;
		}
		if (!amDead)
		{
			if (velocity.sqrMagnitude >= 0.05f)
			{
				bool flipX = FlipX;
				if (velocity.x < -0.01f)
				{
					FlipX = true;
				}
				else if (velocity.x > 0.01f)
				{
					FlipX = false;
				}
				if (!Animations.IsPlayingRunAnimation() || flipX != FlipX || !myPlayer.cosmetics.IsSkinPlayingRunAnim())
				{
					Animations.PlayRunAnimation();
					myPlayer.cosmetics.AnimateSkinRun();
				}
			}
			else if (Animations.IsPlayingRunAnimation() || Animations.IsPlayingSpawnAnimation() || !Animations.IsPlayingSomeAnimation())
			{
				myPlayer.cosmetics.AnimateSkinIdle();
				Animations.PlayIdleAnimation();
				myPlayer.SetHatAndVisorAlpha(1f);
			}
			return;
		}
		myPlayer.cosmetics.SetGhost();
		if (myPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
		{
			if (!Animations.IsPlayingGuardianAngelIdleAnimation())
			{
				Animations.PlayGuardianAngelIdleAnimation();
				myPlayer.SetHatAndVisorAlpha(0.5f);
			}
		}
		else if (!Animations.IsPlayingGhostIdleAnimation())
		{
			Animations.PlayGhostIdleAnimation();
			myPlayer.SetHatAndVisorAlpha(0.5f);
		}
		if (velocity.x < -0.01f)
		{
			FlipX = true;
		}
		else if (velocity.x > 0.01f)
		{
			FlipX = false;
		}
	}

	public void AnimateCustom(AnimationClip anim)
	{
		if (!DoingCustomAnimation)
		{
			DoingCustomAnimation = true;
			((MonoBehaviour)this).StartCoroutine(CoAnimateCustom(anim));
		}
	}

	private IEnumerator CoAnimateCustom(AnimationClip anim)
	{
		myPlayer.moveable = false;
		if (myPlayer.AmOwner)
		{
			((Behaviour)inputHandler).enabled = true;
		}
		yield return Animations.CoPlayCustomAnimation(anim);
		myPlayer.cosmetics.AnimateSkinIdle();
		Animations.PlayIdleAnimation();
		myPlayer.moveable = true;
		if (myPlayer.AmOwner)
		{
			((Behaviour)inputHandler).enabled = false;
		}
		DoingCustomAnimation = false;
		myPlayer.cosmetics.SetBodyCosmeticsVisible(b: true);
	}

	public IEnumerator CoSpawnPlayer(LobbyBehaviour lobby)
	{
		if (Object.op_Implicit((Object)(object)lobby))
		{
			if (myPlayer.AmOwner)
			{
				((Behaviour)inputHandler).enabled = true;
			}
			int spawnSeatId = myPlayer.PlayerId % lobby.SpawnPositions.Length;
			Vector3 spawnPos = Vec2ToPosition(lobby.SpawnPositions[spawnSeatId]);
			myPlayer.cosmetics.ToggleName(active: false);
			((Behaviour)myPlayer.Collider).enabled = false;
			((Behaviour)myPlayer.NetTransform).enabled = false;
			KillAnimation.SetMovement(myPlayer, canMove: false);
			yield return (object)new WaitForFixedUpdate();
			myPlayer.cosmetics.SetForcedVisible(isVisible: true);
			bool amFlipped = spawnSeatId > 4;
			SoundManager.Instance.PlaySound(lobby.SpawnSound, loop: false).volume = 0.75f;
			FlipX = amFlipped;
			myPlayer.cosmetics.AnimateSkinSpawn();
			((Component)myPlayer).transform.position = spawnPos;
			yield return Animations.CoPlaySpawnAnimation(FlipX);
			((Component)this).transform.position = spawnPos + new Vector3(amFlipped ? (-0.3f) : 0.3f, -0.24f);
			ResetMoveState(stopCoroutines: false);
			Vector3 val = -spawnPos;
			Vector2 val2 = Vector2.op_Implicit(val.normalized);
			yield return WalkPlayerTo(Vector2.op_Implicit(spawnPos) + val2);
			((Behaviour)myPlayer.Collider).enabled = true;
			KillAnimation.SetMovement(myPlayer, canMove: true);
			((Behaviour)myPlayer.NetTransform).enabled = true;
			myPlayer.NetTransform.ClearPositionQueues();
			myPlayer.cosmetics.ToggleName(active: true);
			if (myPlayer.AmOwner)
			{
				((Behaviour)inputHandler).enabled = false;
			}
		}
	}

	public void ExitAllVents()
	{
		ConsoleJoystick.SetMode_Gameplay();
		Vent.currentVent = null;
		ResetMoveState();
		myPlayer.moveable = true;
		Vent[] allVents = ShipStatus.Instance.AllVents;
		for (int i = 0; i < allVents.Length; i++)
		{
			allVents[i].SetButtons(enabled: false);
		}
	}

	private IEnumerator CoPet(Vector2 pos, Vector2 petPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (myPlayer.AmOwner)
		{
			((Behaviour)inputHandler).enabled = true;
		}
		myPlayer.petting = true;
		myPlayer.moveable = false;
		myPlayer.cosmetics.CurrentPet.SetGettingPet(petting: true, petPos);
		yield return WalkPlayerTo(pos);
		body.velocity = Vector2.zero;
		myPlayer.moveable = false;
		FlipX = false;
		myPlayer.cosmetics.AnimateSkinIdle();
		myPlayer.cosmetics.PettingHand.StartPet(myPlayer.cosmetics.CurrentPet);
		yield return myPlayer.cosmetics.PettingHand.WaitUntilFinishedPetting();
		if (myPlayer.AmOwner)
		{
			StatsManager.Instance.IncrementStat(StringNames.StatsTimesPettedPet);
		}
		CancelPet();
	}

	private void CheckCancelPetting()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)myPlayer.cosmetics.PettingHand).gameObject.activeSelf && myPlayer.petting)
		{
			Vector2 deltaL = DestroyableSingleton<HudManager>.Instance.joystick.DeltaL;
			if (DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick)
			{
				Player player = ReInput.players.GetPlayer(0);
				deltaL.x = player.GetAxis(9);
				deltaL.y = player.GetAxis(10);
			}
			if (deltaL.sqrMagnitude > 0f)
			{
				RpcCancelPet();
			}
		}
	}

	private void CancelPet()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (petCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(petCoroutine);
		}
		myPlayer.cosmetics.PettingHand.StopPetting();
		myPlayer.moveable = true;
		Animations.PlayIdleAnimation();
		myPlayer.cosmetics.CurrentPet.SetGettingPet(petting: false, Vector2.op_Implicit(((Component)myPlayer.cosmetics.CurrentPet).transform.position));
		myPlayer.petting = false;
		if (myPlayer.AmOwner)
		{
			((Behaviour)inputHandler).enabled = false;
		}
	}

	private IEnumerator CoEnterVent(int id)
	{
		if (!Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			Vent vent = ShipStatus.Instance.AllVents.First((Vent v) => v.Id == id);
			myPlayer.NetTransform.SetPaused(isPaused: true);
			if (myPlayer.AmOwner)
			{
				((Behaviour)inputHandler).enabled = true;
			}
			myPlayer.walkingToVent = true;
			myPlayer.moveable = false;
			yield return WalkPlayerTo(Vector2.op_Implicit(((Component)vent).transform.position + vent.Offset));
			myPlayer.inVent = true;
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.VentUsed(myPlayer.Data);
			vent.EnterVent(myPlayer);
			myPlayer.cosmetics.AnimateSkinEnterVent();
			yield return Animations.CoPlayEnterVentAnimation(vent.NumFramesUntilPlayerDisappears);
			myPlayer.cosmetics.AnimateSkinIdle();
			Animations.PlayIdleAnimation();
			myPlayer.Visible = false;
			myPlayer.walkingToVent = false;
			myPlayer.currentRoleAnimations.ForEach((RoleEffectAnimation an) =>
			{
				an.ToggleRenderer(on: false);
			});
			if (myPlayer.AmOwner)
			{
				VentilationSystem.Update(VentilationSystem.Operation.Enter, id);
				((Behaviour)inputHandler).enabled = false;
			}
			logger.Debug($"Player {myPlayer.PlayerId} entered vent {id}");
		}
	}

	private IEnumerator CoExitVent(int id)
	{
		Vent vent = ShipStatus.Instance.AllVents.First((Vent v) => v.Id == id);
		Vector3 position = ((Component)vent).transform.position;
		position -= Vector2.op_Implicit(myPlayer.Collider.offset);
		myPlayer.NetTransform.SnapTo(Vector2.op_Implicit(position));
		if (myPlayer.AmOwner)
		{
			((Behaviour)inputHandler).enabled = true;
			VentilationSystem.Update(VentilationSystem.Operation.Exit, id);
		}
		yield return vent.ExitVent(myPlayer);
		myPlayer.Visible = true;
		myPlayer.inVent = false;
		myPlayer.cosmetics.AnimateSkinExitVent();
		yield return Animations.CoPlayExitVentAnimation();
		myPlayer.cosmetics.AnimateSkinIdle();
		Animations.PlayIdleAnimation();
		myPlayer.moveable = true;
		myPlayer.currentRoleAnimations.ForEach((RoleEffectAnimation an) =>
		{
			an.ToggleRenderer(on: true);
		});
		if (myPlayer.AmOwner)
		{
			((Behaviour)inputHandler).enabled = false;
			if (myPlayer.Data.Role.Role == RoleTypes.Engineer)
			{
				myPlayer.Data.Role.SetCooldown();
			}
		}
		myPlayer.NetTransform.SetPaused(isPaused: false);
		logger.Debug($"Player {myPlayer.PlayerId} exited vent {id}");
	}

	public IEnumerator WalkPlayerTo(Vector2 worldPos, float tolerance = 0.01f, float speedMul = 1f, bool ignoreColliderOffset = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!ignoreColliderOffset)
		{
			worldPos -= myPlayer.Collider.offset;
		}
		Rigidbody2D body = this.body;
		Vector2 del = worldPos - Vector2.op_Implicit(((Component)this).transform.position);
		while (del.sqrMagnitude > tolerance)
		{
			float num = Mathf.Clamp(del.magnitude * 2f, 0.05f, 1f);
			body.velocity = del.normalized * Speed * num * speedMul;
			yield return null;
			Vector2 velocity = body.velocity;
			if (velocity.magnitude < 0.005f && (double)del.sqrMagnitude < 0.1)
			{
				break;
			}
			del = worldPos - Vector2.op_Implicit(((Component)this).transform.position);
		}
		del = default;
		body.velocity = Vector2.zero;
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}

	private void BootFromVent(int ventId)
	{
		if (AmOwner)
		{
			ShipStatus.Instance.AllVents.ForEach((Vent v) =>
			{
				v.SetButtons(enabled: false);
			});
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ImpostorDiscoveredSound, loop: false, 0.8f);
			}
		}
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(CoExitVent(ventId));
	}
}
