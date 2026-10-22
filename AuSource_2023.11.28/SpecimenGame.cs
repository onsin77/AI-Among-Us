using Rewired;
using UnityEngine;

public class SpecimenGame : Minigame
{
	public Collider2D[] Specimens;

	public Transform[] Slots;

	private Controller cont = new Controller();

	public AudioClip[] PlaceSounds;

	private SpriteRenderer[] SpecimenSprites;

	public Color highlightColor;

	private bool prevHadStick;

	private bool prevHadButton;

	private int prevSelectedSpecimen = -1;

	private int selectedSpecimen;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: true);
		SpecimenSprites = new SpriteRenderer[Specimens.Length];
		for (int i = 0; i < Specimens.Length; i++)
		{
			SpecimenSprites[i] = ((Component)Specimens[i]).GetComponent<SpriteRenderer>();
		}
	}

	public void Update()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		cont.Update();
		if (selectedSpecimen != prevSelectedSpecimen)
		{
			if (prevSelectedSpecimen != -1)
			{
				((Renderer)SpecimenSprites[prevSelectedSpecimen]).material.SetFloat("_Outline", 0f);
			}
			prevSelectedSpecimen = selectedSpecimen;
			((Renderer)SpecimenSprites[prevSelectedSpecimen]).material.SetFloat("_Outline", 10f);
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (amOpening || amClosing != CloseState.None)
			{
				return;
			}
			Player player = ReInput.players.GetPlayer(0);
			Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
			if (player.GetButton(11))
			{
				if (!prevHadButton)
				{
					inputHandler.disableVirtualCursor = false;
					VirtualCursor.instance.SetWorldPosition(Vector2.op_Implicit(((Component)Specimens[selectedSpecimen]).transform.position));
				}
				else
				{
					Vector3 position = ((Component)Specimens[selectedSpecimen]).transform.position;
					position.x = VirtualCursor.currentPosition.x;
					position.y = VirtualCursor.currentPosition.y;
					if (Vector2.Distance(Vector2.op_Implicit(position), Vector2.op_Implicit(Slots[selectedSpecimen].position)) < 0.25f)
					{
						position = Slots[selectedSpecimen].position;
					}
					((Component)Specimens[selectedSpecimen]).transform.position = position;
				}
				prevHadButton = true;
				return;
			}
			if (prevHadButton)
			{
				inputHandler.disableVirtualCursor = true;
				if (Vector2.Distance(Vector2.op_Implicit(((Component)Specimens[selectedSpecimen]).transform.position), Vector2.op_Implicit(Slots[selectedSpecimen].position)) < 0.25f)
				{
					((Component)Specimens[selectedSpecimen]).transform.position = Slots[selectedSpecimen].position;
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(PlaceSounds.Random(), loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
					}
				}
				CheckForWin();
			}
			prevHadButton = false;
			if (axis2DRaw.sqrMagnitude > 0.7f)
			{
				if (!prevHadStick)
				{
					Vector2 normalized = axis2DRaw.normalized;
					int num = -1;
					float num2 = 0f;
					for (int i = 0; i < Specimens.Length; i++)
					{
						Vector2 val = Vector2.op_Implicit(((Component)Specimens[i]).transform.position - ((Component)Specimens[selectedSpecimen]).transform.position);
						float magnitude = val.magnitude;
						val /= magnitude;
						float num3 = Vector2.Dot(val, normalized) / Mathf.Lerp(magnitude, 1f, 0.95f);
						if (i != selectedSpecimen && (num == -1 || (num3 > num2 && num3 > 0f)))
						{
							num2 = num3;
							num = i;
						}
					}
					if (num != -1)
					{
						selectedSpecimen = num;
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
		if (prevSelectedSpecimen != -1)
		{
			((Renderer)SpecimenSprites[prevSelectedSpecimen]).material.SetFloat("_Outline", 0f);
			prevSelectedSpecimen = -1;
		}
		for (int j = 0; j < Specimens.Length; j++)
		{
			Collider2D val2 = Specimens[j];
			switch (cont.CheckDrag(val2))
			{
			case DragState.Dragging:
			{
				Vector3 localPosition = Vector2.op_Implicit(cont.DragPosition) - ((Component)this).transform.position;
				localPosition.z = 0f;
				((Component)val2).transform.localPosition = localPosition;
				if (Vector2.Distance(Vector2.op_Implicit(((Component)val2).transform.position), Vector2.op_Implicit(Slots[j].position)) < 0.25f)
				{
					((Component)val2).transform.position = Slots[j].position;
				}
				break;
			}
			case DragState.Released:
				if (Vector2.Distance(Vector2.op_Implicit(((Component)val2).transform.position), Vector2.op_Implicit(Slots[j].position)) < 0.25f)
				{
					((Component)val2).transform.position = Slots[j].position;
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(PlaceSounds.Random(), loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
					}
				}
				CheckForWin();
				break;
			}
		}
	}

	private void CheckForWin()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		for (int i = 0; i < Specimens.Length; i++)
		{
			if (Vector2.Distance(Vector2.op_Implicit(((Component)Specimens[i]).transform.position), Vector2.op_Implicit(Slots[i].position)) > 0.25f)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (prevSelectedSpecimen != -1)
			{
				((Renderer)SpecimenSprites[prevSelectedSpecimen]).material.SetFloat("_Outline", 0f);
			}
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
	}
}
