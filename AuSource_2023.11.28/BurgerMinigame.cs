using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

public class BurgerMinigame : Minigame
{
	public BurgerTopping Plate;

	public BurgerTopping[] Toppings;

	public Sprite[] PaperToppings;

	public SpriteRenderer[] PaperSlots;

	private BurgerToppingTypes[] ExpectedToppings = new BurgerToppingTypes[6];

	private Stack<BurgerTopping> burger = new Stack<BurgerTopping>();

	private Controller controller = new Controller();

	public Transform Paper;

	private const float PaperOpenedY = 0f;

	private const float PaperClosedY = 4.8f;

	private bool PaperClosed;

	public SpriteRenderer Shadow;

	public Transform stickCursor;

	public SpriteRenderer handSprite;

	public BoxCollider2D stickSelectionCenterPoint;

	private Vector3 selectionCenterStart;

	private BurgerTopping stickSelectedTopping;

	private bool grabbedTopping;

	private Vector3 internalCursorPos;

	public float cursorLerpSpeed = 5f;

	public float cursorHoverLerpSpeed = 1f;

	private float stackHeight;

	public override void Begin(PlayerTask task)
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		burger.Push(Plate);
		((Renderer)Shadow).enabled = false;
		ExpectedToppings[0] = BurgerToppingTypes.Plate;
		if (BoolRange.Next(0.1f))
		{
			ExpectedToppings[1] = BurgerToppingTypes.Lettuce;
			ExpectedToppings[5] = BurgerToppingTypes.Lettuce;
		}
		else
		{
			ExpectedToppings[1] = BurgerToppingTypes.BottomBun;
			ExpectedToppings[5] = BurgerToppingTypes.TopBun;
		}
		for (int i = 2; i < 5; i++)
		{
			BurgerToppingTypes top = (BurgerToppingTypes)IntRange.Next(2, 6);
			if (ExpectedToppings.Count((BurgerToppingTypes t) => t == top) >= 2)
			{
				i--;
			}
			else
			{
				ExpectedToppings[i] = top;
			}
		}
		if (BoolRange.Next(0.01f))
		{
			BurgerToppingTypes burgerToppingTypes = ExpectedToppings[5];
			ExpectedToppings[5] = ExpectedToppings[4];
			ExpectedToppings[4] = burgerToppingTypes;
		}
		for (int num = 1; num < ExpectedToppings.Length; num++)
		{
			PaperSlots[num - 1].sprite = PaperToppings[(int)ExpectedToppings[num]];
		}
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)handSprite);
		internalCursorPos = stickCursor.localPosition;
		selectionCenterStart = ((Component)stickSelectionCenterPoint).transform.localPosition;
		SetupInput(disableCursor: true);
	}

	private void SelectTopping(BurgerTopping topping)
	{
		if ((Object)(object)stickSelectedTopping != (Object)null)
		{
			((Renderer)((Component)stickSelectedTopping).GetComponent<SpriteRenderer>()).material.SetFloat("_Outline", 0f);
		}
		stickSelectedTopping = topping;
		if ((Object)(object)stickSelectedTopping != (Object)null)
		{
			((Renderer)((Component)stickSelectedTopping).GetComponent<SpriteRenderer>()).material.SetFloat("_Outline", 10f);
		}
	}

	public void Update()
	{
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		inputHandler.disableVirtualCursor = false;
		controller.Update();
		if (PaperClosed)
		{
			BurgerTopping[] toppings;
			if (Controller.currentTouchType == Controller.TouchType.Joystick)
			{
				if (grabbedTopping)
				{
					if (((Component)stickCursor).gameObject.activeSelf)
					{
						((Component)stickCursor).gameObject.SetActive(false);
					}
				}
				else if (!((Component)stickCursor).gameObject.activeSelf)
				{
					((Component)stickCursor).gameObject.SetActive(true);
				}
				if (burger.Count > 0)
				{
					((Component)stickSelectionCenterPoint).transform.localPosition = ((Component)burger.Peek()).transform.localPosition;
				}
				else
				{
					((Component)stickSelectionCenterPoint).transform.localPosition = selectionCenterStart;
				}
				Vector2 axis2DRaw = ReInput.players.GetPlayer(0).GetAxis2DRaw(13, 14);
				bool flag = false;
				if (!grabbedTopping)
				{
					toppings = Toppings;
					foreach (BurgerTopping burgerTopping in toppings)
					{
						if (controller.CheckHover(burgerTopping.Hitbox))
						{
							SelectTopping(burgerTopping);
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					SelectTopping(null);
				}
				Vector3 val = ((Component)stickSelectionCenterPoint).transform.localPosition + axis2DRaw.x * Vector3.right * stickSelectionCenterPoint.size.x * 0.5f + axis2DRaw.y * Vector3.up * stickSelectionCenterPoint.size.y * 0.5f;
				internalCursorPos = Vector3.Lerp(internalCursorPos, val, Time.deltaTime * ((flag && !grabbedTopping) ? cursorHoverLerpSpeed : cursorLerpSpeed));
				stickCursor.localPosition = internalCursorPos;
				Vector2.op_Implicit(stickCursor.position);
				VirtualCursor.instance.SetWorldPosition(Vector2.op_Implicit(stickCursor.position));
			}
			else if (((Component)stickCursor).gameObject.activeSelf)
			{
				((Component)stickCursor).gameObject.SetActive(false);
			}
			grabbedTopping = false;
			toppings = Toppings;
			foreach (BurgerTopping burgerTopping2 in toppings)
			{
				switch (controller.CheckDrag(burgerTopping2.Hitbox))
				{
				case DragState.TouchStart:
				{
					if ((Object)(object)burgerTopping2 == (Object)(object)burger.Peek())
					{
						burger.Pop();
					}
					if (burger.Contains(burgerTopping2))
					{
						controller.ClearTouch();
						break;
					}
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(burgerTopping2.GrabSound, loop: false);
					}
					VibrationManager.Vibrate(0.15f, 0.15f, 0.1f, VibrationManager.VibrationFalloff.Linear);
					stackHeight = burger.Sum((BurgerTopping s) => s.Offset) + burgerTopping2.Offset;
					((Renderer)Shadow).enabled = true;
					Vector3 val2 = Vector2.op_Implicit(controller.DragPosition) - ((Component)this).transform.position;
					val2.z = -1f;
					((Component)burgerTopping2).transform.localPosition = val2;
					((Component)Shadow).transform.localPosition = val2 + new Vector3(0f, 0f - stackHeight, 0.8f);
					break;
				}
				case DragState.Holding:
					grabbedTopping = true;
					break;
				case DragState.Dragging:
				{
					grabbedTopping = true;
					Vector3 val3 = Vector2.op_Implicit(controller.DragPosition) - ((Component)this).transform.position;
					val3.z = -1f;
					((Component)burgerTopping2).transform.localPosition = val3;
					((Component)Shadow).transform.localPosition = val3 + new Vector3(0f, 0f - stackHeight, 0.8f);
					break;
				}
				case DragState.Released:
				{
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(burgerTopping2.DropSound, loop: false);
					}
					VibrationManager.Vibrate(0.35f, 0.35f, 0.1f, VibrationManager.VibrationFalloff.Linear);
					((Renderer)Shadow).enabled = false;
					BurgerTopping burgerTopping3 = burger.Peek();
					if (!burgerTopping3.Hitbox.IsTouching(burgerTopping2.Hitbox))
					{
						break;
					}
					((Component)burgerTopping2).transform.position = ((Component)burgerTopping3).transform.position + new Vector3(0f, burgerTopping2.Offset, -0.001f);
					burger.Push(burgerTopping2);
					if (burger.Count != ExpectedToppings.Length)
					{
						break;
					}
					bool flag2 = true;
					BurgerToppingTypes[] array = burger.Select((BurgerTopping tt) => tt.ToppingType).Reverse().ToArray();
					for (int num = 0; num < array.Length; num++)
					{
						if (array[num] != ExpectedToppings[num])
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						MyNormTask.NextStep();
						((MonoBehaviour)this).StartCoroutine(CoStartClose(0.5f));
					}
					break;
				}
				}
			}
		}
		else if (((Component)stickCursor).gameObject.activeSelf)
		{
			((Component)stickCursor).gameObject.SetActive(false);
		}
		Vector3 localPosition = Paper.localPosition;
		localPosition.y = Mathf.Lerp(localPosition.y, PaperClosed ? 4.8f : 0f, Time.deltaTime * 10f);
		Paper.localPosition = localPosition;
	}

	public void TogglePaper()
	{
		PaperClosed = !PaperClosed;
	}
}
