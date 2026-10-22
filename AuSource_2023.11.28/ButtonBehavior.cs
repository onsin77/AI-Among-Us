using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonBehavior : UiElement
{
	public bool OnUp = true;

	public bool OnDown;

	public bool Repeat;

	public ButtonClickedEvent OnClick = new ButtonClickedEvent();

	private Controller myController = new Controller();

	private Collider2D[] colliders;

	private float downTime;

	public SpriteRenderer spriteRenderer;

	private bool checkedClickEvent;

	public void OnEnable()
	{
		colliders = ((Component)this).GetComponents<Collider2D>();
		myController.Reset();
		spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		if (checkedClickEvent)
		{
			return;
		}
		checkedClickEvent = true;
		int persistentEventCount = ((UnityEventBase)OnClick).GetPersistentEventCount();
		CloseButtonConsoleBehaviour component = ((Component)this).GetComponent<CloseButtonConsoleBehaviour>();
		string text = ((Object)((Component)this).gameObject).name.ToLower();
		if (Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		for (int i = 0; i < persistentEventCount; i++)
		{
			string persistentMethodName = ((UnityEventBase)OnClick).GetPersistentMethodName(i);
			if (text.Contains("close") && persistentMethodName.ToLower().Contains("close"))
			{
				((Component)this).gameObject.AddComponent<CloseButtonConsoleBehaviour>();
				break;
			}
		}
	}

	public void Update()
	{
		myController.Update();
		Collider2D[] array = colliders;
		foreach (Collider2D coll in array)
		{
			switch (myController.CheckDrag(coll))
			{
			case DragState.TouchStart:
				if (OnDown)
				{
					((UnityEvent)OnClick).Invoke();
				}
				break;
			case DragState.Released:
				if (OnUp)
				{
					((UnityEvent)OnClick).Invoke();
				}
				break;
			case DragState.Dragging:
				if (Repeat)
				{
					downTime += Time.fixedDeltaTime;
					if (downTime >= 0.3f)
					{
						downTime = 0f;
						((UnityEvent)OnClick).Invoke();
					}
				}
				else
				{
					downTime = 0f;
				}
				break;
			}
		}
	}

	public void ReceiveClick()
	{
		((UnityEvent)OnClick).Invoke();
	}

	public ButtonBehavior()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
	}
}
