using System.Collections;
using System.Linq;
using Rewired;
using UnityEngine;

public class CookFishMinigame : Minigame
{
	private enum State
	{
		NotCooking,
		CookingSide1,
		CookingSide2
	}

	[SerializeField]
	private float cookingTimeSeconds;

	[SerializeField]
	private CookableFish[] cookableFishes;

	[SerializeField]
	private FungleTimer timer;

	[SerializeField]
	private GameObject taskArrow;

	private Player gamepad;

	private bool checkForTimerDone;

	private int controllerFishIndex;

	[SerializeField]
	[Header("Audio")]
	private AudioClip onTimerDoneSfx;

	private State CurrentState
	{
		get
		{
			return (State)MyNormTask.Data[0];
		}
		set
		{
			MyNormTask.Data[0] = (byte)value;
		}
	}

	private void Update()
	{
		CheckControllerInput();
		if (!MyNormTask.IsComplete && MyNormTask.Data.Length >= 1 && checkForTimerDone)
		{
			if (MyNormTask.TaskTimer <= 0f)
			{
				VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
				timer.SetDone();
			}
			else
			{
				timer.SetValue(MyNormTask.TaskTimer);
			}
			CheckIfCookingTimerComplete();
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: true);
		controllerFishIndex = 0;
		ControllerSelectFish();
		gamepad = ReInput.players.GetPlayer(0);
		if (MyNormTask.Data.Length < 1)
		{
			MyNormTask.Data = new byte[1];
		}
		taskArrow.SetActive(false);
		SetupInitialFishStates();
	}

	private void ControllerSelectFish()
	{
		for (int i = 0; i < cookableFishes.Length; i++)
		{
			cookableFishes[i].SetControllerSelectedVisual(i == controllerFishIndex);
		}
	}

	private void CheckControllerInput()
	{
		if (gamepad.GetButtonDown(11))
		{
			cookableFishes[controllerFishIndex].ClickFish();
			return;
		}
		bool flag = false;
		if (gamepad.GetButtonDown(13))
		{
			controllerFishIndex++;
			flag = true;
		}
		if (gamepad.GetNegativeButtonDown(13))
		{
			controllerFishIndex--;
			flag = true;
		}
		if (flag)
		{
			controllerFishIndex = Mathf.Clamp(controllerFishIndex, 0, cookableFishes.Length - 1);
			ControllerSelectFish();
		}
	}

	private void SetupInitialFishStates()
	{
		switch (CurrentState)
		{
		case State.NotCooking:
		{
			timer.Hide();
			CookableFish[] array = cookableFishes;
			foreach (CookableFish obj3 in array)
			{
				obj3.SetState(CookableFish.State.FishNotPlaced);
				obj3.SetAdditionalOnClick(CheckIfAllFishPlaced);
			}
			taskArrow.SetActive(false);
			break;
		}
		case State.CookingSide1:
			timer.Show();
			if (MyNormTask.TaskTimer <= 0f)
			{
				timer.SetDone();
				CookableFish[] array = cookableFishes;
				foreach (CookableFish obj4 in array)
				{
					obj4.SetState(CookableFish.State.FishPlaced);
					obj4.SetAdditionalOnClick(CheckIfAllFishCookedOnSide1);
				}
				taskArrow.SetActive(true);
			}
			else
			{
				checkForTimerDone = true;
				CookableFish[] array = cookableFishes;
				foreach (CookableFish obj5 in array)
				{
					obj5.SetState(CookableFish.State.FishPlaced);
					obj5.BlockClick();
				}
				taskArrow.SetActive(false);
			}
			break;
		case State.CookingSide2:
			timer.Show();
			if (MyNormTask.TaskTimer <= 0f)
			{
				timer.SetDone();
				CookableFish[] array = cookableFishes;
				foreach (CookableFish obj in array)
				{
					obj.SetState(CookableFish.State.OtherSideCooking);
					obj.SetAdditionalOnClick(CheckIfAllFishCookedOnSide2);
				}
				taskArrow.SetActive(true);
			}
			else
			{
				checkForTimerDone = true;
				CookableFish[] array = cookableFishes;
				foreach (CookableFish obj2 in array)
				{
					obj2.SetState(CookableFish.State.OtherSideCooking);
					obj2.BlockClick();
				}
				taskArrow.SetActive(false);
			}
			break;
		}
	}

	private void CheckIfCookingTimerComplete()
	{
		switch (CurrentState)
		{
		case State.CookingSide1:
			if (MyNormTask.TaskTimer <= 0f)
			{
				CookableFish[] array = cookableFishes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetAdditionalOnClick(CheckIfAllFishCookedOnSide1);
				}
				taskArrow.SetActive(true);
				checkForTimerDone = false;
				timer.Shake();
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySoundImmediate(onTimerDoneSfx, loop: false);
				}
			}
			break;
		case State.CookingSide2:
			if (MyNormTask.TaskTimer <= 0f)
			{
				CookableFish[] array = cookableFishes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetAdditionalOnClick(CheckIfAllFishCookedOnSide2);
				}
				taskArrow.SetActive(true);
				checkForTimerDone = false;
				timer.Shake();
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySoundImmediate(onTimerDoneSfx, loop: false);
				}
			}
			break;
		}
	}

	private void CheckIfAllFishPlaced()
	{
		if (!cookableFishes.Any((CookableFish fish) => fish.CurrentState != CookableFish.State.FishPlaced))
		{
			MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
			MyNormTask.TaskTimer = cookingTimeSeconds;
			CurrentState = State.CookingSide1;
			timer.Show();
			timer.SetValue(MyNormTask.TaskTimer);
			checkForTimerDone = true;
			BlockClick();
		}
	}

	private void CheckIfAllFishCookedOnSide1()
	{
		taskArrow.SetActive(false);
		if (!cookableFishes.Any((CookableFish fish) => fish.CurrentState != CookableFish.State.OtherSideCooking))
		{
			MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
			MyNormTask.TaskTimer = cookingTimeSeconds;
			CurrentState = State.CookingSide2;
			timer.Show();
			timer.SetValue(MyNormTask.TaskTimer);
			checkForTimerDone = true;
			BlockClick();
		}
	}

	private void CheckIfAllFishCookedOnSide2()
	{
		taskArrow.SetActive(false);
		if (!cookableFishes.Any((CookableFish fish) => fish.CurrentState != CookableFish.State.Finished))
		{
			MyNormTask.NextStep();
			timer.Hide();
			BlockClick();
			((MonoBehaviour)this).StartCoroutine(CoWaitBeforeClosing());
		}
	}

	private void BlockClick()
	{
		CookableFish[] array = cookableFishes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].BlockClick();
		}
	}

	private IEnumerator CoWaitBeforeClosing()
	{
		yield return Effects.Wait(0.75f);
		((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
	}
}
