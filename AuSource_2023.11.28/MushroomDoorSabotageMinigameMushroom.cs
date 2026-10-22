using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MushroomDoorSabotageMinigameMushroom : MonoBehaviour
{
	public enum State
	{
		None,
		Visible,
		Invisible,
		Appearing,
		Disappearing,
		Whacked
	}

	[SerializeField]
	private PassiveButton button;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private ParticleSystem whackedEffect;

	[SerializeField]
	[Header("Audio")]
	private AudioClip mushroomAppearSFX;

	[SerializeField]
	private AudioClip mushroomDisappearSFX;

	[SerializeField]
	private AudioClip mushroomHitSFX;

	private float timeInState;

	public State CurrentState { get; private set; }

	public bool IsTimeInStateComplete => timeInState <= 0f;

	public event Action<MushroomDoorSabotageMinigameMushroom> OnClick;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		((UnityEvent)button.OnClick).AddListener((UnityAction)OnMushroomClick);
	}

	public void SetState(State state, float maxTimeInState)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentState == State.Whacked)
		{
			return;
		}
		if (CurrentState == State.None)
		{
			CurrentState = state;
			timeInState = maxTimeInState;
			((Behaviour)button).enabled = false;
			((Component)this).transform.localScale = Vector3.zero;
			if (CurrentState == State.Visible)
			{
				((MonoBehaviour)this).StartCoroutine(CoShowMushroom());
			}
			return;
		}
		switch (state)
		{
		case State.Visible:
			((MonoBehaviour)this).StartCoroutine(CoShowMushroom());
			break;
		case State.Invisible:
			((MonoBehaviour)this).StartCoroutine(CoHideMushroom());
			break;
		}
		timeInState = maxTimeInState;
	}

	public void DeductTimeInState(float deltaTime)
	{
		timeInState -= deltaTime;
	}

	private IEnumerator CoShowMushroom()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(mushroomAppearSFX, loop: false);
		}
		CurrentState = State.Appearing;
		yield return Effects.ScaleIn(((Component)this).transform, 0f, 1f, 0.6f);
		((Behaviour)button).enabled = true;
		CurrentState = State.Visible;
	}

	private IEnumerator CoHideMushroom()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(mushroomDisappearSFX, loop: false);
		}
		CurrentState = State.Disappearing;
		yield return Effects.ScaleIn(((Component)this).transform, 1f, 0f, 0.3f);
		((Behaviour)button).enabled = false;
		CurrentState = State.Invisible;
	}

	private void OnMushroomClick()
	{
		if (CurrentState != State.Whacked)
		{
			CurrentState = State.Whacked;
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CoAnimateWhackMushroom());
			OnClick?.Invoke(this);
		}
	}

	private IEnumerator CoAnimateWhackMushroom()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(mushroomHitSFX, loop: false);
		}
		whackedEffect.Play();
		button.SetButtonEnableState(enabled: false);
		((Component)sprite).gameObject.SetActive(true);
		yield return Effects.All(Effects.ScaleIn(((Component)this).transform, ((Component)this).transform.localScale.x, 1.2f, 0.3f), Effects.ColorFade(sprite, Palette.White, Palette.ClearWhite, 0.2f));
	}
}
