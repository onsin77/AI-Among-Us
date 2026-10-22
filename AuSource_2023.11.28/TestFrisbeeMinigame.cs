using System.Collections;
using PowerTools;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class TestFrisbeeMinigame : Minigame
{
	[Header("Design Values")]
	[SerializeField]
	[Range(0.5f, 0.99f)]
	private float pickUpAnalogThresshold = 0.9f;

	[Header("Components")]
	[SerializeField]
	private PassiveButton stuckFrisbee;

	[SerializeField]
	private Collider2D throwableFrisbee;

	[SerializeField]
	private SpriteAnim sparkle;

	[SerializeField]
	private GameObject throwControllerGlyph;

	[SerializeField]
	[Tooltip("The number of seconds frisbee spends in the air after tossing, before it disappears")]
	private float totalSeconds = 0.5f;

	[SerializeField]
	private Transform finalFrisbeePosition;

	[SerializeField]
	[Header("Sounds")]
	private AudioClip grabSound;

	[SerializeField]
	private AudioClip throwSound;

	[SerializeField]
	private AudioClip twinkleSound;

	private readonly Controller controller = new Controller();

	private Vector3 originalFrisbeePosition;

	private Vector3 throwStartPosition;

	private bool pickedUp;

	private bool thrown;

	private float time;

	private bool isCompleting;

	private Player player;

	private void Update()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		if (MyNormTask.IsComplete || amClosing != CloseState.None || isCompleting)
		{
			return;
		}
		if (!pickedUp)
		{
			if (player.GetAxis(17) >= pickUpAnalogThresshold)
			{
				PickUpFrisbee();
			}
			return;
		}
		if (thrown)
		{
			time += Time.deltaTime;
			float num = time / totalSeconds;
			((Component)throwableFrisbee).transform.localPosition = Vector3.Lerp(throwStartPosition, finalFrisbeePosition.localPosition, num);
			((Component)throwableFrisbee).transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, num);
			if (Mathf.Approximately(0f, ((Component)throwableFrisbee).transform.localScale.x))
			{
				thrown = false;
				((MonoBehaviour)this).StartCoroutine(CoFinalAnimation());
			}
			return;
		}
		controller.Update();
		switch (controller.CheckDrag(throwableFrisbee))
		{
		case DragState.Dragging:
			if (!thrown && !MyNormTask.IsComplete)
			{
				Vector2 val2 = controller.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
				Vector3 localPosition = ((Component)throwableFrisbee).transform.localPosition;
				if (val2.x < localPosition.x || val2.y < localPosition.y)
				{
					return;
				}
				localPosition.x = val2.x;
				localPosition.y = val2.y;
				((Component)throwableFrisbee).transform.localPosition = localPosition;
				if (localPosition.x - originalFrisbeePosition.x > 0.2f && localPosition.y - originalFrisbeePosition.y > 0.2f)
				{
					ThrowFrisbee();
				}
			}
			break;
		case DragState.Released:
		{
			Vector2 val = Vector2.op_Implicit(((Component)throwableFrisbee).transform.localPosition - originalFrisbeePosition);
			if (val.y < 0f || val.x < 0f || val.magnitude < 0.2f)
			{
				((Component)throwableFrisbee).transform.localPosition = originalFrisbeePosition;
				return;
			}
			ThrowFrisbee();
			break;
		}
		}
		if (!thrown && !MyNormTask.IsComplete && player.GetAxis(17) > 0.6f && player.GetAxis(16) > 0.6f)
		{
			ThrowFrisbee();
		}
	}

	private IEnumerator CoFinalAnimation()
	{
		isCompleting = true;
		yield return Effects.Wait(0.3f);
		((Component)sparkle).gameObject.SetActive(true);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(twinkleSound, loop: false);
		}
		yield return Effects.Wait(0.5f);
		((Component)sparkle).gameObject.SetActive(false);
		yield return Effects.Wait(0.25f);
		MyNormTask.NextStep();
		yield return CoStartClose();
	}

	public override void Begin(PlayerTask task)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected Obj, but got Unknown
		base.Begin(task);
		SetupInput(disableCursor: true);
		player = ReInput.players.GetPlayer(0);
		((Component)stuckFrisbee).gameObject.SetActive(true);
		((Component)throwableFrisbee).gameObject.SetActive(false);
		((UnityEvent)stuckFrisbee.OnClick).AddListener((UnityAction)PickUpFrisbee);
	}

	private void PickUpFrisbee()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		pickedUp = true;
		((Component)stuckFrisbee).gameObject.SetActive(false);
		((Component)throwableFrisbee).gameObject.SetActive(true);
		originalFrisbeePosition = ((Component)throwableFrisbee).transform.localPosition;
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(grabSound, loop: false);
		}
	}

	private void ThrowFrisbee()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		throwControllerGlyph.SetActive(false);
		thrown = true;
		throwStartPosition = ((Component)throwableFrisbee).transform.localPosition;
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(throwSound, loop: false);
		}
	}

	[ContextMenu("Reset Frisbee Position")]
	private void ResetFrisbeePosition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		((Component)throwableFrisbee).transform.localPosition = originalFrisbeePosition;
		((Component)throwableFrisbee).transform.localScale = Vector3.one;
	}
}
