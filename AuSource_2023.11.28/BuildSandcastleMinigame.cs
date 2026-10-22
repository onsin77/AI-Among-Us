using Rewired;
using UnityEngine;

public class BuildSandcastleMinigame : Minigame
{
	private const float CLOSE_ENOUGH_TO_TOP = 0.8f;

	[Tooltip("The speed at which lifting with an analog stick moves towards the true lift value.")]
	[SerializeField]
	[Header("Timing")]
	private float liftLerpSpeed = 5f;

	[Tooltip("The acceleration at which the bucket falls back down, per second. (Added to fallVelocity per second)")]
	[SerializeField]
	private float fallGravity = 0.5f;

	[Header("Lift Range")]
	[SerializeField]
	private Transform startPosition;

	[SerializeField]
	private Transform endPosition;

	[Header("Bucket")]
	[SerializeField]
	private Collider2D bucketCollider;

	[SerializeField]
	private SpriteRenderer bucketSprite;

	[SerializeField]
	private SpriteRenderer bucketBackSprite;

	[SerializeField]
	private GameObject bucketGlyph;

	[Header("Sounds")]
	[SerializeField]
	private AudioClip grabSound;

	[SerializeField]
	private AudioClip completeSound;

	private readonly Controller controller = new Controller();

	private Player player;

	private bool hasInput;

	private float fallVelocity;

	private float targetBucketY;

	private float trueBucketY;

	private void Update()
	{
		if (MyNormTask.IsComplete)
		{
			return;
		}
		bool flag = hasInput;
		if (((Behaviour)bucketCollider).enabled)
		{
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
			{
				UpdateJoystickInput();
			}
			else
			{
				UpdateDefaultInput();
			}
		}
		if (flag && !hasInput)
		{
			fallVelocity = 0f;
		}
		if (hasInput && !flag && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(grabSound, loop: false);
		}
		if (trueBucketY >= 0.8f)
		{
			Success();
			return;
		}
		if (!hasInput && trueBucketY > 0f)
		{
			fallVelocity += fallGravity;
			SetBucketY(trueBucketY - fallVelocity * Time.deltaTime, instant: true);
		}
		UpdateBucketPosition();
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: true);
		player = ReInput.players.GetPlayer(0);
	}

	private void UpdateDefaultInput()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		hasInput = false;
		controller.Update();
		DragState dragState = controller.CheckDrag(bucketCollider);
		if ((uint)(dragState - 1) <= 2u)
		{
			hasInput = true;
			float num = endPosition.position.y - controller.DragPosition.y;
			float newValue = 1f - Mathf.Clamp01(num / Vector2.Distance(Vector2.op_Implicit(startPosition.position), Vector2.op_Implicit(endPosition.position)));
			SetBucketY(newValue, instant: true);
		}
	}

	private void UpdateJoystickInput()
	{
		hasInput = false;
		if (player.GetAxisRaw(14) > 0.01f)
		{
			hasInput = true;
			SetBucketY(player.GetAxisRaw(14), instant: false);
		}
	}

	private void SetBucketY(float newValue, bool instant)
	{
		targetBucketY = Mathf.Clamp01(newValue);
		if (instant)
		{
			trueBucketY = targetBucketY;
		}
		else
		{
			trueBucketY = Mathf.Lerp(trueBucketY, targetBucketY, Time.deltaTime * liftLerpSpeed);
		}
	}

	private void UpdateBucketPosition()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		((Component)bucketCollider).transform.localPosition = Vector3.Lerp(startPosition.localPosition, endPosition.localPosition, trueBucketY);
	}

	private void Success()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)bucketCollider).enabled = false;
		bucketGlyph.SetActive(false);
		((MonoBehaviour)this).StartCoroutine(Effects.Sequence(Effects.All(Effects.ColorFade(bucketSprite, Color.white, Color.clear, 0.5f), Effects.ColorFade(bucketBackSprite, Color.white, Color.clear, 0.5f)), CoStartClose()));
		MyNormTask.NextStep();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(completeSound, loop: false);
		}
	}
}
