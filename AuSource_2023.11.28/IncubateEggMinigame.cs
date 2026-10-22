using System.Collections;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class IncubateEggMinigame : Minigame
{
	private enum State
	{
		Empty,
		EggPlaced,
		EggHatched,
		Complete
	}

	private const int STATE_INDEX = 1;

	[SerializeField]
	private PassiveButton incubatorButton;

	[SerializeField]
	private SpriteRenderer egg;

	[SerializeField]
	private float secondsToHatchEgg = 5f;

	[SerializeField]
	private GameObject incubatorLight;

	[Header("Critter")]
	[SerializeField]
	private SpriteRenderer critter;

	[SerializeField]
	private IncubateEggMinigameEmote happyEmote;

	[SerializeField]
	private IncubateEggMinigameEmote thirstyEmote;

	[SerializeField]
	private IncubateEggMinigameEmote hungryEmote;

	[SerializeField]
	[Header("Food")]
	private PassiveButton foodBowl;

	[SerializeField]
	private GameObject food;

	[SerializeField]
	private PassiveButton waterBottle;

	[SerializeField]
	private GameObject waterFull;

	[Header("Timer")]
	[SerializeField]
	private FungleTimer timer;

	[Header("Glyphs")]
	[SerializeField]
	private GameObject emptyGlyphs;

	[SerializeField]
	private GameObject hatchedGlyphs;

	[SerializeField]
	[Header("Audio")]
	private AudioClip foodPlacedSfx;

	[SerializeField]
	private AudioClip waterPlacedSfx;

	[SerializeField]
	private AudioClip eggPlacedSfx;

	[SerializeField]
	private AudioClip eggHatchSfx;

	[SerializeField]
	private AudioClip timerEndSfx;

	private Player player;

	private float lastEmoteTime;

	private State CurrentState
	{
		get
		{
			return (State)MyNormTask.Data[1];
		}
		set
		{
			MyNormTask.Data[1] = (byte)value;
		}
	}

	private void Update()
	{
		UpdateEmotes();
		if (MyNormTask.IsComplete || MyNormTask.Data == null || MyNormTask.Data.Length <= 1)
		{
			return;
		}
		if (CurrentState == State.Empty)
		{
			if (player.GetButtonDown(11))
			{
				PutEgg();
			}
		}
		else if (CurrentState == State.EggPlaced)
		{
			timer.SetValue(MyNormTask.TaskTimer);
			if (MyNormTask.TaskTimer <= 0f)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySoundImmediate(timerEndSfx, loop: false);
				}
				CurrentState = State.EggHatched;
				((MonoBehaviour)this).StartCoroutine(CoHatchEgg());
			}
		}
		else
		{
			if (CurrentState != State.EggHatched)
			{
				return;
			}
			if (player.GetButtonDown(21))
			{
				FillWaterBottle();
			}
			if (player.GetButtonDown(24))
			{
				FillFoodBowl();
			}
			if (food.activeSelf && waterFull.activeSelf)
			{
				CurrentState = State.Complete;
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(Effects.ActionAfterDelay(0.75f, () =>
				{
					((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
				}));
			}
		}
	}

	private void UpdateEmotes()
	{
		if (food.activeSelf && waterFull.activeSelf && !happyEmote.IsVisible)
		{
			thirstyEmote.SetVisible(visible: false);
			hungryEmote.SetVisible(visible: false);
			happyEmote.SetVisible(visible: true);
		}
		else if (!food.activeSelf && !waterFull.activeSelf)
		{
			if (thirstyEmote.IsVisible == hungryEmote.IsVisible)
			{
				thirstyEmote.SetVisible(visible: false);
				hungryEmote.SetVisible(visible: true);
				lastEmoteTime = Time.time;
			}
			if (Time.time > lastEmoteTime + 3f)
			{
				thirstyEmote.SetVisible(!thirstyEmote.IsVisible);
				hungryEmote.SetVisible(!hungryEmote.IsVisible);
				lastEmoteTime = Time.time;
			}
		}
		else
		{
			hungryEmote.SetVisible(!food.activeSelf);
			thirstyEmote.SetVisible(!waterFull.activeSelf);
		}
	}

	public override void Begin(PlayerTask task)
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected Obj, but got Unknown
		base.Begin(task);
		SetupInput(disableCursor: true);
		player = ReInput.players.GetPlayer(0);
		if (MyNormTask.Data == null || MyNormTask.Data.Length <= 1)
		{
			MyNormTask.Data = new byte[2]
			{
				(byte)((MyNormTask.Data != null) ? MyNormTask.Data[0] : 0),
				0
			};
		}
		((Component)critter).gameObject.SetActive(false);
		food.SetActive(false);
		waterFull.SetActive(false);
		incubatorLight.SetActive(false);
		((Component)egg).gameObject.SetActive(false);
		emptyGlyphs.SetActive(false);
		hatchedGlyphs.SetActive(false);
		UpdateEmotes();
		switch (CurrentState)
		{
		case State.Empty:
			((UnityEvent)incubatorButton.OnClick).AddListener((UnityAction)PutEgg);
			emptyGlyphs.SetActive(true);
			timer.Hide();
			break;
		case State.EggPlaced:
			SetEggPlaced();
			break;
		case State.EggHatched:
			((Component)critter).gameObject.SetActive(true);
			ActivateCritterFoodAndAnimate();
			break;
		}
	}

	private void PutEgg()
	{
		MyNormTask.TaskTimer = secondsToHatchEgg;
		MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
		MyNormTask.ShowTaskTimer = true;
		CurrentState = State.EggPlaced;
		emptyGlyphs.SetActive(false);
		incubatorButton.SetButtonEnableState(enabled: false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(eggPlacedSfx, loop: false);
		}
		SetEggPlaced();
		((MonoBehaviour)this).StartCoroutine(Effects.ActionAfterDelay(0.75f, () =>
		{
			((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
		}));
	}

	private void SetEggPlaced()
	{
		((Component)egg).gameObject.SetActive(true);
		incubatorLight.SetActive(true);
		emptyGlyphs.SetActive(false);
		hatchedGlyphs.SetActive(false);
		timer.Show();
		timer.SetValue(MyNormTask.TaskTimer);
	}

	private IEnumerator CoHatchEgg()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(eggHatchSfx, loop: false);
		}
		VibrationManager.Vibrate(0.7f, 0.7f, 0.1f);
		yield return Effects.ColorFade(egg, Color.white, Palette.ClearWhite, 0.5f);
		((Component)critter).gameObject.SetActive(true);
		yield return Effects.ColorFade(critter, Palette.ClearWhite, Color.white, 0.5f);
		ActivateCritterFoodAndAnimate();
	}

	private void ActivateCritterFoodAndAnimate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected Obj, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected Obj, but got Unknown
		timer.Hide();
		incubatorLight.SetActive(false);
		emptyGlyphs.SetActive(false);
		hatchedGlyphs.SetActive(true);
		UpdateEmotes();
		((UnityEvent)foodBowl.OnClick).AddListener((UnityAction)FillFoodBowl);
		((UnityEvent)waterBottle.OnClick).AddListener((UnityAction)FillWaterBottle);
		((MonoBehaviour)this).StartCoroutine(CoAnimateCritter());
	}

	private void FillFoodBowl()
	{
		if (!food.activeSelf)
		{
			food.SetActive(true);
			foodBowl.SetButtonEnableState(enabled: false);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(foodPlacedSfx, loop: false);
			}
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		}
	}

	private void FillWaterBottle()
	{
		if (!waterFull.activeSelf)
		{
			waterFull.SetActive(true);
			waterBottle.SetButtonEnableState(enabled: false);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(waterPlacedSfx, loop: false);
			}
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		}
	}

	private IEnumerator CoAnimateCritter()
	{
		while (true)
		{
			yield return Effects.Bounce(((Component)critter).transform);
		}
	}
}
