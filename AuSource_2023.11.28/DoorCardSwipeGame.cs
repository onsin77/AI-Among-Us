using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;

public class DoorCardSwipeGame : Minigame, IDoorMinigame
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

	private FloatRange YRange = new FloatRange(-1.77f, 2f);

	public float minAcceptedTime = 0.3f;

	public Collider2D col;

	public SpriteRenderer confirmSymbol;

	public Sprite AcceptSymbol;

	public Sprite RejectSymbol;

	public TextMeshPro StatusText;

	public AudioClip AcceptSound;

	public AudioClip DenySound;

	public AudioClip[] CardMove;

	public AudioClip WalletOut;

	public float dragTime;

	private bool moving;

	private Vector2 prevStickInput = Vector2.zero;

	private bool hadPrev;

	private OpenableDoor MyDoor;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardPleaseInsert);
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
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
						float y = localPosition.y;
						localPosition.y -= num2;
						if (num2 > 0.01f)
						{
							dragTime += Time.deltaTime;
							if (!moving)
							{
								moving = true;
								if (Constants.ShouldPlaySfx())
								{
									SoundManager.Instance.PlaySound(CardMove.Random(), loop: false);
								}
							}
						}
						localPosition.y = YRange.Clamp(localPosition.y);
						float num3 = localPosition.y - y;
						float num4 = YRange.ReverseLerp(localPosition.y);
						float num5 = 0.8f * num3;
						VibrationManager.Vibrate(num5 * (1f - num4), num5 * num4, 0.01f);
					}
					else if ((double)YRange.ReverseLerp(localPosition.y) <= 0.05)
					{
						dragTime = 0f;
					}
					prevStickInput = vector;
					hadPrev = true;
					break;
				}
				if (hadPrev)
				{
					if (localPosition.y - YRange.min < 0.05f && !BoolRange.Next(0.01f))
					{
						if (dragTime > minAcceptedTime)
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(AcceptSound, loop: false);
							}
							State = TaskStages.After;
							((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardAccepted);
							((MonoBehaviour)this).StartCoroutine(PutCardBack());
							ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(MyDoor.Id | 0x40));
							MyDoor.SetDoorway(open: true);
							((MonoBehaviour)this).StartCoroutine(CoStartClose(0.4f));
							confirmSymbol.sprite = AcceptSymbol;
						}
						else
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(DenySound, loop: false);
							}
							((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooFast);
							confirmSymbol.sprite = RejectSymbol;
						}
					}
					else
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(DenySound, loop: false);
						}
						((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardBadRead);
						confirmSymbol.sprite = RejectSymbol;
					}
				}
				localPosition.y = Mathf.Lerp(localPosition.y, YRange.max, Time.deltaTime * 4f);
				hadPrev = false;
				if ((double)YRange.ReverseLerp(localPosition.y) <= 0.05)
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
				val.y = YRange.Clamp(val.y);
				if (localPosition.y - val.y > 0.01f)
				{
					dragTime += Time.deltaTime;
					confirmSymbol.sprite = null;
					if (!moving)
					{
						moving = true;
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(CardMove.Random(), loop: false);
						}
					}
				}
				localPosition.y = val.y;
				break;
			}
			case DragState.NoTouch:
				if (State == TaskStages.Inserted)
				{
					localPosition.y = Mathf.Lerp(localPosition.y, YRange.max, Time.deltaTime * 4f);
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
					if (localPosition.y - YRange.min < 0.05f && !BoolRange.Next(0.01f))
					{
						if (dragTime > minAcceptedTime)
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(AcceptSound, loop: false);
							}
							State = TaskStages.After;
							((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardAccepted);
							((MonoBehaviour)this).StartCoroutine(PutCardBack());
							ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(MyDoor.Id | 0x40));
							MyDoor.SetDoorway(open: true);
							((MonoBehaviour)this).StartCoroutine(CoStartClose(0.4f));
							confirmSymbol.sprite = AcceptSymbol;
						}
						else
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(DenySound, loop: false);
							}
							((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooFast);
							confirmSymbol.sprite = RejectSymbol;
						}
					}
					else
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(DenySound, loop: false);
						}
						((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardBadRead);
						confirmSymbol.sprite = RejectSymbol;
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
		Vector3 localPosition = ((Component)col).transform.localPosition;
		Vector3 dest = new Vector3(0.452f, -1.9f, 0f);
		yield return Effects.All(Effects.Rotate2D(((Component)col).transform, 90f, 0f, 0.4f), Effects.Slide3D(((Component)col).transform, localPosition, dest, 0.4f));
		((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardPleaseInsert);
		State = TaskStages.Before;
	}

	private IEnumerator InsertCard()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(WalletOut, loop: false);
		}
		Vector3 localPosition = ((Component)col).transform.localPosition;
		Vector3 dest = new Vector3(-1.43f, YRange.max, 0f);
		yield return Effects.All(Effects.Rotate2D(((Component)col).transform, 0f, 90f, 0.4f), Effects.Slide3D(((Component)col).transform, localPosition, dest, 0.4f));
		((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardPleaseSwipe);
		State = TaskStages.Inserted;
	}

	public void SetDoor(OpenableDoor door)
	{
		MyDoor = door;
	}

	public DoorCardSwipeGame()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
	}
}
