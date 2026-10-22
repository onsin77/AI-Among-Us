using UnityEngine;

public class DressUpMinigame : Minigame
{
	public SpriteRenderer DummyHat;

	public SpriteRenderer DummyAccessory;

	public SpriteRenderer DummyClothes;

	public SpriteRenderer ActualHat;

	public SpriteRenderer ActualAccessory;

	public SpriteRenderer ActualClothes;

	public DressUpCosmetic[] buttons;

	public Sprite[] Hats;

	public Sprite[] Accessories;

	public Sprite[] Clothes;

	public Collider2D hatHitbox;

	public Collider2D faceHitbox;

	public Collider2D bodyHitbox;

	public SpriteRenderer draggable;

	public AudioClip hatSound;

	public AudioClip faceSound;

	public AudioClip clothesSound;

	public AudioClip correctSound;

	public AudioClip incorrectSound;

	public AudioClip finishedSound;

	private Controller controller = new Controller();

	public SpriteRenderer grabbyHand;

	public Transform cursorObject;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		int num = Hats.RandomIdx();
		((Component)DummyHat).transform.SetLocalZ((num == 1) ? 1 : (-1));
		DummyHat.sprite = Hats[num];
		DummyAccessory.sprite = Accessories.Random();
		DummyClothes.sprite = Clothes.Random();
		((Renderer)draggable).enabled = false;
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)grabbyHand);
		SetupInput(disableCursor: false);
	}

	public void Update()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		Vector3 position = cursorObject.position;
		position.x = VirtualCursor.currentPosition.x;
		position.y = VirtualCursor.currentPosition.y;
		cursorObject.position = position;
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			if (((Renderer)grabbyHand).enabled == ((Renderer)draggable).enabled)
			{
				((Renderer)grabbyHand).enabled = !((Renderer)draggable).enabled;
			}
		}
		else if (((Renderer)grabbyHand).enabled)
		{
			((Renderer)grabbyHand).enabled = false;
		}
		controller.Update();
		DressUpCosmetic[] array = buttons;
		foreach (DressUpCosmetic dressUpCosmetic in array)
		{
			Vector2 dragPosition = controller.DragPosition;
			Bounds bounds;
			switch (controller.CheckDrag(dressUpCosmetic.Hitbox))
			{
			case DragState.TouchStart:
			{
				((Renderer)draggable).enabled = true;
				draggable.sprite = dressUpCosmetic.Rend.sprite;
				Transform transform = ((Component)draggable).transform;
				Vector3 val3 = Vector2.op_Implicit(controller.DragPosition);
				bounds = draggable.sprite.bounds;
				transform.position = val3 - bounds.center;
				switch (dressUpCosmetic.Slot)
				{
				case CosmeticType.Hat:
				{
					int num = Hats.IndexOf(dressUpCosmetic.Rend.sprite);
					((Component)ActualHat).transform.SetLocalZ((num == 1) ? 1 : (-1));
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(hatSound, loop: false);
					}
					break;
				}
				case CosmeticType.Accessory:
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(faceSound, loop: false);
					}
					break;
				case CosmeticType.Skin:
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(clothesSound, loop: false);
					}
					break;
				}
				break;
			}
			case DragState.Dragging:
			{
				Vector3 val = Vector2.op_Implicit(controller.DragPosition);
				Vector3 val2 = val;
				bounds = draggable.sprite.bounds;
				val = val2 - bounds.center;
				val.z = -5f + ((Component)this).transform.position.z;
				switch (dressUpCosmetic.Slot)
				{
				case CosmeticType.Hat:
					if (hatHitbox.OverlapPoint(controller.DragPosition))
					{
						val = ((Component)ActualHat).transform.position;
					}
					break;
				case CosmeticType.Accessory:
					if (faceHitbox.OverlapPoint(controller.DragPosition))
					{
						val = ((Component)ActualAccessory).transform.position;
					}
					break;
				case CosmeticType.Skin:
					if (bodyHitbox.OverlapPoint(controller.DragPosition))
					{
						val = ((Component)ActualClothes).transform.position;
					}
					break;
				}
				((Component)draggable).transform.position = val;
				break;
			}
			case DragState.Released:
				((Renderer)draggable).enabled = false;
				switch (dressUpCosmetic.Slot)
				{
				case CosmeticType.Hat:
					if (hatHitbox.OverlapPoint(dragPosition))
					{
						SetHat(Hats.IndexOf(dressUpCosmetic.Rend.sprite));
					}
					break;
				case CosmeticType.Accessory:
					if (faceHitbox.OverlapPoint(dragPosition))
					{
						SetAccessory(Accessories.IndexOf(dressUpCosmetic.Rend.sprite));
					}
					break;
				case CosmeticType.Skin:
					if (bodyHitbox.OverlapPoint(dragPosition))
					{
						SetClothes(Clothes.IndexOf(dressUpCosmetic.Rend.sprite));
					}
					break;
				}
				break;
			}
		}
	}

	public void SetHat(int i)
	{
		if (amClosing != CloseState.None || i < 0)
		{
			return;
		}
		((Component)ActualHat).transform.SetLocalZ((i == 1) ? 1 : (-1));
		ActualHat.sprite = Hats[i];
		if ((Object)(object)DummyHat.sprite == (Object)(object)ActualHat.sprite)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(correctSound, loop: false);
			}
		}
		else if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(incorrectSound, loop: false);
		}
		CheckOutfit();
	}

	public void SetAccessory(int i)
	{
		if (amClosing != CloseState.None || i < 0)
		{
			return;
		}
		ActualAccessory.sprite = Accessories[i];
		if ((Object)(object)DummyAccessory.sprite == (Object)(object)ActualAccessory.sprite)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(correctSound, loop: false);
			}
		}
		else if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(incorrectSound, loop: false);
		}
		CheckOutfit();
	}

	public void SetClothes(int i)
	{
		if (amClosing != CloseState.None || i < 0)
		{
			return;
		}
		ActualClothes.sprite = Clothes[i];
		if ((Object)(object)DummyClothes.sprite == (Object)(object)ActualClothes.sprite)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(correctSound, loop: false);
			}
		}
		else if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(incorrectSound, loop: false);
		}
		CheckOutfit();
	}

	private void CheckOutfit()
	{
		if ((Object)(object)DummyHat.sprite == (Object)(object)ActualHat.sprite && (Object)(object)DummyClothes.sprite == (Object)(object)ActualClothes.sprite && (Object)(object)DummyAccessory.sprite == (Object)(object)ActualAccessory.sprite)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(finishedSound, loop: false);
			}
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose(0.7f));
		}
	}
}
