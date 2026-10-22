using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PassiveButtonManager : DestroyableSingleton<PassiveButtonManager>
{
	private enum ButtonStates
	{
		Up,
		Down,
		Drag
	}

	private class DepthComparer : IComparer<PassiveUiElement>
	{
		public static readonly DepthComparer Instance = new DepthComparer();

		public int Compare(PassiveUiElement x, PassiveUiElement y)
		{
			if ((Object)(object)x == (Object)null)
			{
				return 1;
			}
			if ((Object)(object)y == (Object)null)
			{
				return -1;
			}
			return x.CachedZ.CompareTo(y.CachedZ);
		}
	}

	public List<PassiveUiElement> Buttons = new List<PassiveUiElement>();

	private List<IFocusHolder> FocusHolders = new List<IFocusHolder>();

	private PassiveUiElement currentOver;

	public Controller controller = new Controller();

	private ButtonStates currentState;

	private Collider2D[] results = new Collider2D[40];

	public void RegisterOne(PassiveUiElement button)
	{
		Buttons.Add(button);
	}

	public void RemoveOne(PassiveUiElement passiveButton)
	{
		Buttons.Remove(passiveButton);
	}

	public void RegisterOne(IFocusHolder focusHolder)
	{
		FocusHolders.Add(focusHolder);
	}

	public void RemoveOne(IFocusHolder focusHolder)
	{
		FocusHolders.Remove(focusHolder);
	}

	public void Update()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isFocused)
		{
			return;
		}
		controller.Update();
		for (int i = 0; i < Buttons.Count; i++)
		{
			if (((Component)Buttons[i]).transform.hasChanged)
			{
				Buttons[i].CachedZ = ((Component)Buttons[i]).transform.position.z;
				((Component)Buttons[i]).transform.hasChanged = false;
			}
		}
		for (int j = 1; j < Buttons.Count; j++)
		{
			if (DepthComparer.Instance.Compare(Buttons[j - 1], Buttons[j]) > 0)
			{
				Buttons.Sort(DepthComparer.Instance);
				break;
			}
		}
		HandleMouseOut();
		for (int k = 0; k < Buttons.Count; k++)
		{
			PassiveUiElement passiveUiElement = Buttons[k];
			if (!Object.op_Implicit((Object)(object)passiveUiElement))
			{
				Buttons.RemoveAt(k);
				k--;
			}
			else
			{
				if (!((Behaviour)passiveUiElement).isActiveAndEnabled)
				{
					continue;
				}
				if (Object.op_Implicit((Object)(object)passiveUiElement.ClickMask))
				{
					Controller.TouchState touch = controller.GetTouch(0);
					Vector2 position = touch.Position;
					if (touch.IsDown && !passiveUiElement.ClickMask.OverlapPoint(position))
					{
						continue;
					}
				}
				for (int l = 0; l < passiveUiElement.Colliders.Length; l++)
				{
					Collider2D col = passiveUiElement.Colliders[l];
					if (!Object.op_Implicit((Object)(object)col) || !((Behaviour)col).isActiveAndEnabled)
					{
						continue;
					}
					HandleMouseOver(passiveUiElement, col);
					switch (controller.CheckDrag(col))
					{
					case DragState.TouchStart:
						if (passiveUiElement.HandleDown)
						{
							passiveUiElement.ReceiveClickDown();
						}
						break;
					case DragState.Holding:
						if (passiveUiElement.HandleRepeat)
						{
							passiveUiElement.ReceiveRepeatDown();
						}
						break;
					case DragState.Dragging:
						if (passiveUiElement.HandleDrag)
						{
							Vector2 dragDelta = controller.DragPosition - controller.DragStartPosition;
							passiveUiElement.ReceiveClickDrag(dragDelta);
							controller.ResetDragPosition();
						}
						else if (passiveUiElement.HandleRepeat)
						{
							passiveUiElement.ReceiveRepeatDown();
						}
						else if (Buttons.Any((PassiveUiElement b2) =>
						{
							//IL_0016: Unknown result type (might be due to invalid IL or missing references)
							//IL_002b: Unknown result type (might be due to invalid IL or missing references)
							return b2.HandleDrag && ((Behaviour)b2).isActiveAndEnabled && ((Component)b2).transform.position.z > ((Component)col).transform.position.z;
						}))
						{
							controller.ClearTouch();
						}
						break;
					case DragState.Released:
						if (passiveUiElement.HandleUp)
						{
							passiveUiElement.ReceiveClickUp();
						}
						if (passiveUiElement.HandleRepeat)
						{
							passiveUiElement.ReleaseButton();
						}
						break;
					}
				}
			}
		}
		if (controller.AnyTouchDown)
		{
			Vector2 touch2 = GetTouch(getDownTouch: false);
			HandleFocus(touch2);
		}
	}

	private void HandleFocus(Vector2 pt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		for (int i = 0; i < FocusHolders.Count; i++)
		{
			IFocusHolder focusHolder = FocusHolders[i];
			if (!Object.op_Implicit((Object)((focusHolder is MonoBehaviour) ? focusHolder : null)))
			{
				FocusHolders.RemoveAt(i);
				i--;
			}
			else
			{
				if (!focusHolder.CheckCollision(pt))
				{
					continue;
				}
				float depth = ((Component)((focusHolder is MonoBehaviour) ? focusHolder : null)).transform.position.z;
				if (Buttons.Any((PassiveUiElement top) =>
				{
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					return ((Behaviour)top).isActiveAndEnabled && ((Component)top).transform.position.z < depth && top.Colliders.Any((Collider2D c) =>
					{
						//IL_0002: Unknown result type (might be due to invalid IL or missing references)
						return c.OverlapPoint(pt);
					});
				}))
				{
					break;
				}
				flag = true;
				focusHolder.GiveFocus();
				for (int num = 0; num < FocusHolders.Count; num++)
				{
					if (num != i)
					{
						FocusHolders[num].LoseFocus();
					}
				}
				break;
			}
		}
		if (!flag)
		{
			for (int num2 = 0; num2 < FocusHolders.Count; num2++)
			{
				FocusHolders[num2].LoseFocus();
			}
		}
	}

	public void LoseFocusForAll()
	{
		for (int i = 0; i < FocusHolders.Count; i++)
		{
			FocusHolders[i].LoseFocus();
		}
	}

	private void HandleMouseOut()
	{
		if (!Object.op_Implicit((Object)(object)currentOver))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < controller.Touches.Length; i++)
		{
			Controller.TouchState pt = controller.GetTouch(i);
			if (pt.active && currentOver.Colliders.Any((Collider2D c) =>
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return c.OverlapPoint(pt.Position);
			}))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			currentOver.ReceiveMouseOut();
			currentOver = null;
		}
	}

	private void HandleMouseOver(PassiveUiElement button, Collider2D col)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (!button.HandleOverOut || (Object)(object)button == (Object)(object)currentOver)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)button.ClickMask))
		{
			Vector2 position = controller.GetTouch(0).Position;
			if (!button.ClickMask.OverlapPoint(position))
			{
				return;
			}
		}
		if (Object.op_Implicit((Object)(object)currentOver) && ((Component)button).transform.position.z > ((Component)currentOver).transform.position.z)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < controller.Touches.Length; i++)
		{
			if (controller.Touches[i].active && col.OverlapPoint(controller.GetTouch(i).Position))
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (Object.op_Implicit((Object)(object)currentOver) && (Object)(object)currentOver != (Object)(object)button)
			{
				currentOver.ReceiveMouseOut();
			}
			currentOver = button;
			currentOver.ReceiveMouseOver();
		}
	}

	private Vector2 GetTouch(bool getDownTouch)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (getDownTouch)
		{
			for (int i = 0; i < controller.Touches.Length; i++)
			{
				if (controller.Touches[i].TouchStart)
				{
					return controller.Touches[i].Position;
				}
			}
		}
		else
		{
			for (int j = 0; j < controller.Touches.Length; j++)
			{
				if (controller.Touches[j].TouchEnd)
				{
					return controller.Touches[j].Position;
				}
			}
		}
		return new Vector2(-5000f, -5000f);
	}
}
