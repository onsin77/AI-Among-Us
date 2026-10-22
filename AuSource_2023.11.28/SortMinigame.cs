using Rewired;
using UnityEngine;

public class SortMinigame : Minigame
{
	public SortGameObject[] Objects;

	public BoxCollider2D AnimalBox;

	public BoxCollider2D PlantBox;

	public BoxCollider2D MineralBox;

	public AudioClip[] PickUpSounds;

	public AudioClip[] DropSounds;

	private Controller myController = new Controller();

	public Transform selectorObject;

	public SpriteRenderer selectorHand;

	private bool prevHadButton;

	private int currentlyGrabbedObject = -1;

	public void Start()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Objects.Shuffle();
		for (int i = 0; i < Objects.Length; i++)
		{
			SortGameObject sortGameObject = Objects[i];
			((Component)sortGameObject).transform.localPosition = new Vector3(Mathf.Lerp(-2f, 2f, (float)i / ((float)Objects.Length - 1f)), FloatRange.Next(-2.25f, -1.7f), -1f);
			CheckBox(sortGameObject, dropped: true);
		}
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)selectorHand);
		SetupInput(disableCursor: false);
	}

	public void Update()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		myController.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			bool button = ReInput.players.GetPlayer(0).GetButton(11);
			Vector3 position = ((Component)selectorObject).transform.position;
			position.x = VirtualCursor.currentPosition.x;
			position.y = VirtualCursor.currentPosition.y;
			((Component)selectorObject).transform.position = position;
			if (button)
			{
				if (!prevHadButton)
				{
					float num = 0f;
					currentlyGrabbedObject = -1;
					for (int i = 0; i < Objects.Length; i++)
					{
						Vector2 val = Vector2.op_Implicit(((Component)Objects[i]).transform.localPosition - ((Component)selectorObject).transform.localPosition);
						float sqrMagnitude = val.sqrMagnitude;
						if (sqrMagnitude <= 2f && (currentlyGrabbedObject == -1 || sqrMagnitude < num))
						{
							num = sqrMagnitude;
							currentlyGrabbedObject = i;
						}
					}
					if (currentlyGrabbedObject != -1)
					{
						SortGameObject obj = Objects[currentlyGrabbedObject];
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(PickUpSounds.Random(), loop: false);
						}
						((MonoBehaviour)obj).StopAllCoroutines();
						((MonoBehaviour)obj).StartCoroutine(obj.CoShadowRise());
					}
				}
				else if (currentlyGrabbedObject != -1)
				{
					SortGameObject sortGameObject = Objects[currentlyGrabbedObject];
					Vector3 position2 = ((Component)sortGameObject).transform.position;
					position2.x = position.x;
					position2.y = position.y;
					((Component)sortGameObject).transform.position = position2;
					CheckBox(sortGameObject, dropped: false);
				}
				prevHadButton = true;
			}
			else
			{
				if (prevHadButton && currentlyGrabbedObject != -1)
				{
					SortGameObject sortGameObject2 = Objects[currentlyGrabbedObject];
					bool flag = true;
					for (int j = 0; j < Objects.Length; j++)
					{
						SortGameObject obj2 = Objects[j];
						flag &= CheckBox(obj2, dropped: true);
					}
					((MonoBehaviour)sortGameObject2).StopAllCoroutines();
					((MonoBehaviour)sortGameObject2).StartCoroutine(sortGameObject2.CoShadowFall(CheckBox(sortGameObject2, dropped: true), DropSounds.Random()));
					if (flag)
					{
						MyNormTask.NextStep();
						((MonoBehaviour)this).StartCoroutine(CoStartClose());
					}
				}
				prevHadButton = false;
				currentlyGrabbedObject = -1;
			}
			if (currentlyGrabbedObject != -1 && ((Component)selectorObject).gameObject.activeSelf)
			{
				((Component)selectorObject).gameObject.SetActive(false);
			}
			else if (currentlyGrabbedObject == -1 && !((Component)selectorObject).gameObject.activeSelf)
			{
				((Component)selectorObject).gameObject.SetActive(true);
			}
			return;
		}
		if (((Component)selectorObject).gameObject.activeSelf)
		{
			((Component)selectorObject).gameObject.SetActive(false);
		}
		if (currentlyGrabbedObject != -1)
		{
			SortGameObject sortGameObject3 = Objects[currentlyGrabbedObject];
			bool flag2 = true;
			for (int k = 0; k < Objects.Length; k++)
			{
				SortGameObject obj3 = Objects[k];
				flag2 &= CheckBox(obj3, dropped: true);
			}
			((MonoBehaviour)sortGameObject3).StopAllCoroutines();
			((MonoBehaviour)sortGameObject3).StartCoroutine(sortGameObject3.CoShadowFall(CheckBox(sortGameObject3, dropped: true), DropSounds.Random()));
			if (flag2)
			{
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
			currentlyGrabbedObject = -1;
			prevHadButton = false;
		}
		for (int l = 0; l < Objects.Length; l++)
		{
			SortGameObject sortGameObject4 = Objects[l];
			switch (myController.CheckDrag(sortGameObject4.Collider))
			{
			case DragState.TouchStart:
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(PickUpSounds.Random(), loop: false);
				}
				((MonoBehaviour)sortGameObject4).StopAllCoroutines();
				((MonoBehaviour)sortGameObject4).StartCoroutine(sortGameObject4.CoShadowRise());
				break;
			case DragState.Dragging:
			{
				Vector2 dragPosition = myController.DragPosition;
				Vector3 position3 = ((Component)sortGameObject4).transform.position;
				position3.x = dragPosition.x;
				position3.y = dragPosition.y;
				((Component)sortGameObject4).transform.position = position3;
				CheckBox(sortGameObject4, dropped: false);
				break;
			}
			case DragState.Released:
			{
				bool flag3 = true;
				for (int m = 0; m < Objects.Length; m++)
				{
					SortGameObject obj4 = Objects[m];
					flag3 &= CheckBox(obj4, dropped: true);
				}
				((MonoBehaviour)sortGameObject4).StopAllCoroutines();
				((MonoBehaviour)sortGameObject4).StartCoroutine(sortGameObject4.CoShadowFall(CheckBox(sortGameObject4, dropped: true), DropSounds.Random()));
				if (flag3)
				{
					MyNormTask.NextStep();
					((MonoBehaviour)this).StartCoroutine(CoStartClose());
				}
				break;
			}
			}
		}
	}

	private bool CheckBox(SortGameObject obj, bool dropped)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider2D val = null;
		switch (obj.MyType)
		{
		case SortGameObject.ObjType.Animal:
			val = AnimalBox;
			break;
		case SortGameObject.ObjType.Mineral:
			val = MineralBox;
			break;
		case SortGameObject.ObjType.Plant:
			val = PlantBox;
			break;
		}
		if (obj.Collider.IsTouching((Collider2D)(object)val))
		{
			((Renderer)obj.Shadow).material.SetFloat("_Outline", 1f);
			((Renderer)obj.Shadow).material.SetColor("_OutlineColor", new Color(0f, 0.8f, 1f));
			return true;
		}
		if (dropped)
		{
			((Renderer)obj.Shadow).material.SetFloat("_Outline", 1f);
			((Renderer)obj.Shadow).material.SetColor("_OutlineColor", Color.red);
		}
		else
		{
			((Renderer)obj.Shadow).material.SetFloat("_Outline", 0f);
		}
		return false;
	}
}
