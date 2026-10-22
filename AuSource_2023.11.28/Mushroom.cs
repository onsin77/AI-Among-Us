using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
	private const float TRIGGER_WAIT_TIME = 0.5f;

	private const string SFX_ACTIVATE_NAME = "SporeActivate";

	private const string SFX_SPAWN_NAME = "MushroomSpawn";

	private const float VIBRATION_INTENSITY = 0.5f;

	private const float VIBRATION_FALLOFF_DISTANCE = 3.5f;

	private const float VIBRATION_DURATION = 0.25f;

	private static readonly Collider2D[] Hits = new Collider2D[15];

	[SerializeField]
	private Collider2D mushroomCollider;

	[SerializeField]
	private SpriteRenderer mushroom;

	[SerializeField]
	private SpriteRenderer spores;

	[SerializeField]
	private GameObject sporeMask;

	[Tooltip("Use Fix Mushroom IDs to auto-set this value for all mushrooms on the map.")]
	[Header("Metadata")]
	[SerializeField]
	private int id;

	[SerializeField]
	[Header("Mushroom Animations")]
	private SpriteAnim mushroomAnimator;

	[SerializeField]
	private AnimationClip mushroomIdle;

	[SerializeField]
	private AnimationClip mushroomAppear;

	[SerializeField]
	private AnimationClip mushroomSteppedOn;

	[Header("Spore Cloud Animations")]
	[SerializeField]
	private SpriteAnim sporeCloudAnimator;

	[SerializeField]
	private SpriteAnim sporeCloudMaskAnimator;

	[SerializeField]
	private AnimationClip sporeCloudIdle;

	[SerializeField]
	private AnimationClip sporeCloudAppear;

	[SerializeField]
	private AnimationClip sporeCloudDisappear;

	[Header("Timings")]
	[SerializeField]
	private float secondsBetweenSporeReleases = 17f;

	[SerializeField]
	private float secondsSporeIsActive = 5f;

	[SerializeField]
	private float secondsSporeReappearsFor = 2f;

	[SerializeField]
	private float secondsSporeFade = 1f;

	[SerializeField]
	[Header("Audio")]
	private AudioClip spawnSound;

	[SerializeField]
	private AudioClip activateSporeSound;

	private ContactFilter2D filter;

	private Vector3 origPosition;

	public int Id => id;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		origPosition = ((Component)this).transform.position;
		filter = new ContactFilter2D
		{
			useLayerMask = true,
			layerMask = LayerMask.op_Implicit(Constants.LivingPlayersOnlyMask)
		};
		ResetState();
	}

	private void FixedUpdate()
	{
		if (((Behaviour)mushroomCollider).enabled && Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) && mushroomCollider.IsTouching(PlayerControl.LocalPlayer.Collider))
		{
			StartSporeTrigger(PlayerControl.LocalPlayer);
		}
	}

	public void StartSporeTrigger(PlayerControl player)
	{
		((Behaviour)mushroomCollider).enabled = false;
		((MonoBehaviour)this).StartCoroutine(WaitForSpores());
		player.CmdCheckSporeTrigger(this);
	}

	public void TriggerSpores()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)this).StopAllCoroutines();
		((Component)this).transform.position = origPosition;
		((MonoBehaviour)this).StartCoroutine(CoReleaseSpores());
	}

	private IEnumerator WaitForSpores()
	{
		yield return Effects.Shake(((Component)this).transform, 0.5f, 0.02f, taper: true);
		((Behaviour)mushroomCollider).enabled = true;
	}

	private void ResetState()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = origPosition;
		((Renderer)mushroom).enabled = true;
		mushroomAnimator.Play(mushroomIdle);
		((Behaviour)mushroomCollider).enabled = true;
		((Renderer)spores).enabled = false;
		sporeMask.SetActive(false);
	}

	private IEnumerator CoReleaseSpores()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("SporeActivate", activateSporeSound, loop: false, SoundDynamics, SoundManager.Instance.SfxChannel);
			VibrationManager.Vibrate(0.5f, Vector2.op_Implicit(((Component)this).transform.position), 3.5f, 0.25f);
		}
		((Behaviour)mushroomCollider).enabled = false;
		mushroomAnimator.Play(mushroomSteppedOn);
		yield return (object)new WaitUntil((Func<bool>)(() => mushroomAnimator.FrameTime > 2));
		sporeMask.SetActive(true);
		((Renderer)spores).enabled = true;
		sporeCloudAnimator.Play(sporeCloudAppear);
		yield return new WaitForAnimationFinish(sporeCloudMaskAnimator, sporeCloudAppear);
		sporeCloudAnimator.Play(sporeCloudIdle);
		sporeCloudMaskAnimator.Play(sporeCloudIdle);
		yield return Effects.All(Effects.Sequence(new WaitForAnimationFinish(mushroomAnimator, mushroomSteppedOn, restartIfAlreadyPlaying: false), Effects.Action(() =>
		{
			((Renderer)mushroom).enabled = false;
		})), Effects.Wait(secondsSporeIsActive));
		sporeCloudAnimator.Play(sporeCloudDisappear);
		yield return new WaitForAnimationFinish(sporeCloudMaskAnimator, sporeCloudDisappear);
		((Renderer)spores).enabled = false;
		sporeMask.SetActive(false);
		yield return Effects.Wait(secondsBetweenSporeReleases - secondsSporeFade - secondsSporeIsActive - secondsSporeReappearsFor);
		((Renderer)mushroom).enabled = true;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("MushroomSpawn", spawnSound, loop: false, SoundDynamics, SoundManager.Instance.SfxChannel);
			VibrationManager.Vibrate(0.5f, Vector2.op_Implicit(((Component)this).transform.position), 3.5f, 0.25f);
		}
		yield return new WaitForAnimationFinish(mushroomAnimator, mushroomAppear);
		ResetState();
	}

	private void SoundDynamics(AudioSource source, float dt)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			source.volume = 0f;
			return;
		}
		source.volume = 1f;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		source.volume = SoundManager.GetSoundVolume(Vector2.op_Implicit(((Component)this).transform.position), truePosition);
	}
}
