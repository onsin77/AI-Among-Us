using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZiplineBehaviour : MonoBehaviour
{
	public const float MAX_USE_DISTANCE = 3f;

	private const string SFX_NAME = "ZiplineTravel_";

	private const float PET_HIDE_SPEED = 2f;

	private const float TOGGLE_PET_INVISIBLE_SECONDS = 0.5f;

	private const float TOGGLE_PET_VISIBLE_SECONDS = 0f;

	[Header("Positions")]
	[SerializeField]
	private Transform handleTop;

	[SerializeField]
	private Transform handleBottom;

	[SerializeField]
	private Transform landingPositionBottom;

	[SerializeField]
	private Transform landingPositionTop;

	[SerializeField]
	private Transform dropPositionBottom;

	[SerializeField]
	private Transform dropPositionTop;

	[Header("Travel time")]
	[SerializeField]
	private float upTravelTime = 2f;

	[SerializeField]
	private float downTravelTime = 2f;

	[Header("Animation Settings")]
	[SerializeField]
	private float timeJump = 0.2f;

	[SerializeField]
	private AnimationCurve jumpZiplineCurve;

	[SerializeField]
	private AnimationCurve jumpZiplineCurveBottom;

	[SerializeField]
	private AnimationCurve jumpZiplineHandCurve;

	[SerializeField]
	[Header("Hand Settings")]
	private ObjectPoolBehavior handPool;

	[SerializeField]
	private Transform upHandPosition;

	[SerializeField]
	private Transform downHandPosition;

	[Header("Audio")]
	[SerializeField]
	private AudioClip attachSound;

	[SerializeField]
	private AudioClip detachSound;

	[SerializeField]
	private AudioClip upSound;

	[SerializeField]
	private AudioClip downSound;

	[SerializeField]
	private AudioClip downLoopSound;

	private readonly Logger logger = new Logger("ZiplineBehaviour", Logger.Level.Info, Logger.Category.Gameplay);

	private Dictionary<byte, Coroutine> playerIdWalkCoroutines;

	private Dictionary<byte, Coroutine> playerIdUseZiplineCoroutines;

	private Dictionary<byte, HandZiplinePoolable> playerIdHands;

	private Dictionary<byte, IEnumerator> soundLoopCoroutines;

	private ZiplineConsole lastUsedConsole;

	private void Awake()
	{
		playerIdWalkCoroutines = new Dictionary<byte, Coroutine>();
		playerIdUseZiplineCoroutines = new Dictionary<byte, Coroutine>();
		playerIdHands = new Dictionary<byte, HandZiplinePoolable>();
		soundLoopCoroutines = new Dictionary<byte, IEnumerator>();
	}

	public void Use(bool fromTop, ZiplineConsole console)
	{
		lastUsedConsole = console;
		PreUseZipline(PlayerControl.LocalPlayer, fromTop);
	}

	public void OnMeetingCalled()
	{
		StopAllZiplineSounds();
		((MonoBehaviour)this).StopAllCoroutines();
		ResetZiplineMeeting();
		playerIdWalkCoroutines.Clear();
		playerIdUseZiplineCoroutines.Clear();
		playerIdHands.Clear();
	}

	public void Use(PlayerControl player, bool fromTop)
	{
		if (!player.Data.IsDead && !player.Data.Disconnected)
		{
			Transform start;
			Transform end;
			Transform landing;
			if (fromTop)
			{
				start = handleTop;
				end = handleBottom;
				landing = landingPositionBottom;
			}
			else
			{
				start = handleBottom;
				end = handleTop;
				landing = landingPositionTop;
			}
			StopAllCoroutinesForPlayer(player);
			playerIdUseZiplineCoroutines[player.PlayerId] = ((MonoBehaviour)this).StartCoroutine(CoUseZipline(player, start, end, landing, fromTop));
		}
	}

	public Vector3 GetHandlePos(bool fromTop)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!fromTop)
		{
			return handleBottom.position;
		}
		return handleTop.position;
	}

	public void CancelZiplineUseForPlayer(PlayerControl player)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		StopAllCoroutinesForPlayer(player);
		if (playerIdHands.TryGetValue(player.PlayerId, out var value))
		{
			handPool.Reclaim(value);
			playerIdHands.Remove(player.PlayerId);
		}
		ResetTarget(player);
		player.SetPetPosition(((Component)player).transform.position);
		player.cosmetics.TogglePetVisible(visible: true);
	}

	private void StopAllCoroutinesForPlayer(PlayerControl player)
	{
		if (playerIdWalkCoroutines.TryGetValue(player.PlayerId, out var value))
		{
			if (value != null)
			{
				((MonoBehaviour)this).StopCoroutine(value);
			}
			playerIdWalkCoroutines.Remove(player.PlayerId);
		}
		if (playerIdUseZiplineCoroutines.TryGetValue(player.PlayerId, out var value2))
		{
			if (value2 != null)
			{
				((MonoBehaviour)this).StopCoroutine(value2);
			}
			playerIdUseZiplineCoroutines.Remove(player.PlayerId);
		}
	}

	private void PreUseZipline(PlayerControl player, bool fromTop)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		float lastTime = Time.time;
		if (!playerIdWalkCoroutines.ContainsKey(player.PlayerId))
		{
			Transform val = ((!fromTop) ? handleBottom : handleTop);
			PreparePlayerForZipline(player, gotPermission: false);
			Coroutine value = ((MonoBehaviour)this).StartCoroutine(CoWalkPlayerToZiplineWhileWaiting(player, val.position));
			playerIdWalkCoroutines.Add(player.PlayerId, value);
			GameServerUtils.DebugTime("walkToSeconds", ref lastTime, Time.time);
			PlayerControl.LocalPlayer.CmdCheckUseZipline(player, this, fromTop);
		}
		else
		{
			logger.Warning($"{player} was already walking towards the zipline.");
		}
	}

	private IEnumerator CoUseZipline(PlayerControl player, Transform start, Transform end, Transform landing, bool fromTop)
	{
		float ziplineTime = Time.time;
		float totalTime = Time.time;
		logger.Info($"[{Time.time}] CoUseZipline: Animating player {player.PlayerId} going across zipline");
		int superAbort = 100;
		while (player.isKilling)
		{
			superAbort--;
			if (superAbort < 0)
			{
				yield break;
			}
			yield return null;
		}
		PreparePlayerForZipline(player, gotPermission: true);
		yield return CoWalkPlayerToZipline(player, start.position);
		GameServerUtils.DebugTime("walkToSeconds", ref ziplineTime, Time.time);
		HandZiplinePoolable currentHand = GetHand();
		playerIdHands[player.PlayerId] = currentHand;
		currentHand.SetPlayerColor(player.CurrentOutfit, PlayerMaterial.MaskType.None);
		PlaySound(attachSound, Vector2.op_Implicit(start.position), PlayerControl.LocalPlayer.GetTruePosition());
		yield return CoAnimatePlayerJumpingOnToZipline(player, fromTop, currentHand);
		GameServerUtils.DebugTime("jumpToSeconds", ref ziplineTime, Time.time);
		StartSound(player, fromTop);
		yield return CoAnimateZiplineAndPlayer(player, end.position, fromTop, currentHand);
		GameServerUtils.DebugTime("travelSeconds", ref ziplineTime, Time.time);
		StopZiplineSound(player);
		PlaySound(detachSound, Vector2.op_Implicit(end.position), PlayerControl.LocalPlayer.GetTruePosition());
		yield return CoAlightPlayerFromZipline(player, landing.position, fromTop, currentHand);
		GameServerUtils.DebugTime("jumpFromSeconds + walkFrom + wait", ref ziplineTime, Time.time);
		GameServerUtils.DebugTime("totalZiplineSeconds (compare this with server log output)", ref totalTime, Time.time);
		if (Object.op_Implicit((Object)(object)lastUsedConsole) && player.AmOwner)
		{
			lastUsedConsole.SetDestinationCooldown();
		}
		player.NetTransform.Halt();
		ResetTarget(player);
		ResetZipline();
	}

	private IEnumerator CoAnimatePlayerJumpingOnToZipline(PlayerControl player, bool fromTop, HandZiplinePoolable hand)
	{
		((Component)hand).gameObject.SetActive(true);
		AnimationCurve curve;
		if (fromTop)
		{
			hand.StartDownAnimation();
			((Component)hand).transform.position = upHandPosition.position;
			curve = jumpZiplineCurve;
		}
		else
		{
			hand.StartUpAnimation();
			((Component)hand).transform.position = downHandPosition.position;
			curve = jumpZiplineCurveBottom;
		}
		player.cosmetics.AnimateSkinJump();
		float ziplineTime = Time.time;
		yield return Effects.All(Effects.CurvePositionY(((Component)player).transform, curve, timeJump), Effects.CurvePositionY(((Component)hand).transform, jumpZiplineHandCurve, timeJump), player.MyPhysics.Animations.CoPlayJumpAnimation());
		GameServerUtils.DebugTime("effectsAllSeconds", ref ziplineTime, Time.time);
		yield return Effects.Wait(0.1f);
		((Behaviour)player.MyPhysics).enabled = false;
	}

	private void PreparePlayerForZipline(PlayerControl player, bool gotPermission)
	{
		player.MyPhysics.ResetMoveState();
		if (player.AmOwner)
		{
			PlayerControl.HideCursorTemporarily();
		}
		player.moveable = false;
		player.ForceKillTimerContinue = true;
		((Behaviour)player.NetTransform).enabled = false;
		player.SetKinematic(b: true);
		player.NetTransform.ClearPositionQueues();
		if (gotPermission)
		{
			player.inMovingPlat = true;
		}
	}

	private IEnumerator CoTogglePetVisibility(PlayerControl player, float delaySeconds, bool shouldMakeVisible)
	{
		yield return (object)new WaitForSeconds(delaySeconds);
		player.cosmetics.TogglePetVisible(shouldMakeVisible);
	}

	private IEnumerator CoWalkPlayerToZipline(PlayerControl player, Vector3 handleStartPosition)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (player.cosmetics.HasPetEquipped())
		{
			((MonoBehaviour)this).StartCoroutine(CoTogglePetVisibility(player, 0.5f, shouldMakeVisible: false));
		}
		yield return player.MyPhysics.WalkPlayerTo(Vector2.op_Implicit(handleStartPosition), 0.001f, 1f, ignoreColliderOffset: true);
	}

	private IEnumerator CoWalkPlayerToZiplineWhileWaiting(PlayerControl player, Vector3 handleStartPosition)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (player.cosmetics.HasPetEquipped())
		{
			((MonoBehaviour)this).StartCoroutine(CoTogglePetVisibility(player, 0.5f, shouldMakeVisible: false));
		}
		yield return player.MyPhysics.WalkPlayerTo(Vector2.op_Implicit(handleStartPosition), 0.001f, 1f, ignoreColliderOffset: true);
		((MonoBehaviour)this).StartCoroutine(CoTogglePetVisibility(player, 0f, shouldMakeVisible: true));
		ResetTarget(player);
		player.SetPetPosition(((Component)player).transform.position);
		player.cosmetics.TogglePetVisible(visible: true);
		if (playerIdWalkCoroutines.ContainsKey(player.PlayerId))
		{
			playerIdWalkCoroutines.Remove(player.PlayerId);
		}
	}

	private IEnumerator CoAnimateZiplineAndPlayer(PlayerControl player, Vector3 handleEndPosition, bool fromTop, HandZiplinePoolable hand)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float travelSeconds;
		if (fromTop)
		{
			travelSeconds = downTravelTime;
			handleEndPosition = dropPositionBottom.position;
		}
		else
		{
			travelSeconds = upTravelTime;
			handleEndPosition = dropPositionTop.position;
		}
		Vector3 zero = Vector3.zero;
		Vector3 startPos = ((Component)player).transform.position;
		Vector3 handOffset = ((Component)hand).transform.position - startPos;
		for (float time = 0f; time < travelSeconds; time += Time.deltaTime)
		{
			float num = time / travelSeconds;
			if ((Object)(object)player != (Object)null)
			{
				zero.x = Mathf.SmoothStep(startPos.x, handleEndPosition.x, num);
				zero.y = Mathf.SmoothStep(startPos.y, handleEndPosition.y, num);
				zero.z = ((Component)player).transform.localPosition.z;
				((Component)player).transform.position = zero;
				zero += handOffset;
				zero.z = ((Component)hand).transform.position.z;
				((Component)hand).transform.position = zero;
				yield return null;
				continue;
			}
			Debug.LogWarning((object)"Player transform was destroyed while moving");
			break;
		}
	}

	private IEnumerator CoAlightPlayerFromZipline(PlayerControl player, Vector3 landingPosition, bool fromTop, HandZiplinePoolable hand)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)player.MyPhysics).enabled = true;
		float ziplineTime = Time.time;
		Vector3 val = ((Component)this).transform.TransformPoint(landingPosition);
		if (fromTop)
		{
			hand.StartDownOutroAnimation();
		}
		else
		{
			hand.StartUpOutroAnimation();
		}
		GameServerUtils.DebugTime("jumpFromSeconds", ref ziplineTime, Time.time);
		yield return player.MyPhysics.WalkPlayerTo(Vector2.op_Implicit(val));
		GameServerUtils.DebugTime("walkFromSeconds", ref ziplineTime, Time.time);
		player.SetPetPosition(((Component)player).transform.position);
		((MonoBehaviour)this).StartCoroutine(CoTogglePetVisibility(player, 0f, shouldMakeVisible: true));
		yield return Effects.Wait(0.1f);
	}

	private void StartSound(PlayerControl player, bool fromTop)
	{
		StopZiplineSound(player);
		if (fromTop)
		{
			soundLoopCoroutines[player.PlayerId] = CoDownSound(player);
			((MonoBehaviour)this).StartCoroutine(soundLoopCoroutines[player.PlayerId]);
		}
		else if (ShouldPlaySound())
		{
			SoundManager.Instance.PlayDynamicSound("ZiplineTravel_" + player.PlayerId, upSound, loop: true, SoundDynamics, SoundManager.Instance.SfxChannel);
		}
	}

	private void StopZiplineSound(PlayerControl player)
	{
		if (soundLoopCoroutines.ContainsKey(player.PlayerId) && soundLoopCoroutines[player.PlayerId] != null)
		{
			((MonoBehaviour)this).StopCoroutine(soundLoopCoroutines[player.PlayerId]);
			soundLoopCoroutines[player.PlayerId] = null;
		}
		string name = "ZiplineTravel_" + player.PlayerId;
		if (SoundManager.Instance.HasNamedSound(name))
		{
			SoundManager.Instance.StopNamedSound(name);
		}
	}

	private void StopAllZiplineSounds()
	{
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			StopZiplineSound(allPlayerControl);
		}
	}

	private IEnumerator CoDownSound(PlayerControl player)
	{
		if (ShouldPlaySound())
		{
			SoundManager.Instance.PlayDynamicSound("ZiplineTravel_" + player.PlayerId, downSound, loop: false, SoundDynamics, SoundManager.Instance.SfxChannel);
			yield return (object)new WaitForSeconds(downSound.length - 0.05f);
			SoundManager.Instance.PlayDynamicSound("ZiplineTravel_" + player.PlayerId, downLoopSound, loop: true, SoundDynamics, SoundManager.Instance.SfxChannel);
		}
	}

	private void SoundDynamics(AudioSource source, float dt)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			source.volume = 0f;
			return;
		}
		source.volume = 1f;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		Vector2 closestPointOnLine = GetClosestPointOnLine(Vector2.op_Implicit(handleBottom.position), Vector2.op_Implicit(handleTop.position), truePosition);
		float num = Vector2.Distance(truePosition, closestPointOnLine);
		source.volume = SoundManager.GetSoundVolume(closestPointOnLine, truePosition);
		if (num < 0.25f)
		{
			VibrationManager.Vibrate(0.15f, 0.15f);
		}
	}

	private bool ShouldPlaySound()
	{
		return Constants.ShouldPlaySfx();
	}

	private void PlaySound(AudioClip sound, Vector2 soundPosition, Vector2 playerPosition)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (ShouldPlaySound())
		{
			float soundVolume = SoundManager.GetSoundVolume(soundPosition, playerPosition);
			if (!(soundVolume <= 0f))
			{
				SoundManager.Instance.PlaySoundImmediate(sound, loop: false, soundVolume);
			}
		}
	}

	private Vector2 GetClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = lineEnd - lineStart;
		float magnitude = val.magnitude;
		val.Normalize();
		float num = Vector2.Dot(position - lineStart, val);
		num = Mathf.Clamp(num, 0f, magnitude);
		return lineStart + val * num;
	}

	private void ResetZipline()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.StopNamedSound("ZiplineTravel_");
		}
		VibrationManager.ClearAllVibration();
	}

	private void ResetZiplineMeeting()
	{
		handPool.ReclaimAll();
		ResetZipline();
	}

	private void ResetTarget(PlayerControl player)
	{
		if ((Object)(object)player != (Object)null)
		{
			player.inMovingPlat = false;
			player.moveable = true;
			((Behaviour)player.NetTransform).enabled = true;
			player.SetKinematic(b: false);
			player.ForceKillTimerContinue = false;
		}
	}

	private HandZiplinePoolable GetHand()
	{
		return handPool.Get<HandZiplinePoolable>();
	}

	private void OnDrawGizmos()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)handleTop == (Object)null) && !((Object)(object)handleBottom == (Object)null))
		{
			Color color = Gizmos.color;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(handleTop.position, handleBottom.position);
			Gizmos.color = color;
		}
	}
}
