using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CookableFish : MonoBehaviour
{
	public enum State
	{
		FishNotPlaced,
		FishPlaced,
		OtherSideCooking,
		Finished
	}

	[SerializeField]
	private PassiveButton button;

	[SerializeField]
	private SpriteRenderer outline;

	[SerializeField]
	private SpriteRenderer cookingFish;

	[SerializeField]
	private Sprite rawFish;

	[SerializeField]
	private Sprite cookedFish;

	[SerializeField]
	private GameObject glyphActionGO;

	[SerializeField]
	[Header("Audio")]
	private AudioClip onGrillSfx;

	[SerializeField]
	private AudioClip onFlipSfx;

	[SerializeField]
	private AudioClip onFinishCookingSfx;

	private Action additionalOnClick;

	public State CurrentState { get; private set; }

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		((UnityEvent)button.OnClick).AddListener((UnityAction)(() =>
		{
			ClickFish();
		}));
	}

	public void SetState(State newState)
	{
		CurrentState = newState;
		switch (newState)
		{
		case State.FishNotPlaced:
			((Renderer)outline).enabled = true;
			((Component)cookingFish).gameObject.SetActive(false);
			break;
		case State.FishPlaced:
			((Renderer)outline).enabled = false;
			((Component)cookingFish).gameObject.SetActive(true);
			cookingFish.sprite = rawFish;
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
			break;
		case State.OtherSideCooking:
			((Renderer)outline).enabled = false;
			((Component)cookingFish).gameObject.SetActive(true);
			cookingFish.sprite = cookedFish;
			cookingFish.flipX = true;
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
			break;
		case State.Finished:
			((Renderer)outline).enabled = false;
			((Component)cookingFish).gameObject.SetActive(false);
			((Behaviour)button).enabled = false;
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
			break;
		}
	}

	public void SetAdditionalOnClick(Action onClick)
	{
		((Behaviour)button).enabled = true;
		additionalOnClick = onClick;
	}

	public void ClickFish()
	{
		if (!((Behaviour)button).enabled)
		{
			return;
		}
		switch (CurrentState)
		{
		case State.FishNotPlaced:
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(onGrillSfx, loop: false);
			}
			SetState(State.FishPlaced);
			break;
		case State.FishPlaced:
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(onFlipSfx, loop: false);
			}
			((MonoBehaviour)this).StartCoroutine(OnFlipFishSfx());
			SetState(State.OtherSideCooking);
			break;
		case State.OtherSideCooking:
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(onFinishCookingSfx, loop: false);
			}
			SetState(State.Finished);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (additionalOnClick != null)
		{
			additionalOnClick();
		}
		BlockClick();
	}

	private IEnumerator OnFlipFishSfx()
	{
		yield return (object)new WaitForSecondsRealtime(0.1f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(onGrillSfx, loop: false);
		}
	}

	public void SetControllerSelectedVisual(bool selected)
	{
		glyphActionGO.SetActive(selected);
	}

	public void BlockClick()
	{
		((Behaviour)button).enabled = false;
	}
}
