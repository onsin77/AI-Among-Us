using Rewired;
using UnityEngine;

public class LiftWeightsMinigame : Minigame
{
	private enum State
	{
		Idle,
		Lifting,
		Dropping
	}

	private const string BAR_FILL_SFX = "liftWeightsBarFillLoop";

	[SerializeField]
	private Transform barbell;

	[SerializeField]
	private Collider2D actionButton;

	[SerializeField]
	private Transform top;

	[SerializeField]
	private Transform bottom;

	[SerializeField]
	private AnimationCurve liftSpeedCurve;

	[SerializeField]
	private AnimationCurve dropSpeedCurve;

	[SerializeField]
	private float secondsToLift = 1f;

	[SerializeField]
	private SpriteRenderer[] counters;

	[SerializeField]
	private Transform fillBarParent;

	[SerializeField]
	private SpriteRenderer fillBar;

	[SerializeField]
	private Transform validIndicator;

	[SerializeField]
	private FloatRange fillBarScaleRange;

	[SerializeField]
	private FloatRange validFillPercentRange;

	[SerializeField]
	private Sprite buffSprite;

	[SerializeField]
	private Sprite weakSprite;

	[SerializeField]
	private SpriteRenderer buffButton;

	[SerializeField]
	private SpriteRenderer hands;

	[SerializeField]
	[Header("Sounds")]
	private AudioClip startLiftingSound;

	[SerializeField]
	private AudioClip barfillSound;

	[SerializeField]
	private AudioClip completeRepSound;

	[SerializeField]
	private AudioClip completeAllRepsSound;

	[SerializeField]
	private AudioClip failRepSound;

	private readonly Controller controller = new Controller();

	private Player player;

	private bool hasInput;

	private State state;

	private float currentBarFillPercent;

	private float liftStartTime;

	private float dropStartTime;

	private float secondsToDrop;

	private AudioSource barfillAudioSource;

	private void Update()
	{
		if (MyNormTask.IsComplete)
		{
			return;
		}
		bool flag = GetTouchInput() || GetControllerInput();
		if (!hasInput && flag)
		{
			StartLifting();
		}
		else if (hasInput == flag && flag)
		{
			ContinueLifting();
		}
		else if (hasInput && !flag)
		{
			EndLifting();
		}
		hasInput = flag;
		if (state == State.Dropping)
		{
			float num = currentBarFillPercent;
			num -= dropSpeedCurve.Evaluate(num);
			SetBarFill(num);
			UpdateBarbellPosition();
			if (num <= 0.01f)
			{
				state = State.Idle;
			}
		}
	}

	public override void Begin(PlayerTask task)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		SetupInput(disableCursor: true);
		player = ReInput.players.GetPlayer(0);
		state = State.Idle;
		((Component)barbell).transform.localPosition = bottom.localPosition;
		SetBarFill(0f);
		UpdateBarbellPosition();
		InitializeValidIndicator();
		MyNormTask.MaxStep = counters.Length;
		MyNormTask.ShowTaskStep = true;
		for (int i = 0; i < MyNormTask.TaskStep && i < counters.Length; i++)
		{
			counters[i].color = Color.green;
		}
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)hands);
		barfillAudioSource = SoundManager.Instance.GetNamedSfxSource("liftWeightsBarFillLoop");
		barfillAudioSource.clip = barfillSound;
		barfillAudioSource.loop = true;
		barfillAudioSource.pitch = 0f;
		barfillAudioSource.volume = 0f;
	}

	private bool GetTouchInput()
	{
		controller.Update();
		switch (controller.CheckDrag(actionButton))
		{
		case DragState.TouchStart:
		case DragState.Holding:
		case DragState.Dragging:
			return true;
		default:
			return false;
		}
	}

	public bool GetControllerInput()
	{
		if (player.GetButton(21))
		{
			return player.GetButton(24);
		}
		return false;
	}

	private void StartLifting()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (state == State.Idle)
		{
			state = State.Lifting;
			liftStartTime = Time.time;
			fillBar.color = Color.white;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(startLiftingSound, loop: false);
				barfillAudioSource.volume = 1f;
				barfillAudioSource.Play();
			}
		}
	}

	private void ContinueLifting()
	{
		if (state == State.Lifting)
		{
			float num = Mathf.Clamp01((Time.time - liftStartTime) / secondsToLift);
			SetBarFill(num);
			UpdateBarbellPosition();
			barfillAudioSource.pitch = num;
			if (num >= 1f)
			{
				barfillAudioSource.Stop();
			}
			else
			{
				VibrationManager.Vibrate(0.3f, 0.3f, 0.02f);
			}
		}
	}

	private void EndLifting()
	{
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (state != State.Lifting)
		{
			return;
		}
		if (validFillPercentRange.Contains(currentBarFillPercent))
		{
			counters[MyNormTask.taskStep].color = Color.green;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(completeRepSound, loop: false);
			}
			((MonoBehaviour)this).StartCoroutine(Effects.Bloop(0f, ((Component)counters[MyNormTask.taskStep]).transform, ((Component)counters[MyNormTask.taskStep]).transform.localScale.x));
			MyNormTask.NextStep();
			VibrationManager.Vibrate(0.7f, 0.7f, 0.2f);
			if (MyNormTask.IsComplete)
			{
				if (Constants.ShouldPlaySfx())
				{
					((MonoBehaviour)this).StartCoroutine(Effects.Sequence(Effects.Wait(0.1f), Effects.Action(() =>
					{
						SoundManager.Instance.PlaySound(completeAllRepsSound, loop: false);
					}), CoStartClose()));
				}
				else
				{
					((MonoBehaviour)this).StartCoroutine(CoStartClose());
				}
			}
		}
		else
		{
			fillBar.color = Color.red;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(failRepSound, loop: false);
			}
		}
		barfillAudioSource.Stop();
		barfillAudioSource.volume = 0f;
		state = State.Dropping;
	}

	private void SetBarFill(float t)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		currentBarFillPercent = t;
		fillBarParent.localScale = new Vector3(fillBarScaleRange.Lerp(t), fillBarParent.localScale.y, 1f);
		if (validFillPercentRange.Contains(currentBarFillPercent))
		{
			if ((Object)(object)buffButton.sprite != (Object)(object)buffSprite)
			{
				buffButton.sprite = buffSprite;
			}
		}
		else if ((Object)(object)buffButton.sprite != (Object)(object)weakSprite)
		{
			buffButton.sprite = weakSprite;
		}
	}

	private void UpdateBarbellPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(fillBarParent.localScale.x / (validFillPercentRange.max * fillBarScaleRange.Width));
		((Component)barbell).transform.localPosition = Vector3.Lerp(((Component)bottom).transform.localPosition, ((Component)top).transform.localPosition, liftSpeedCurve.Evaluate(num));
	}

	private void OnValidate()
	{
		InitializeValidIndicator();
	}

	private void InitializeValidIndicator()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)validIndicator).transform.localPosition;
		localPosition.x = validFillPercentRange.min * fillBarScaleRange.Width;
		((Component)validIndicator).transform.localPosition = localPosition;
		validIndicator.localScale = new Vector3(validFillPercentRange.Width * fillBarScaleRange.Width, validIndicator.localScale.y, 1f);
	}

	public override void Close()
	{
		barfillAudioSource.Stop();
		base.Close();
	}
}
