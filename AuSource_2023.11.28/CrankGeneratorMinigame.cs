using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class CrankGeneratorMinigame : Minigame
{
	private static readonly string SHADER_Y_PERCENT = "_PercentY";

	private static readonly string CRANK_MOTOR_SFX = "crankMotorLoop";

	[Tooltip("Indicates how much spinning its needed to complete task")]
	[SerializeField]
	private float targetProgress = 2000f;

	[Tooltip("How fast does machine loses power when not spinning")]
	[SerializeField]
	private float progressDecay = 250f;

	[Tooltip("CrankAudioPlays targetprogress/lightbulbRate")]
	[SerializeField]
	private int crankAudioRate;

	[SerializeField]
	private SpriteRenderer powerBarSpriteRenderer;

	[SerializeField]
	private Collider2D crank;

	[SerializeField]
	private Transform checkpoint;

	[SerializeField]
	private SpriteRenderer lightbulb;

	[SerializeField]
	private SpriteRenderer miniLights;

	[SerializeField]
	private AudioClip[] turnAudioClips;

	[SerializeField]
	private AudioClip motorAudioClip;

	[Tooltip("Multiplier to make cranking easier/harder on gamepad")]
	private float gamepadAngleProgressMul = 0.75f;

	private readonly Controller controller = new Controller();

	private Player gamepad;

	private Vector2? prevGamepadAngle;

	private float progress;

	private float currentAngle;

	private bool lightbulbEffect;

	private int crankAudioDiv;

	private AudioSource motorAudioSource;

	private List<AudioClip> randomAudioClips;

	private bool finishCrank;

	private void Update()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (finishCrank)
		{
			return;
		}
		controller.Update();
		switch (controller.CheckDrag(crank))
		{
		case DragState.TouchStart:
			currentAngle = Vector3.SignedAngle(((Component)checkpoint).transform.up, ((Component)crank).transform.up, Vector3.forward);
			break;
		case DragState.Dragging:
		{
			Vector2 val = controller.DragPosition - Vector2.op_Implicit(((Component)crank).transform.position);
			Vector3 up = Vector2.op_Implicit(val.normalized);
			((Component)crank).transform.up = up;
			float num = Vector3.SignedAngle(((Component)checkpoint).transform.up, ((Component)crank).transform.up, Vector3.forward);
			if (num < 0f)
			{
				num += 360f;
			}
			float num2 = currentAngle - num;
			if (num2 >= 0f && num2 < 50f)
			{
				progress += num2;
			}
			currentAngle = num;
			break;
		}
		}
		UpdateGamepadInput();
		progress = Mathf.Clamp(progress - progressDecay * Time.deltaTime, 0f, targetProgress);
		float num3 = Mathf.Clamp(progress / targetProgress, 0f, 1f);
		float num4 = Mathf.Lerp(0f, 1f, 1f - num3);
		((Renderer)powerBarSpriteRenderer).material.SetFloat(SHADER_Y_PERCENT, num4);
		if (Constants.ShouldPlaySfx())
		{
			motorAudioSource.volume = num3;
			motorAudioSource.pitch = Mathf.Floor(num3 * 100f) / 100f;
		}
		if (Constants.ShouldPlaySfx())
		{
			if (progress < targetProgress / (float)crankAudioRate * (float)(crankAudioDiv - 1))
			{
				crankAudioDiv = Mathf.Clamp(crankAudioDiv - 1, 1, crankAudioRate);
			}
			if (progress >= targetProgress / (float)crankAudioRate * (float)crankAudioDiv && crankAudioDiv < crankAudioRate)
			{
				crankAudioDiv = Mathf.Clamp(crankAudioDiv + 1, 1, crankAudioRate);
				SoundManager.Instance.PlaySound(GetRandomNonRepeatSound(), loop: false);
			}
		}
		if (progress >= targetProgress)
		{
			TurnOnLights();
			finishCrank = true;
			((MonoBehaviour)this).StartCoroutine(WaitComplete());
		}
	}

	private IEnumerator WaitComplete()
	{
		yield return Effects.Wait(0.8f);
		MyNormTask.NextStep();
		if (MyNormTask.IsComplete)
		{
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: true);
		gamepad = ReInput.players.GetPlayer(0);
		progress = 0f;
		((Renderer)powerBarSpriteRenderer).material.SetFloat(SHADER_Y_PERCENT, progress);
		crankAudioDiv = 1;
		finishCrank = false;
		motorAudioSource = SoundManager.Instance.GetNamedSfxSource(CRANK_MOTOR_SFX);
		motorAudioSource.clip = motorAudioClip;
		motorAudioSource.loop = true;
		motorAudioSource.pitch = 0f;
		motorAudioSource.volume = 0f;
		motorAudioSource.Play();
		randomAudioClips = new List<AudioClip>();
	}

	public override void Close()
	{
		finishCrank = true;
		base.Close();
		SoundManager.Instance.StopNamedSound(CRANK_MOTOR_SFX);
	}

	private void UpdateGamepadInput()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (gamepad == null)
		{
			return;
		}
		Vector2 axis2DRaw = gamepad.GetAxis2DRaw(13, 14);
		Vector2 normalized = axis2DRaw.normalized;
		if (normalized.magnitude != 0f)
		{
			if (!prevGamepadAngle.HasValue)
			{
				prevGamepadAngle = normalized;
				return;
			}
			float num = Vector2.SignedAngle(normalized, prevGamepadAngle.Value);
			num = Mathf.Clamp(num, 0f, 50f) * gamepadAngleProgressMul;
			progress += num;
			VibrationManager.Vibrate(0.3f, 0.3f, 0.02f);
			((Component)crank).transform.up = Vector2.op_Implicit(normalized);
			prevGamepadAngle = normalized;
		}
	}

	private void TurnOnLights()
	{
		((Component)lightbulb).gameObject.SetActive(true);
		((Component)miniLights).gameObject.SetActive(true);
	}

	private AudioClip GetRandomNonRepeatSound()
	{
		randomAudioClips = new List<AudioClip>(turnAudioClips);
		AudioClip val = randomAudioClips.Random();
		randomAudioClips.Remove(val);
		return val;
	}
}
