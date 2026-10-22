using System.Collections;
using Rewired;
using UnityEngine;

public class CatchFishMinigame : Minigame
{
	private enum State
	{
		WaitingForFish,
		FishOnHook,
		FishEscaped
	}

	public const int NUM_FISH_TO_CATCH = 3;

	public const int NUM_STEPS_IN_CATCHING_FISH = 2;

	private const float VIBRATION_INTENSITY = 0.7f;

	private const float SECONDS_TO_WAIT_FOR_FISH_TO_BE_CAUGHT = 5f;

	private const float LIGHTLY_BOBBING = 0.02f;

	private const float STRONGLY_BOBBING = 0.1f;

	[Header("Fishing Rod")]
	[SerializeField]
	private Transform fishingRod;

	[SerializeField]
	private SpriteRenderer fishingLine;

	[SerializeField]
	private Vector3 rodPutAwayPosition;

	[SerializeField]
	private FloatRange fishingRodXPositionRange;

	[SerializeField]
	private SpriteRenderer bauble;

	[Header("Fish Swimming")]
	[SerializeField]
	private SpriteRenderer currentFish;

	[SerializeField]
	private float[] startXPositions;

	[SerializeField]
	private FloatRange startYPositionRange;

	[SerializeField]
	private AnimationCurve[] fishMovementCurves;

	[SerializeField]
	private float totalSwimTime;

	[Header("Fish Caught")]
	[SerializeField]
	private SpriteRenderer[] fishCounters;

	[SerializeField]
	private Sprite[] fishCounterSprites;

	[SerializeField]
	private SpriteRenderer caughtFish;

	[SerializeField]
	private Transform alert;

	[SerializeField]
	private float caughtFishScale;

	[Header("Audio")]
	[SerializeField]
	private AudioClip onHookSfx;

	[SerializeField]
	private AudioClip onMissSfx;

	[SerializeField]
	private AudioClip onCatchSfx;

	private readonly Controller controller = new Controller();

	private State currentState;

	private int numCaughtFish;

	private Coroutine bounceCoroutine;

	private void Update()
	{
		if (numCaughtFish < 3)
		{
			controller.Update();
			if (currentState == State.FishOnHook && IsInputCatchFish())
			{
				((MonoBehaviour)this).StopAllCoroutines();
				((MonoBehaviour)this).StartCoroutine(Effects.Sequence(CoPutAwayFishingRod(), CoAnimateCaughtFish(), CoUpdateProgress()));
			}
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: true);
		if (3 != fishCounters.Length)
		{
			Logger.GlobalInstance.Error($"fishCounters.Length was {fishCounters.Length} but should be {3}; task is probably not completable");
		}
		MyNormTask.MaxStep = 3;
		numCaughtFish = MyNormTask.TaskStep;
		for (int i = 0; i < numCaughtFish; i++)
		{
			SetCaughtFish(i);
		}
		((MonoBehaviour)this).StartCoroutine(CoBeginFishing());
	}

	private bool IsInputCatchFish()
	{
		if (!controller.AnyTouchUp)
		{
			return ReInput.players.GetPlayer(0).GetButtonDown(21);
		}
		return true;
	}

	private void SetCaughtFish(int index)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		byte b = MyNormTask.Data[index + 1];
		fishCounters[index].sprite = fishCounterSprites[b];
		((Component)fishCounters[index]).transform.localScale = Vector3.one * caughtFishScale;
	}

	private IEnumerator CoBeginFishing()
	{
		while (true)
		{
			yield return CoPrepareToFish();
			yield return CoMoveFishToBauble();
			yield return CoWaitForFishToBeReeledIn();
			yield return CoFishEscapes();
		}
	}

	private IEnumerator CoPrepareToFish()
	{
		if (bounceCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(bounceCoroutine);
			bounceCoroutine = null;
		}
		currentState = State.WaitingForFish;
		currentFish.color = Palette.ClearWhite;
		caughtFish.color = Palette.ClearWhite;
		((Component)alert).gameObject.SetActive(false);
		((Component)bauble).gameObject.SetActive(false);
		fishingLine.color = Palette.ClearWhite;
		Vector3 localPosition = fishingRod.localPosition;
		localPosition.x = fishingRodXPositionRange.min;
		fishingRod.localPosition = localPosition;
		Vector3 eulerAngles = ((Component)fishingRod).transform.eulerAngles;
		eulerAngles.z = 0f;
		((Component)fishingRod).transform.eulerAngles = eulerAngles;
		Vector3 val = localPosition;
		val.x = fishingRodXPositionRange.Next();
		yield return Effects.Slide2D(fishingRod, Vector2.op_Implicit(localPosition), Vector2.op_Implicit(val));
		yield return Effects.ColorFade(fishingLine, Palette.ClearWhite, Color.white, 0.75f);
		((Component)bauble).gameObject.SetActive(true);
		bounceCoroutine = ((MonoBehaviour)this).StartCoroutine(CoBounceBauble(0.02f));
		currentFish.color = Color.white;
	}

	private IEnumerator CoBounceBauble(float bounciness)
	{
		while (true)
		{
			yield return Effects.ScaleIn(((Component)bauble).transform, 1f - bounciness, 1f + bounciness, 0.2f);
			yield return Effects.ScaleIn(((Component)bauble).transform, 1f + bounciness, 1f - bounciness, 0.2f);
		}
	}

	private IEnumerator CoMoveFishToBauble()
	{
		Vector3 localPosition = ((Component)currentFish).transform.localPosition;
		float num = startXPositions.Random();
		float y = startYPositionRange.Next();
		localPosition.x = num;
		localPosition.y = y;
		((Component)currentFish).transform.localPosition = localPosition;
		float num2 = ((Component)bauble).transform.position.x - ((Component)currentFish).transform.position.x;
		num2 -= Mathf.Sign(num2) * 0.75f;
		float fishEndPosX = num + num2;
		yield return CoMoveFishToLocation(fishEndPosX);
		VibrationManager.Vibrate(0.7f, 0.7f, 0.1f, VibrationManager.VibrationFalloff.Linear);
		((Component)alert).gameObject.SetActive(true);
		Vector3 localPosition2 = ((Component)alert).transform.localPosition;
		localPosition2 = new Vector3(localPosition2.x + (((Component)bauble).transform.position.x - ((Component)alert).transform.position.x) - 0.5f, localPosition2.y, localPosition2.z);
		((Component)alert).transform.localPosition = localPosition2;
		currentState = State.FishOnHook;
		((MonoBehaviour)this).StopCoroutine(bounceCoroutine);
		bounceCoroutine = ((MonoBehaviour)this).StartCoroutine(CoBounceBauble(0.1f));
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(onHookSfx, loop: false);
		}
	}

	private IEnumerator CoWaitForFishToBeReeledIn()
	{
		yield return Effects.Wait(5f);
	}

	private IEnumerator CoFishEscapes()
	{
		((Component)alert).gameObject.SetActive(false);
		currentState = State.FishEscaped;
		((MonoBehaviour)this).StopCoroutine(bounceCoroutine);
		bounceCoroutine = ((MonoBehaviour)this).StartCoroutine(CoBounceBauble(0.02f));
		float fishEndPosX = startXPositions.Random();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(onMissSfx, loop: false);
		}
		yield return CoMoveFishToLocation(fishEndPosX);
	}

	private IEnumerator CoMoveFishToLocation(float fishEndPosX)
	{
		Vector3 localPosition = ((Component)currentFish).transform.localPosition;
		float fishStartPosX = localPosition.x;
		float fishStartPosY = localPosition.y;
		currentFish.flipX = fishStartPosX < fishEndPosX;
		AnimationCurve movementCurve = fishMovementCurves.Random();
		float time = 0f;
		while (time < totalSwimTime)
		{
			time += Time.deltaTime;
			float num = time / totalSwimTime;
			Vector3 localPosition2 = ((Component)currentFish).transform.localPosition;
			localPosition2.x = Mathf.Lerp(fishStartPosX, fishEndPosX, num);
			localPosition2.y = fishStartPosY + movementCurve.Evaluate(num);
			((Component)currentFish).transform.localPosition = localPosition2;
			yield return null;
		}
	}

	private IEnumerator CoPutAwayFishingRod()
	{
		currentState = State.WaitingForFish;
		Vector3 eulerAngles = ((Component)fishingRod).transform.eulerAngles;
		eulerAngles.z = -27f;
		((Component)fishingRod).transform.eulerAngles = eulerAngles;
		((Component)fishingRod).transform.localPosition = rodPutAwayPosition;
		((Component)alert).gameObject.SetActive(false);
		((Component)bauble).gameObject.SetActive(false);
		fishingLine.color = Palette.ClearWhite;
		yield return null;
	}

	private IEnumerator CoAnimateCaughtFish()
	{
		if (Constants.ShouldPlaySfx() && amClosing == CloseState.None)
		{
			SoundManager.Instance.PlaySoundImmediate(onCatchSfx, loop: false);
		}
		VibrationManager.Vibrate(0.7f, 0.7f, 0.1f, VibrationManager.VibrationFalloff.Linear);
		currentFish.color = Palette.ClearWhite;
		((Component)caughtFish).transform.localPosition = new Vector3(0f, 0f, -0.1f);
		caughtFish.color = Color.white;
		byte b = MyNormTask.Data[numCaughtFish + 1];
		caughtFish.sprite = fishCounterSprites[b];
		((Component)caughtFish).transform.localScale = Vector3.zero;
		yield return Effects.Bloop(0f, ((Component)caughtFish).transform, 1f, 0.75f);
		Vector2 targetPos = Vector2.op_Implicit(((Component)caughtFish).transform.InverseTransformPoint(((Component)fishCounters[numCaughtFish]).transform.position));
		yield return Effects.All(Effects.ArcSlide(0.5f, ((Component)caughtFish).transform, Vector2.op_Implicit(((Component)caughtFish).transform.localPosition), targetPos, 1f), Effects.ScaleIn(((Component)caughtFish).transform, 1f, caughtFishScale, 0.5f));
		SetCaughtFish(numCaughtFish);
		numCaughtFish++;
		yield return null;
	}

	private IEnumerator CoUpdateProgress()
	{
		if (numCaughtFish >= 3)
		{
			MyNormTask.taskStep = 0;
			MyNormTask.MaxStep = 2;
			MyNormTask.NextStep();
			yield return Effects.Wait(0.2f);
			((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
		}
		else
		{
			MyNormTask.NextStep();
			yield return Effects.Wait(0.2f);
			((MonoBehaviour)this).StartCoroutine(CoBeginFishing());
		}
		yield return null;
	}
}
