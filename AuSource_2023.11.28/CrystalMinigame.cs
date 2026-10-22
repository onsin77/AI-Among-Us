using System.Linq;
using Rewired;
using UnityEngine;

public class CrystalMinigame : Minigame
{
	public CrystalBehaviour[] CrystalPieces;

	private CrystalBehaviour[] Shuffed;

	public Transform[] CrystalSlots;

	public FloatRange XRange;

	public float TrayY = -2.28f;

	public AudioClip[] PickUpSounds;

	public AudioClip AttachSound;

	private Controller myController = new Controller();

	private bool prevHadStick;

	private bool prevHadButton;

	private int prevSelectedCrystal = -1;

	private int selectedCrystal;

	public void Start()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Shuffed = CrystalPieces.ToArray();
		Shuffed.Shuffle();
		for (int i = 0; i < Shuffed.Length; i++)
		{
			((Component)Shuffed[i]).transform.localPosition = new Vector3(XRange.Lerp(((float)i + 0.5f) / (float)Shuffed.Length), TrayY, ((float)i - (float)Shuffed.Length / 2f) / 100f);
		}
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		myController.Update();
		if (selectedCrystal != prevSelectedCrystal)
		{
			if (prevSelectedCrystal != -1)
			{
				CrystalPieces[prevSelectedCrystal].ReceiveMouseOut();
			}
			prevSelectedCrystal = selectedCrystal;
			if (selectedCrystal != -1)
			{
				CrystalPieces[selectedCrystal].ReceiveMouseOver();
			}
		}
		if (selectedCrystal == -1)
		{
			return;
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (amOpening || amClosing != CloseState.None)
			{
				return;
			}
			Player player = ReInput.players.GetPlayer(0);
			Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
			bool button = player.GetButton(11);
			CrystalBehaviour crystalBehaviour = CrystalPieces[selectedCrystal];
			if (button)
			{
				if (!prevHadButton)
				{
					inputHandler.disableVirtualCursor = false;
					VirtualCursor.instance.SetWorldPosition(Vector2.op_Implicit(((Component)CrystalPieces[selectedCrystal]).transform.position));
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(PickUpSounds.Random(), loop: false);
					}
					((MonoBehaviour)crystalBehaviour).StopAllCoroutines();
				}
				else
				{
					Vector3 position = ((Component)CrystalPieces[selectedCrystal]).transform.position;
					position.x = VirtualCursor.currentPosition.x;
					position.y = VirtualCursor.currentPosition.y;
					((Component)crystalBehaviour).transform.position = position;
				}
				prevHadButton = true;
				return;
			}
			if (prevHadButton)
			{
				inputHandler.disableVirtualCursor = true;
				CheckSolution(selectedCrystal);
				if (crystalBehaviour.CanMove)
				{
					int num = Shuffed.IndexOf(crystalBehaviour);
					((MonoBehaviour)crystalBehaviour).StartCoroutine(Effects.Slide2D(((Component)crystalBehaviour).transform, Vector2.op_Implicit(((Component)crystalBehaviour).transform.localPosition), new Vector2(XRange.Lerp(((float)num + 0.5f) / (float)CrystalPieces.Length), TrayY), 0.15f));
				}
			}
			prevHadButton = false;
			if (axis2DRaw.sqrMagnitude > 0.7f)
			{
				if (!prevHadStick)
				{
					Vector2 normalized = axis2DRaw.normalized;
					int num2 = -1;
					float num3 = 0f;
					float num4 = 0.4f;
					for (int i = 0; i < CrystalPieces.Length; i++)
					{
						Vector2 val = Vector2.op_Implicit(((Component)CrystalPieces[i]).transform.position - ((Component)CrystalPieces[selectedCrystal]).transform.position);
						float magnitude = val.magnitude;
						val /= magnitude;
						float num5 = Vector2.Dot(val, normalized) / Mathf.Lerp(magnitude, 1f, 0.95f);
						if (i != selectedCrystal && num5 > num3 && num5 > 0f && num5 > num4)
						{
							num3 = num5;
							num2 = i;
						}
					}
					if (num2 != -1)
					{
						selectedCrystal = num2;
					}
				}
				prevHadStick = true;
			}
			else
			{
				prevHadStick = false;
			}
			return;
		}
		prevHadButton = false;
		prevHadStick = false;
		if (prevSelectedCrystal != -1)
		{
			CrystalPieces[prevSelectedCrystal].Renderer.color = Color.white;
			CrystalPieces[prevSelectedCrystal].OnMouseOut.Invoke();
			prevSelectedCrystal = -1;
		}
		for (int j = 0; j < CrystalPieces.Length; j++)
		{
			CrystalBehaviour crystalBehaviour2 = CrystalPieces[j];
			switch (myController.CheckDrag((Collider2D)(object)crystalBehaviour2.Collider))
			{
			case DragState.TouchStart:
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(PickUpSounds.Random(), loop: false);
				}
				((MonoBehaviour)crystalBehaviour2).StopAllCoroutines();
				break;
			case DragState.Dragging:
			{
				Vector3 position2 = Vector2.op_Implicit(myController.DragPosition);
				position2.z = ((Component)this).transform.position.z;
				((Component)crystalBehaviour2).transform.position = position2;
				break;
			}
			case DragState.Released:
				CheckSolution(j);
				if (crystalBehaviour2.CanMove)
				{
					int num6 = Shuffed.IndexOf(crystalBehaviour2);
					((MonoBehaviour)crystalBehaviour2).StartCoroutine(Effects.Slide2D(((Component)crystalBehaviour2).transform, Vector2.op_Implicit(((Component)crystalBehaviour2).transform.localPosition), new Vector2(XRange.Lerp(((float)num6 + 0.5f) / (float)CrystalPieces.Length), TrayY), 0.15f));
				}
				break;
			}
		}
	}

	private void CheckSolution(int startAt)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		CrystalBehaviour crystalBehaviour = CrystalPieces[startAt];
		if (!crystalBehaviour.CanMove)
		{
			return;
		}
		Transform val = CrystalSlots[startAt];
		if (((Collider2D)crystalBehaviour.Collider).OverlapPoint(Vector2.op_Implicit(val.position)))
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(AttachSound, loop: false);
			}
			crystalBehaviour.CanMove = false;
			crystalBehaviour.TargetPosition = val;
			for (int i = startAt; i < CrystalPieces.Length; i++)
			{
				CrystalPieces[i].Flash((float)(i - startAt) * 0.1f);
			}
			for (int num = startAt - 1; num >= 0; num--)
			{
				CrystalPieces[num].Flash((float)(startAt - num) * 0.1f);
			}
			int num2 = -1;
			float num3 = 0f;
			for (int j = 0; j < CrystalPieces.Length; j++)
			{
				if (CrystalPieces[j].CanMove && (num2 == -1 || ((Component)CrystalPieces[j]).transform.localPosition.x < num3))
				{
					num2 = j;
					num3 = ((Component)CrystalPieces[j]).transform.localPosition.x;
				}
			}
			if (selectedCrystal != -1)
			{
				CrystalPieces[selectedCrystal].Renderer.color = Color.white;
			}
			if (num2 != -1)
			{
				selectedCrystal = num2;
			}
		}
		if (CrystalPieces.All((CrystalBehaviour c) => !c.CanMove))
		{
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
			selectedCrystal = -1;
		}
	}
}
