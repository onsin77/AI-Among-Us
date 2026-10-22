using System;
using Rewired;
using UnityEngine;

public class HoistSuppliesMinigame : Minigame
{
	private const string PULL_LOOP_NAME = "hoist_supplies_pull_loop_sfx";

	[SerializeField]
	private Collider2D draggable;

	[SerializeField]
	[Tooltip("The difficulty of the hoist minigame, increasing as the target gets closer to completion.")]
	[Header("Input")]
	private AnimationCurve liftMultiplierCurve;

	[Tooltip("The forced delay (in seconds) between each hoist controller button press. Players cannot press the hoist button faster than this.")]
	[Range(0.01f, 4f)]
	[SerializeField]
	private float controllerButtonDelay = 0.2f;

	[Tooltip("The amount added every time a controller button is pressed (Multiplied by the liftMultiplierCurve)")]
	[Range(0.01f, 2f)]
	[SerializeField]
	private float controllerRaiseAmount = 0.4f;

	[Range(0.01f, 2f)]
	[Tooltip("The amount of offset to add to the pull-rope every time the controller pull button is pressed")]
	[SerializeField]
	private float controllerPullAmount = 0.4f;

	[SerializeField]
	[Tooltip("The Speed at which the hoisted sprite animates to it's intended offset each time the controller pull button is pressed. (Recommended between the 3-15 range)")]
	[Header("Animation Speeds")]
	private float raiseAnimSpeed = 5f;

	[Tooltip("The Speed at which the pull-rope animates to it's intended offset each time the controller pull button is pressed. (Recommended between the 3-15 range)")]
	[SerializeField]
	private float pullAnimSpeed = 5f;

	[Header("Rope")]
	[SerializeField]
	private SpriteRenderer rope;

	[SerializeField]
	private Vector3 ropeTopPos;

	[SerializeField]
	private Vector3 ropeBottomPos;

	[SerializeField]
	private FloatRange ropeLength;

	[Header("Basket")]
	[SerializeField]
	private Transform basket;

	[SerializeField]
	private FloatRange basketSize;

	[SerializeField]
	private Vector3 basketTopPos;

	[SerializeField]
	private Vector3 basketBottomPos;

	[SerializeField]
	[Header("Draggable Rope")]
	private Vector3 draggableRopeTopPos;

	[SerializeField]
	private Vector3 draggableRopeBottomPos;

	[SerializeField]
	private Transform ropeTexture;

	[SerializeField]
	[Header("Lights")]
	private GameObject redCircle;

	[SerializeField]
	private GameObject greenCircle;

	[SerializeField]
	[Header("Controller Hint")]
	private GameObject controllerHintContainer;

	[SerializeField]
	[Header("Audio")]
	private AudioClip[] pullOneShotSfx;

	[SerializeField]
	private AudioClip pullLoopSfx;

	[SerializeField]
	[Space(10f)]
	private AnimationCurve pullLoopSfxVolumeCurve;

	[SerializeField]
	private float pullLoopSfxVolumeSpeed = 20f;

	private readonly Controller controller = new Controller();

	private Player player;

	private bool completed;

	private Vector2 lastDragPosition;

	private float targetPullLoopVolume;

	private float truePullLoopVolume;

	private float targetRaisedValue;

	private float trueRaisedValue;

	private float targetPullValue;

	private float truePullValue;

	private float raisedValueOnDragStart;

	private float pullValueOnDragStart;

	private float lastControllerInputTime;

	private void Update()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		truePullLoopVolume = Mathf.Lerp(truePullLoopVolume, targetPullLoopVolume, Time.deltaTime * pullLoopSfxVolumeSpeed);
		targetPullLoopVolume = 0f;
		if (completed || amClosing != CloseState.None)
		{
			return;
		}
		if (((Behaviour)draggable).enabled)
		{
			controller.Update();
			switch (controller.CheckDrag(draggable))
			{
			case DragState.TouchStart:
				raisedValueOnDragStart = trueRaisedValue;
				pullValueOnDragStart = truePullValue;
				break;
			case DragState.Dragging:
			{
				Vector2 val = controller.DragPosition - controller.DragStartPosition;
				Vector2 val2 = lastDragPosition - controller.DragPosition;
				float num = val2.magnitude / Time.deltaTime;
				lastDragPosition = controller.DragPosition;
				if (!(val.y > 0f))
				{
					float num2 = Mathf.Clamp01(val.magnitude / Vector3.Distance(draggableRopeBottomPos, draggableRopeTopPos));
					targetPullValue = (truePullValue = pullValueOnDragStart + num2);
					targetRaisedValue = (trueRaisedValue = raisedValueOnDragStart + num2 * liftMultiplierCurve.Evaluate(targetRaisedValue));
					targetPullLoopVolume = Mathf.Clamp01((num - 0.3f) / 3.7f);
				}
				break;
			}
			case DragState.Released:
				((Component)draggable).transform.localPosition = draggableRopeTopPos;
				break;
			}
		}
		if (Time.time > lastControllerInputTime + controllerButtonDelay && (player.GetButtonDown(21) || player.GetButtonDown(24)))
		{
			lastControllerInputTime = Time.time;
			targetRaisedValue += liftMultiplierCurve.Evaluate(targetRaisedValue) * controllerRaiseAmount;
			targetPullValue += controllerPullAmount;
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(pullOneShotSfx.Random(), loop: false);
			}
		}
		truePullValue = Mathf.Lerp(truePullValue, targetPullValue, pullAnimSpeed * Time.deltaTime);
		SetPullPosition(truePullValue);
		trueRaisedValue = Mathf.Lerp(trueRaisedValue, targetRaisedValue, raiseAnimSpeed * Time.deltaTime);
		SetRopePosition(1f - trueRaisedValue);
		if (trueRaisedValue >= 1f)
		{
			completed = true;
			((Behaviour)draggable).enabled = false;
			MyNormTask.NextStep();
			greenCircle.SetActive(true);
			redCircle.SetActive(false);
			((MonoBehaviour)this).StartCoroutine(Effects.ActionAfterDelay(0.5f, () =>
			{
				((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
			}));
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: true);
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		OnInputMethodChanged();
		player = ReInput.players.GetPlayer(0);
		redCircle.SetActive(true);
		greenCircle.SetActive(false);
		truePullLoopVolume = 0f;
		targetPullLoopVolume = 0f;
		SoundManager.Instance.PlayDynamicSound("hoist_supplies_pull_loop_sfx", pullLoopSfx, loop: true, PullLoopDynamics, SoundManager.Instance.SfxChannel);
	}

	public override void Close()
	{
		base.Close();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		SoundManager.Instance.StopNamedSound("hoist_supplies_pull_loop_sfx");
	}

	[ContextMenu("ResetRope")]
	private void ResetRope()
	{
		SetRopePosition(1f);
	}

	[ContextMenu("Complete Hoist")]
	private void CompleteHoist()
	{
		SetRopePosition(0f);
	}

	private void SetRopePosition(float distanceFromTop)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		distanceFromTop = Mathf.Clamp(distanceFromTop, -0.2f, 1f);
		((Component)rope).transform.localPosition = Vector3.Lerp(ropeTopPos, ropeBottomPos, distanceFromTop);
		rope.size = new Vector2(0.14f, ropeLength.Lerp(distanceFromTop));
		float num = basketSize.Lerp(distanceFromTop);
		((Component)basket).transform.localScale = new Vector3(num, num, 1f);
		((Component)basket).transform.localPosition = Vector3.Lerp(basketTopPos, basketBottomPos, distanceFromTop);
	}

	private void SetPullPosition(float amount)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Component)ropeTexture).transform.localPosition = Vector3.Lerp(draggableRopeTopPos, draggableRopeBottomPos, Mathf.Repeat(amount, 1f));
	}

	private void OnInputMethodChanged()
	{
		controllerHintContainer.SetActive(ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}

	private void PullLoopDynamics(AudioSource source, float dt)
	{
		source.volume = pullLoopSfxVolumeCurve.Evaluate(truePullLoopVolume);
	}
}
