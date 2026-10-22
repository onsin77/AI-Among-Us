using System.Collections;
using Rewired;
using UnityEngine;

public class RoastMarshmallowFireMinigame : Minigame
{
	private enum State
	{
		Waiting,
		Placing,
		Toasting,
		Taking,
		Removing_Fail,
		Removing_Success
	}

	private enum MarshmallowState
	{
		New,
		Toasted,
		Burnt
	}

	public const int FIRE_CONSOLE_ID = 0;

	private const string BAG_TRIGGER_OUT = "OnOut";

	private const string BAG_TRIGGER_RESET = "OnReset";

	private const string STICK_TRIGGER_GET = "OnGet";

	private const string STICK_TRIGGER_TAKE = "OnTake";

	private const string STICK_TRIGGER_RESET = "OnReset";

	private const string MARSHMALLOW_INT_STATE = "State";

	private const string FIRE_BOOL_VISIBLE = "Visible";

	private const string FIRE_AMBIENCE_SFX = "MarshmallowFireAmbience";

	private static readonly Color MARSHMALLOW_TOASTED_COLOR_START = Palette.ClearWhite;

	private static readonly Color MARSHMALLOW_TOASTED_COLOR_END = Palette.White;

	[Header("Parameters")]
	[SerializeField]
	[Range(0.1f, 10f)]
	private float timeToToasted = 5f;

	[Range(0.1f, 10f)]
	[SerializeField]
	private float timeToBurnt = 3f;

	[Header("Animators")]
	[SerializeField]
	private Animator fireAnimator;

	[SerializeField]
	private Animator bagAnimator;

	[SerializeField]
	private Animator stickAnimator;

	[SerializeField]
	private Animator marshmallowAnimator;

	[Space(10f)]
	[SerializeField]
	private SpriteRenderer toastedMarshmallow;

	[Header("Buttons")]
	[SerializeField]
	private PassiveButton bagMarshmallowButton;

	[SerializeField]
	private PassiveButton stickMarshmallowButton;

	[SerializeField]
	[Header("Audio")]
	private AudioClip sfxMarshmallowOnStick;

	[SerializeField]
	private AudioClip sfxMarshmallowDone;

	[SerializeField]
	private AudioClip sfxTooSoon;

	[SerializeField]
	private AudioClip sfxBurnt;

	[SerializeField]
	private AudioClip sfxFireAmbience;

	private Player player;

	private State state;

	private MarshmallowState marshmallowState;

	private float roastingStartTime;

	private float timeOfLastStateTransition;

	private void Awake()
	{
		bagMarshmallowButton.AddOnClickListeners(OnBagMarshmallowPressed);
		stickMarshmallowButton.AddOnClickListeners(OnStickMarshmallowPressed);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		player = ReInput.players.GetPlayer(0);
		state = State.Waiting;
		marshmallowState = (MarshmallowState)(-1);
		SetMarshmallowState(MarshmallowState.New);
		SetToastedValue(0f);
		SoundManager.Instance.PlayDynamicSound("MarshmallowFireAmbience", sfxFireAmbience, loop: true, FireDynamics, SoundManager.Instance.AmbienceChannel);
	}

	private void FireDynamics(AudioSource source, float dt)
	{
		if (state == State.Toasting)
		{
			source.volume = Mathf.Clamp01(Time.time - timeOfLastStateTransition);
		}
		else if (source.volume > 0f)
		{
			source.volume -= dt;
		}
	}

	private void Update()
	{
		if (player.GetButtonDown(11))
		{
			if (state == State.Waiting && ((Component)bagMarshmallowButton).gameObject.activeInHierarchy)
			{
				OnBagMarshmallowPressed();
			}
			else if (state == State.Toasting && ((Component)stickMarshmallowButton).gameObject.activeInHierarchy)
			{
				OnStickMarshmallowPressed();
			}
		}
		if (state == State.Toasting)
		{
			float num = Mathf.Clamp01((Time.time - roastingStartTime) / timeToToasted);
			if (num < 1f)
			{
				VibrationManager.Vibrate(0.3f, 0.3f, 0.02f);
				SetToastedValue(num);
			}
			else if (Time.time >= roastingStartTime + timeToToasted + timeToBurnt)
			{
				SetMarshmallowState(MarshmallowState.Burnt);
			}
			else if (Time.time >= roastingStartTime + timeToToasted)
			{
				SetMarshmallowState(MarshmallowState.Toasted);
			}
		}
	}

	private void SetState(State newState)
	{
		if (newState != state)
		{
			switch (newState)
			{
			default:
				Debug.LogError((object)string.Format("[{0}] State '{1}' has not been configured.", "RoastMarshmallowFireMinigame", state));
				break;
			case State.Waiting:
				fireAnimator.SetBool("Visible", false);
				bagAnimator.ResetTrigger("OnOut");
				bagAnimator.SetTrigger("OnReset");
				stickAnimator.ResetTrigger("OnGet");
				SetMarshmallowState(MarshmallowState.New);
				break;
			case State.Placing:
				((MonoBehaviour)this).StartCoroutine(CoPlacing());
				break;
			case State.Toasting:
				marshmallowAnimator.SetInteger("State", 0);
				roastingStartTime = Time.time;
				break;
			case State.Taking:
				((MonoBehaviour)this).StartCoroutine(CoTake(marshmallowState == MarshmallowState.Toasted));
				break;
			case State.Removing_Fail:
			case State.Removing_Success:
				break;
			}
			timeOfLastStateTransition = Time.time;
			state = newState;
		}
	}

	private void SetMarshmallowState(MarshmallowState newMarshmallowState)
	{
		if (marshmallowState == newMarshmallowState)
		{
			return;
		}
		marshmallowState = newMarshmallowState;
		marshmallowAnimator.SetInteger("State", (int)newMarshmallowState);
		switch (newMarshmallowState)
		{
		default:
			Debug.LogError((object)string.Format("[{0}] {1} '{2}' has not been configured.", "RoastMarshmallowFireMinigame", "MarshmallowState", newMarshmallowState));
			break;
		case MarshmallowState.New:
			SetToastedValue(0f);
			break;
		case MarshmallowState.Toasted:
			SetToastedValue(1f);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(sfxMarshmallowDone, loop: false);
			}
			break;
		case MarshmallowState.Burnt:
			SetToastedValue(0f);
			VibrationManager.Vibrate(0.7f, 0.7f, 0.1f);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(sfxBurnt, loop: false);
			}
			break;
		}
	}

	private void SetToastedValue(float value)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		toastedMarshmallow.color = Color.Lerp(MARSHMALLOW_TOASTED_COLOR_START, MARSHMALLOW_TOASTED_COLOR_END, value);
	}

	private void OnBagMarshmallowPressed()
	{
		if (state == State.Waiting)
		{
			SetState(State.Placing);
		}
	}

	private void OnStickMarshmallowPressed()
	{
		if (state != State.Toasting)
		{
			return;
		}
		if (marshmallowState == MarshmallowState.New)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(sfxTooSoon, loop: false);
			}
		}
		else
		{
			SetState(State.Taking);
		}
	}

	private IEnumerator CoPlacing()
	{
		SetMarshmallowState(MarshmallowState.New);
		bagAnimator.SetTrigger("OnOut");
		yield return (object)new WaitForSeconds(0.5f);
		stickAnimator.SetTrigger("OnGet");
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(sfxMarshmallowOnStick, loop: false);
		}
		yield return (object)new WaitForSeconds(0.25f);
		fireAnimator.SetBool("Visible", true);
		SetState(State.Toasting);
	}

	private IEnumerator CoTake(bool success)
	{
		stickAnimator.SetTrigger("OnTake");
		fireAnimator.SetBool("Visible", false);
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(sfxMarshmallowOnStick, loop: false);
		}
		yield return (object)new WaitForSeconds(1f);
		if (success)
		{
			SetState(State.Removing_Success);
			MyNormTask.NextStep();
			yield return (object)new WaitForSeconds(1f);
			((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
		}
		else if (marshmallowState == MarshmallowState.Burnt)
		{
			SetState(State.Removing_Fail);
			stickAnimator.SetTrigger("OnReset");
			yield return (object)new WaitForSeconds(1f);
			SetState(State.Waiting);
		}
	}

	public override void Close()
	{
		state = State.Waiting;
		base.Close();
	}

	static RoastMarshmallowFireMinigame()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
	}
}
