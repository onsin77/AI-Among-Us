using UnityEngine;

public class StowArms : Minigame
{
	public GameObject GunContent;

	public GameObject RifleContent;

	public Transform selectorObject;

	public GameObject[] selectorSubobjects;

	public SpriteRenderer[] handSprites;

	public AudioClip pickupGun;

	public AudioClip placeGun;

	public Collider2D[] GunColliders;

	public DragSlot[] GunsSlots;

	public AudioClip pickupRifle;

	public AudioClip placeRifle;

	public Collider2D[] RifleColliders;

	public DragSlot[] RifleSlots;

	private Controller cont = new Controller();

	private Collider2D currentGrabbedObject;

	private Vector3 grabOffset;

	private AudioClip PickupSound
	{
		get
		{
			if (ConsoleId != 1)
			{
				return pickupRifle;
			}
			return pickupGun;
		}
	}

	private AudioClip PlaceSound
	{
		get
		{
			if (ConsoleId != 1)
			{
				return placeRifle;
			}
			return placeGun;
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		if (ConsoleId == 1)
		{
			GunContent.SetActive(true);
			RifleContent.SetActive(false);
		}
		else
		{
			GunContent.SetActive(false);
			RifleContent.SetActive(true);
		}
		SpriteRenderer[] array = handSprites;
		foreach (SpriteRenderer playerMaterialColors in array)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
		SetupInput(disableCursor: false);
	}

	public void Update()
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		cont.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (!((Component)selectorObject).gameObject.activeSelf)
			{
				((Component)selectorObject).gameObject.SetActive(true);
			}
		}
		else if (((Component)selectorObject).gameObject.activeSelf)
		{
			((Component)selectorObject).gameObject.SetActive(false);
		}
		if (ConsoleId == 1)
		{
			DoUpdate(GunColliders, GunsSlots);
		}
		else
		{
			DoUpdate(RifleColliders, RifleSlots);
		}
		if (Object.op_Implicit((Object)(object)currentGrabbedObject))
		{
			selectorObject.position = ((Component)currentGrabbedObject).transform.position;
			selectorObject.SetLocalZ(0f);
			ValidateSelectorActive(selectorSubobjects[0], shouldBeActive: false);
			ValidateSelectorActive(selectorSubobjects[1], ConsoleId == 1);
			ValidateSelectorActive(selectorSubobjects[2], ConsoleId != 1);
		}
		else
		{
			selectorObject.position = Vector2.op_Implicit(VirtualCursor.currentPosition);
			selectorObject.SetLocalZ(0f);
			ValidateSelectorActive(selectorSubobjects[0], shouldBeActive: true);
			ValidateSelectorActive(selectorSubobjects[1], shouldBeActive: false);
			ValidateSelectorActive(selectorSubobjects[2], shouldBeActive: false);
		}
		static void ValidateSelectorActive(GameObject selector, bool shouldBeActive)
		{
			if (selector.gameObject.activeSelf != shouldBeActive)
			{
				selector.gameObject.SetActive(shouldBeActive);
			}
		}
	}

	private void DoUpdate(Collider2D[] colliders, DragSlot[] slots)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		currentGrabbedObject = null;
		for (int i = 0; i < colliders.Length; i++)
		{
			Collider2D val = colliders[i];
			DragSlot dragSlot = slots[i];
			if ((Object)(object)dragSlot.Occupant == (Object)(object)val)
			{
				continue;
			}
			switch (cont.CheckDrag(val))
			{
			case DragState.TouchStart:
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(PickupSound, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
				}
				currentGrabbedObject = val;
				grabOffset = Vector2.op_Implicit(Vector2.op_Implicit(((Component)val).transform.position) - cont.DragStartPosition);
				break;
			case DragState.Holding:
			case DragState.Dragging:
			{
				currentGrabbedObject = val;
				Vector3 position = ((Component)this).transform.position;
				Vector3 position2 = Vector2.op_Implicit(cont.DragPosition) + grabOffset;
				position2.z = position.z;
				((Component)val).transform.position = position2;
				if (Vector2.Distance(Vector2.op_Implicit(((Component)val).transform.position), Vector2.op_Implicit(dragSlot.TargetPosition)) < 0.25f && !Object.op_Implicit((Object)(object)dragSlot.Occupant))
				{
					((Component)val).transform.position = dragSlot.TargetPosition;
				}
				break;
			}
			case DragState.Released:
				if (Vector2.Distance(Vector2.op_Implicit(((Component)val).transform.position), Vector2.op_Implicit(dragSlot.TargetPosition)) < 0.25f && !Object.op_Implicit((Object)(object)dragSlot.Occupant))
				{
					((Component)val).transform.position = dragSlot.TargetPosition;
					dragSlot.Occupant = (Behaviour)(object)val;
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(PlaceSound, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
					}
				}
				CheckForWin(colliders, slots);
				break;
			}
		}
	}

	private void CheckForWin(Collider2D[] colliders, DragSlot[] slots)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		for (int i = 0; i < colliders.Length; i++)
		{
			if (Vector2.Distance(Vector2.op_Implicit(((Component)colliders[i]).transform.position), Vector2.op_Implicit(slots[i].TargetPosition)) > 0.25f)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
	}
}
