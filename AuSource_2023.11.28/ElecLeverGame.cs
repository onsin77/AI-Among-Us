using System.Collections;
using Rewired;
using UnityEngine;

public class ElecLeverGame : Minigame
{
	public Sprite[] NumberIcons;

	public SpriteRenderer NumberImage;

	public SpriteRenderer[] Lights;

	public Sprite LightOn;

	public Sprite LightOff;

	public Collider2D Handle;

	public SpriteRenderer Bars;

	private FloatRange HandleRange = new FloatRange(-2.75f, 0.87f);

	private bool finished;

	private Controller controller = new Controller();

	public AudioClip correctSound;

	public AudioClip incorrectSound;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		NumberImage.sprite = NumberIcons[MyNormTask.Data.IndexOf((byte b) => b == ConsoleId)];
		ResetLights();
		SetupInput(disableCursor: true);
	}

	private void ResetLights()
	{
		int taskStep = MyNormTask.TaskStep;
		for (int i = 0; i < Lights.Length; i++)
		{
			Lights[i].sprite = ((i < taskStep) ? LightOn : LightOff);
		}
	}

	private void SetLights(bool on)
	{
		_ = MyNormTask.Data[0];
		for (int i = 0; i < Lights.Length; i++)
		{
			Lights[i].sprite = (on ? LightOn : LightOff);
		}
	}

	private void Update()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		controller.Update();
		Vector3 localPosition = ((Component)Handle).transform.localPosition;
		float num = HandleRange.ReverseLerp(localPosition.y);
		if (!finished)
		{
			if (Controller.currentTouchType == Controller.TouchType.Joystick)
			{
				Player player = ReInput.players.GetPlayer(0);
				float axisRaw = player.GetAxisRaw(14);
				float axisRaw2 = player.GetAxisRaw(17);
				axisRaw = Mathf.Clamp01(0f - axisRaw);
				axisRaw2 = Mathf.Clamp01(0f - axisRaw2);
				Vector2 val = 0.15f * new Vector2((axisRaw > 0.7f) ? axisRaw : 0f, (axisRaw2 > 0.7f) ? axisRaw2 : 0f);
				int num2 = 0;
				if (axisRaw > 0.7f)
				{
					num2++;
				}
				if (axisRaw2 > 0.7f)
				{
					num2++;
				}
				switch (num2)
				{
				case 0:
				{
					Vector3 localScale4 = ((Component)Bars).transform.localScale;
					localScale4.y = 1f;
					((Component)Handle).transform.localScale = localScale4;
					localPosition.y = Mathf.Lerp(localPosition.y, HandleRange.max, num + Time.deltaTime * 15f);
					break;
				}
				case 1:
				{
					float num4 = Mathf.Max(axisRaw, axisRaw2);
					if (num > 0.9f)
					{
						num -= num4 * Time.deltaTime;
						if (num < 0.9f)
						{
							num = 0.9f;
						}
						else
						{
							VibrationManager.Vibrate(val.x, val.y, 1E-05f);
						}
						localPosition.y = HandleRange.Lerp(num);
						((Component)Handle).transform.localPosition = localPosition;
						Vector3 localScale3 = ((Component)Bars).transform.localScale;
						localScale3.y = HandleRange.ChangeRange(localPosition.y, 0.25f, 1f);
						((Component)Handle).transform.localScale = localScale3;
					}
					break;
				}
				case 2:
				{
					float num3 = (axisRaw + axisRaw2) * 0.5f;
					if (num > 0.7f)
					{
						num -= num3 * Time.deltaTime;
						localPosition.y = HandleRange.Lerp(num);
						VibrationManager.Vibrate(val.x, val.y, 1E-05f);
						((Component)Handle).transform.localPosition = localPosition;
						Vector3 localScale = ((Component)Bars).transform.localScale;
						localScale.y = HandleRange.ChangeRange(localPosition.y, 0.25f, 1f);
						((Component)Handle).transform.localScale = localScale;
					}
					else
					{
						Vector3 localScale2 = ((Component)Bars).transform.localScale;
						localScale2.y = -1f;
						((Component)Handle).transform.localScale = localScale2;
						VibrationManager.Vibrate(0.5f, 0.5f, 0.2f, VibrationManager.VibrationFalloff.Linear);
						localPosition.y = HandleRange.min;
						finished = true;
						((MonoBehaviour)this).StartCoroutine(FinishUp());
					}
					break;
				}
				}
			}
			else
			{
				switch (controller.CheckDrag(Handle))
				{
				case DragState.Dragging:
					if (num > 0.7f)
					{
						Vector2 val2 = controller.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
						float num5 = HandleRange.ReverseLerp(HandleRange.Clamp(val2.y));
						localPosition.y = HandleRange.Lerp(num5 / 2f + 0.5f);
						((Component)Handle).transform.localPosition = localPosition;
						Vector3 localScale6 = ((Component)Bars).transform.localScale;
						localScale6.y = HandleRange.ChangeRange(localPosition.y, 0.25f, 1f);
						((Component)Handle).transform.localScale = localScale6;
					}
					else
					{
						Vector3 localScale7 = ((Component)Bars).transform.localScale;
						localScale7.y = -1f;
						((Component)Handle).transform.localScale = localScale7;
						localPosition.y = HandleRange.min;
					}
					break;
				case DragState.NoTouch:
					if (num > 0.7f)
					{
						Vector3 localScale5 = ((Component)Bars).transform.localScale;
						localScale5.y = 1f;
						((Component)Handle).transform.localScale = localScale5;
						localPosition.y = Mathf.Lerp(localPosition.y, HandleRange.max, num + Time.deltaTime * 15f);
					}
					else
					{
						finished = true;
						((MonoBehaviour)this).StartCoroutine(FinishUp());
					}
					break;
				}
			}
		}
		((Component)Handle).transform.localPosition = localPosition;
		Vector3 localScale8 = ((Component)Bars).transform.localScale;
		localScale8.y = HandleRange.ChangeRange(localPosition.y, -1f, 1f);
		((Component)Bars).transform.localScale = localScale8;
	}

	private IEnumerator FinishUp()
	{
		if (MyNormTask.Data[MyNormTask.taskStep] == ConsoleId)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(correctSound, loop: false);
			}
			Lights[MyNormTask.taskStep].sprite = LightOn;
			MyNormTask?.NextStep();
			yield return Effects.Wait(0.25f);
			Close();
			yield break;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(incorrectSound, loop: false);
		}
		SetLights(on: false);
		yield return Effects.Wait(0.1f);
		Vector3 localScale = ((Component)Bars).transform.localScale;
		localScale.y = 1f;
		((Component)Handle).transform.localScale = localScale;
		Vector3 localPosition = ((Component)Handle).transform.localPosition;
		localPosition.y = HandleRange.max;
		((Component)Handle).transform.localPosition = localPosition;
		Vector3 localScale2 = ((Component)Bars).transform.localScale;
		localScale2.y = HandleRange.ChangeRange(localPosition.y, -1f, 1f);
		((Component)Bars).transform.localScale = localScale2;
		SetLights(on: true);
		yield return Effects.Wait(0.5f);
		ResetLights();
		yield return Effects.Wait(0.25f);
		Close();
	}
}
