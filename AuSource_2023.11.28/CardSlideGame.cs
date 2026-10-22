using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;

public class CardSlideGame : Minigame
{
	private enum TaskStages
	{
		Before,
		Animating,
		Inserted,
		After
	}

	private Color gray = new Color(0.45f, 0.45f, 0.45f);

	private Color green = new Color(0f, 0.8f, 0f);

	private TaskStages State;

	private Controller myController = new Controller();

	private FloatRange XRange = new FloatRange(-2.38f, 2.38f);

	public FloatRange AcceptedTime = new FloatRange(0.4f, 0.6f);

	public Collider2D col;

	public SpriteRenderer redLight;

	public SpriteRenderer greenLight;

	public TextMeshPro StatusText;

	public AudioClip AcceptSound;

	public AudioClip DenySound;

	public AudioClip[] CardMove;

	public AudioClip WalletOut;

	public float dragTime;

	private bool moving;

	private TouchpadBehavior touchpad;

	private Vector2 prevStickInput = Vector2.zero;

	private float xPos = -2.38f;

	private bool hadPrev;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardPleaseInsert);
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0892: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0632: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_086d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0753: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_080c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		if (MyNormTask.IsComplete)
		{
			return;
		}
		Vector3 localPosition = ((Component)col).transform.localPosition;
		myController.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 vector = player.GetAxis2DRaw(13, 14);
			float magnitude = vector.magnitude;
			switch (State)
			{
			case TaskStages.Before:
				if (player.GetAnyButtonDown())
				{
					State = TaskStages.Animating;
					((MonoBehaviour)this).StartCoroutine(InsertCard());
				}
				if (touchpad.IsTouching() && touchpad.GetTouchVector().y > 1f)
				{
					moving = false;
					State = TaskStages.Animating;
					((MonoBehaviour)this).StartCoroutine(InsertCard());
				}
				break;
			case TaskStages.Inserted:
				if (magnitude > 0.9f)
				{
					vector = vector.normalized;
					if (hadPrev)
					{
						float num = prevStickInput.AngleSigned(vector);
						if (num > 180f)
						{
							num -= 360f;
						}
						if (num < -180f)
						{
							num += 360f;
						}
						float num2 = Mathf.Abs(num) * 0.025f;
						float x = localPosition.x;
						localPosition.x += num2;
						if (num2 > 0.01f)
						{
							dragTime += Time.deltaTime;
							redLight.color = gray;
							greenLight.color = gray;
							if (!moving)
							{
								moving = true;
								if (Constants.ShouldPlaySfx())
								{
									SoundManager.Instance.PlaySound(CardMove.Random(), loop: false);
								}
							}
						}
						localPosition.x = XRange.Clamp(localPosition.x);
						float num3 = localPosition.x - x;
						float num4 = XRange.ReverseLerp(localPosition.x);
						float num5 = 0.8f * num3;
						VibrationManager.Vibrate(num5 * (1f - num4), num5 * num4, 0.01f);
					}
					else if ((double)XRange.ReverseLerp(localPosition.x) <= 0.05)
					{
						dragTime = 0f;
					}
					prevStickInput = vector;
					hadPrev = true;
					break;
				}
				if (touchpad.IsTouching())
				{
					localPosition.x = XRange.Clamp(touchpad.GetTouchVector().x + xPos);
					hadPrev = true;
					if (!moving)
					{
						moving = true;
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(CardMove.Random(), loop: false);
						}
					}
					if (moving)
					{
						dragTime += Time.deltaTime;
					}
					break;
				}
				if (hadPrev)
				{
					if (XRange.max - localPosition.x < 0.05f)
					{
						if (AcceptedTime.Contains(dragTime))
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(AcceptSound, loop: false);
							}
							State = TaskStages.After;
							((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardAccepted);
							((MonoBehaviour)this).StartCoroutine(PutCardBack());
							if (Object.op_Implicit((Object)(object)MyNormTask))
							{
								MyNormTask.NextStep();
							}
							redLight.color = gray;
							greenLight.color = green;
						}
						else
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(DenySound, loop: false);
							}
							if (AcceptedTime.max < dragTime)
							{
								((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooSlow);
							}
							else
							{
								((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooFast);
							}
							redLight.color = Color.red;
							greenLight.color = gray;
							DestroyableSingleton<AchievementManager>.Instance.OnTaskFailure(TaskType);
						}
					}
					else
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(DenySound, loop: false);
						}
						((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardBadRead);
						redLight.color = Color.red;
						greenLight.color = gray;
						DestroyableSingleton<AchievementManager>.Instance.OnTaskFailure(TaskType);
					}
				}
				localPosition.x = Mathf.Lerp(localPosition.x, XRange.min, Time.deltaTime * 4f);
				hadPrev = false;
				if ((double)XRange.ReverseLerp(localPosition.x) <= 0.05)
				{
					dragTime = 0f;
				}
				break;
			}
		}
		else
		{
			switch (myController.CheckDrag(col))
			{
			case DragState.TouchStart:
				dragTime = 0f;
				break;
			case DragState.Dragging:
			{
				if (State != TaskStages.Inserted)
				{
					break;
				}
				Vector2 val = myController.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
				val.x = XRange.Clamp(val.x);
				if (val.x - localPosition.x > 0.01f)
				{
					dragTime += Time.deltaTime;
					redLight.color = gray;
					greenLight.color = gray;
					if (!moving)
					{
						moving = true;
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(CardMove.Random(), loop: false);
						}
					}
				}
				localPosition.x = val.x;
				break;
			}
			case DragState.NoTouch:
				if (State == TaskStages.Inserted)
				{
					localPosition.x = Mathf.Lerp(localPosition.x, XRange.min, Time.deltaTime * 4f);
				}
				break;
			case DragState.Released:
				moving = false;
				if (State == TaskStages.Before)
				{
					State = TaskStages.Animating;
					((MonoBehaviour)this).StartCoroutine(InsertCard());
				}
				else
				{
					if (State != TaskStages.Inserted)
					{
						break;
					}
					if (XRange.max - localPosition.x < 0.05f)
					{
						if (AcceptedTime.Contains(dragTime))
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(AcceptSound, loop: false);
							}
							State = TaskStages.After;
							((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardAccepted);
							((MonoBehaviour)this).StartCoroutine(PutCardBack());
							if (Object.op_Implicit((Object)(object)MyNormTask))
							{
								MyNormTask.NextStep();
							}
							redLight.color = gray;
							greenLight.color = green;
						}
						else
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(DenySound, loop: false);
							}
							if (AcceptedTime.max < dragTime)
							{
								((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooSlow);
							}
							else
							{
								((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooFast);
							}
							DestroyableSingleton<AchievementManager>.Instance.OnTaskFailure(TaskType);
							redLight.color = Color.red;
							greenLight.color = gray;
						}
					}
					else
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(DenySound, loop: false);
						}
						((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardBadRead);
						redLight.color = Color.red;
						greenLight.color = gray;
						DestroyableSingleton<AchievementManager>.Instance.OnTaskFailure(TaskType);
					}
				}
				break;
			}
		}
		((Component)col).transform.localPosition = localPosition;
	}

	private IEnumerator PutCardBack()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(WalletOut, loop: false);
		}
		Vector3 pos = ((Component)col).transform.localPosition;
		Vector3 targ = new Vector3(-1.11f, -1.9f, pos.z);
		float time = 0f;
		while (true)
		{
			float num = Mathf.Min(1f, time / 0.6f);
			((Component)col).transform.localPosition = Vector3.Lerp(pos, targ, num);
			((Component)col).transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.75f, num);
			if (time > 0.6f)
			{
				break;
			}
			yield return null;
			time += Time.deltaTime;
		}
		((MonoBehaviour)this).StartCoroutine(CoStartClose());
	}

	private IEnumerator InsertCard()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(WalletOut, loop: false);
		}
		Vector3 pos = ((Component)col).transform.localPosition;
		Vector3 targ = new Vector3(XRange.min, 0.75f, pos.z);
		float time = 0f;
		while (true)
		{
			float num = Mathf.Min(1f, time / 0.6f);
			((Component)col).transform.localPosition = Vector3.Lerp(pos, targ, num);
			((Component)col).transform.localScale = Vector3.Lerp(Vector3.one * 0.75f, Vector3.one, num);
			if (time > 0.6f)
			{
				break;
			}
			yield return null;
			time += Time.deltaTime;
		}
		((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardPleaseSwipe);
		greenLight.color = green;
		State = TaskStages.Inserted;
	}

	public CardSlideGame()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
	}
}
