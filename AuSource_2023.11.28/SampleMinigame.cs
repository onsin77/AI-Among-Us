using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SampleMinigame : Minigame
{
	public enum States : byte
	{
		PrepareSample = 0,
		Complete = 0x10,
		AwaitingStart = 0x20,
		Selection = 0x40,
		Processing = 0x80
	}

	private static StringNames[] ProcessingStrings = new StringNames[4]
	{
		StringNames.TakeBreak,
		StringNames.GrabCoffee,
		StringNames.DontNeedWait,
		StringNames.DoSomethingElse
	};

	private const float PanelMoveDuration = 0.75f;

	private const byte TubeMask = 15;

	public TextMeshPro UpperText;

	public TextMeshPro LowerText;

	public float TimePerStep = 15f;

	public FloatRange platformY = new FloatRange(-3.5f, -0.75f);

	public FloatRange dropperX = new FloatRange(-1.25f, 1.25f);

	public SpriteRenderer CenterPanel;

	public SpriteRenderer Dropper;

	public SpriteRenderer[] Tubes;

	public SpriteRenderer[] Buttons;

	public SpriteRenderer[] LowerButtons;

	public AudioClip ButtonSound;

	public AudioClip PanelMoveSound;

	public AudioClip FailSound;

	public AudioClip[] DropSounds;

	private RandomFill<AudioClip> dropSounds;

	public Transform whichButtonSelector;

	public int whichButtonSelected = 2;

	private float selectMoveCooldown;

	private States State
	{
		get
		{
			return (States)MyNormTask.Data[0];
		}
		set
		{
			MyNormTask.Data[0] = (byte)value;
		}
	}

	private int AnomalyId
	{
		get
		{
			return MyNormTask.Data[1];
		}
		set
		{
			MyNormTask.Data[1] = (byte)value;
		}
	}

	public void Awake()
	{
		dropSounds = new RandomFill<AudioClip>();
		dropSounds.Set(DropSounds);
		((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedHello);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		switch (State)
		{
		case States.Processing:
		{
			for (int j = 0; j < Tubes.Length; j++)
			{
				Tubes[j].color = Color.blue;
			}
			((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(ProcessingStrings.Random());
			SetPlatformBottom();
			break;
		}
		case States.Selection:
		{
			for (int i = 0; i < Tubes.Length; i++)
			{
				Tubes[i].color = Color.blue;
			}
			Tubes[AnomalyId].color = Color.red;
			((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesSelect);
			SetPlatformTop();
			break;
		}
		case States.AwaitingStart:
			((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesPress).PadRight(20, ' ') + "-->";
			SetPlatformTop();
			break;
		case States.PrepareSample:
			((MonoBehaviour)this).StartCoroutine(BringPanelUp(isBeginning: true));
			break;
		}
		SetupInput(disableCursor: true);
	}

	private void SetPlatformBottom()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)CenterPanel).transform.localPosition;
		localPosition.y = platformY.min;
		((Component)CenterPanel).transform.localPosition = localPosition;
	}

	private void SetPlatformTop()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)CenterPanel).transform.localPosition;
		localPosition.y = platformY.max;
		((Component)CenterPanel).transform.localPosition = localPosition;
	}

	public void Update()
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(0);
		switch (State)
		{
		case States.AwaitingStart:
			if (((Component)whichButtonSelector).gameObject.activeSelf)
			{
				((Component)whichButtonSelector).gameObject.SetActive(false);
			}
			if (player.GetButtonDown(11))
			{
				NextStep();
			}
			break;
		case States.Selection:
		{
			if (!((Component)whichButtonSelector).gameObject.activeSelf)
			{
				((Component)whichButtonSelector).gameObject.SetActive(true);
			}
			float axis = player.GetAxis(13);
			if (Mathf.Abs(axis) > 0.5f)
			{
				if (selectMoveCooldown > 0f)
				{
					selectMoveCooldown -= Time.deltaTime;
				}
				else
				{
					int num = (int)Mathf.Sign(axis);
					whichButtonSelected += num;
					whichButtonSelected = Mathf.Clamp(whichButtonSelected, 0, 4);
					Vector3 localPosition = whichButtonSelector.localPosition;
					Vector3 localPosition2 = ((Component)Buttons[whichButtonSelected]).transform.localPosition;
					localPosition.x = localPosition2.x;
					localPosition.y = localPosition2.y;
					((Component)whichButtonSelector).transform.localPosition = localPosition;
					selectMoveCooldown = 0.25f;
				}
			}
			else
			{
				selectMoveCooldown = 0f;
			}
			if (player.GetButtonDown(11))
			{
				ButtonBehavior component = ((Component)Buttons[whichButtonSelected]).GetComponent<ButtonBehavior>();
				if (Object.op_Implicit((Object)(object)component))
				{
					((UnityEvent)component.OnClick).Invoke();
				}
			}
			break;
		}
		case States.Processing:
			if (((Component)whichButtonSelector).gameObject.activeSelf)
			{
				((Component)whichButtonSelector).gameObject.SetActive(false);
			}
			break;
		}
	}

	public void FixedUpdate()
	{
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		if (State == States.Processing)
		{
			if (MyNormTask.TaskTimer <= 0f)
			{
				State = States.Selection;
				((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesSelect);
				((TMP_Text)UpperText).text = "";
				AnomalyId = Tubes.RandomIdx();
				Tubes[AnomalyId].color = Color.red;
				SpriteRenderer[] lowerButtons = LowerButtons;
				for (int i = 0; i < lowerButtons.Length; i++)
				{
					lowerButtons[i].color = Color.white;
				}
				((MonoBehaviour)this).StartCoroutine(BringPanelUp(isBeginning: false));
			}
			else
			{
				((TMP_Text)UpperText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedETA, MyNormTask.TaskTimer);
			}
		}
		else if (State == States.Selection)
		{
			float num = Mathf.Cos(Time.time * 1.5f) - 0.2f;
			Color color = new Color(num, 1f, num, 1f);
			for (int j = 0; j < Buttons.Length; j++)
			{
				Buttons[j].color = color;
			}
		}
		else if (State == States.AwaitingStart)
		{
			float num2 = Mathf.Cos(Time.time * 1.5f) - 0.2f;
			Color color2 = new Color(num2, 1f, num2, 1f);
			SpriteRenderer[] lowerButtons = LowerButtons;
			for (int i = 0; i < lowerButtons.Length; i++)
			{
				lowerButtons[i].color = color2;
			}
		}
	}

	public IEnumerator BringPanelUp(bool isBeginning)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(PanelMoveSound, loop: false);
		}
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		Vector3 pos = ((Component)CenterPanel).transform.localPosition;
		for (float i = 0f; i < 0.75f; i += Time.deltaTime)
		{
			pos.y = platformY.Lerp(i / 0.75f);
			((Component)CenterPanel).transform.localPosition = pos;
			yield return wait;
		}
		if (isBeginning)
		{
			State = States.AwaitingStart;
			((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesPress).PadRight(19, ' ') + " -->";
		}
		pos.y = platformY.max;
		((Component)CenterPanel).transform.localPosition = pos;
	}

	public IEnumerator BringPanelDown()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(PanelMoveSound, loop: false);
		}
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		Vector3 pos = ((Component)CenterPanel).transform.localPosition;
		for (float i = 0f; i < 0.75f; i += Time.deltaTime)
		{
			pos.y = platformY.Lerp(1f - i / 0.75f);
			((Component)CenterPanel).transform.localPosition = pos;
			yield return wait;
		}
		pos.y = platformY.min;
		((Component)CenterPanel).transform.localPosition = pos;
	}

	private IEnumerator DropTube(int id)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(dropSounds.Get(), loop: false);
		}
		Tubes[id].color = Color.blue;
		yield break;
	}

	public void SelectTube(int tubeId)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (State == States.Selection)
		{
			State = States.PrepareSample;
			for (int i = 0; i < Buttons.Length; i++)
			{
				Buttons[i].color = Color.white;
			}
			((MonoBehaviour)this).StartCoroutine(CoSelectTube(AnomalyId, tubeId));
		}
	}

	private IEnumerator CoSelectTube(int correctTube, int selectedTube)
	{
		if (selectedTube != correctTube)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(FailSound, loop: false);
			}
			((TMP_Text)UpperText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BadResult);
			((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BadResult);
			for (int i = 0; i < Buttons.Length; i++)
			{
				Buttons[i].color = Color.red;
			}
			yield return (object)new WaitForSeconds(0.25f);
			for (int j = 0; j < Buttons.Length; j++)
			{
				Buttons[j].color = Color.white;
			}
			yield return (object)new WaitForSeconds(0.25f);
			for (int k = 0; k < Buttons.Length; k++)
			{
				Buttons[k].color = Color.red;
			}
			yield return (object)new WaitForSeconds(0.25f);
			for (int l = 0; l < Buttons.Length; l++)
			{
				Buttons[l].color = Color.white;
			}
			((TMP_Text)UpperText).text = "";
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ButtonSound, loop: false, 0.6f);
			}
			((TMP_Text)UpperText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesThanks);
			MyNormTask.NextStep();
			if (MyNormTask.IsComplete)
			{
				State = States.Complete;
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
		}
		int num = MyNormTask.MaxStep - MyNormTask.taskStep;
		if (num == 0)
		{
			((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesComplete);
		}
		else
		{
			((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.More, num);
		}
		yield return BringPanelDown();
		for (int m = 0; m < Tubes.Length; m++)
		{
			Tubes[m].color = Color.white;
		}
		if (!MyNormTask.IsComplete)
		{
			yield return BringPanelUp(isBeginning: true);
		}
	}

	public void NextStep()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (State == States.AwaitingStart)
		{
			State = States.Processing;
			SpriteRenderer[] lowerButtons = LowerButtons;
			for (int i = 0; i < lowerButtons.Length; i++)
			{
				lowerButtons[i].color = Color.white;
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ButtonSound, loop: false).volume = 0.6f;
			}
			((MonoBehaviour)this).StartCoroutine(CoStartProcessing());
		}
	}

	private IEnumerator CoStartProcessing()
	{
		MyNormTask.TaskTimer = TimePerStep;
		MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
		yield return DropLiquid();
		((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(ProcessingStrings.Random());
		yield return BringPanelDown();
	}

	private IEnumerator DropLiquid()
	{
		((TMP_Text)LowerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesAdding);
		WaitForSeconds dropWait = new WaitForSeconds(0.25f);
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		Vector3 pos = ((Component)Dropper).transform.localPosition;
		yield return dropWait;
		yield return DropTube(0);
		Vector2 vPositionDelta = new Vector2(-0.25f, 0.25f);
		Vector2 vPosition = new Vector2(1f, 0f);
		VibrationManager.Vibrate(vPosition.x * 2.5f, vPosition.y * 2.5f, 0.2f, VibrationManager.VibrationFalloff.None, dropSounds.Get());
		int step = -2;
		while (step < 2)
		{
			float start = (float)step / 2f * 1.25f;
			float xTarg = (float)(step + 1) / 2f * 1.25f;
			for (float i = 0f; i < 0.15f; i += Time.deltaTime)
			{
				pos.x = Mathf.Lerp(start, xTarg, i / 0.15f);
				((Component)Dropper).transform.localPosition = pos;
				yield return wait;
			}
			pos.x = xTarg;
			((Component)Dropper).transform.localPosition = pos;
			yield return dropWait;
			int id = step + 3;
			vPosition += vPositionDelta;
			VibrationManager.Vibrate(vPosition.x * 2.5f, vPosition.y * 2.5f, 0.2f, VibrationManager.VibrationFalloff.None, dropSounds.Get());
			yield return DropTube(id);
			int num = step + 1;
			step = num;
		}
		for (float xTarg = 0f; xTarg < 0.15f; xTarg += Time.deltaTime)
		{
			pos.x = dropperX.Lerp(1f - xTarg / 0.15f);
			((Component)Dropper).transform.localPosition = pos;
			yield return wait;
		}
		pos.x = dropperX.min;
		((Component)Dropper).transform.localPosition = pos;
	}
}
