using System;
using Rewired;
using UnityEngine;

public class WireMinigame : Minigame
{
	private static readonly Color[] colors = new Color[4]
	{
		Color.red,
		new Color(0.15f, 0.15f, 1f, 1f),
		Color.yellow,
		Color.magenta
	};

	public Sprite[] Symbols;

	public Wire[] LeftNodes;

	public WireNode[] RightNodes;

	public SpriteRenderer[] LeftLights;

	public SpriteRenderer[] RightLights;

	private Controller myController = new Controller();

	private sbyte[] ExpectedWires = new sbyte[4];

	private sbyte[] ActualWires = new sbyte[4];

	public AudioClip[] WireSounds;

	private int prevSelectedWireIndex = -1;

	private int selectedWireIndex;

	private bool prevButtonDown;

	private float inputCooldown;

	public Vector2 controllerWirePos = Vector2.zero;

	private const float controllerWireSpeed = 7f;

	public GameObject[] selectingWireGlyphs;

	public GameObject[] movingWireGlyphs;

	public Transform selectedWireUI;

	private bool TaskIsForThisPanel()
	{
		if (MyNormTask.taskStep < MyNormTask.Data.Length && !MyNormTask.IsComplete)
		{
			return MyNormTask.Data[MyNormTask.taskStep] == ConsoleId;
		}
		return false;
	}

	public override void Begin(PlayerTask task)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		IntRange.FillRandomRange(ExpectedWires);
		for (int i = 0; i < LeftNodes.Length; i++)
		{
			ActualWires[i] = -1;
			int num = ExpectedWires[i];
			Wire wire = LeftNodes[i];
			wire.SetColor(colors[num], Symbols[num]);
			wire.WireId = (sbyte)i;
			RightNodes[i].SetColor(colors[i], Symbols[i]);
			RightNodes[i].WireId = (sbyte)i;
			int num2 = ActualWires[i];
			if (num2 > -1)
			{
				wire.ConnectRight(RightNodes[num2]);
			}
			else
			{
				wire.ResetLine(Vector3.zero, reset: true);
			}
		}
		UpdateLights();
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if (!TaskIsForThisPanel())
		{
			return;
		}
		myController.Update();
		((Component)selectedWireUI).gameObject.SetActive(Controller.currentTouchType == Controller.TouchType.Joystick);
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (!amOpening)
			{
				Vector2.op_Implicit(((Component)this).transform.position);
				Player player = ReInput.players.GetPlayer(0);
				Vector2 val = new Vector2(player.GetAxis(13), player.GetAxis(14));
				bool button = player.GetButton(11);
				if (button != prevButtonDown)
				{
					GameObject[] array = movingWireGlyphs;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetActive(button);
					}
					array = selectingWireGlyphs;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetActive(!button);
					}
					if (button)
					{
						controllerWirePos = Vector2.op_Implicit(((Component)LeftNodes[selectedWireIndex]).transform.position + Vector3.right * 0.2f);
						VibrationManager.Vibrate(0.3f, 0f, 0.05f);
					}
				}
				if (button)
				{
					prevSelectedWireIndex = selectedWireIndex;
					Wire wire = LeftNodes[selectedWireIndex];
					controllerWirePos += val * Time.deltaTime * 7f;
					controllerWirePos.x = Mathf.Min(controllerWirePos.x, ((Component)RightNodes[0]).transform.position.x);
					WireNode wireNode = CheckRightSide(controllerWirePos);
					wireNode = CheckRightSide(controllerWirePos);
					if (Object.op_Implicit((Object)(object)wireNode))
					{
						Vector2 val2 = Vector2.op_Implicit(((Component)wireNode).transform.position);
						ActualWires[wire.WireId] = wireNode.WireId;
						wire.ResetLine(Vector2.op_Implicit(val2));
					}
					else
					{
						ActualWires[wire.WireId] = -1;
						wire.ResetLine(Vector2.op_Implicit(controllerWirePos));
					}
				}
				else
				{
					prevSelectedWireIndex = -1;
					if (prevButtonDown)
					{
						Wire wire2 = LeftNodes[selectedWireIndex];
						if (ActualWires[wire2.WireId] == -1)
						{
							wire2.ResetLine(Vector2.op_Implicit(wire2.BaseWorldPos), reset: true);
						}
						else
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(WireSounds.Random(), loop: false);
							}
							VibrationManager.Vibrate(0f, 0.3f, 0.05f);
						}
						CheckTask();
					}
					else if (Math.Abs(val.y) > 0.5f)
					{
						if (inputCooldown > 0f)
						{
							inputCooldown -= Time.deltaTime;
						}
						else
						{
							int num = selectedWireIndex;
							int num2 = (int)Mathf.Sign(0f - val.y);
							selectedWireIndex = Mathf.Clamp(selectedWireIndex + num2, 0, 3);
							if (selectedWireIndex != num)
							{
								inputCooldown = 0.25f;
								Vector3 localPosition = selectedWireUI.localPosition;
								localPosition.y = ((Component)LeftNodes[selectedWireIndex]).transform.localPosition.y;
								selectedWireUI.localPosition = localPosition;
							}
						}
					}
					else
					{
						inputCooldown = 0f;
					}
				}
				prevButtonDown = button;
			}
		}
		else
		{
			prevButtonDown = false;
			if (prevSelectedWireIndex != -1)
			{
				Wire wire3 = LeftNodes[prevSelectedWireIndex];
				if (ActualWires[wire3.WireId] == -1)
				{
					wire3.ResetLine(Vector2.op_Implicit(wire3.BaseWorldPos), reset: true);
				}
				else if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(WireSounds.Random(), loop: false);
				}
				CheckTask();
				prevSelectedWireIndex = -1;
			}
			Vector2.op_Implicit(((Component)this).transform.position);
			for (int j = 0; j < LeftNodes.Length; j++)
			{
				Wire wire4 = LeftNodes[j];
				switch (myController.CheckDrag(wire4.hitbox))
				{
				case DragState.Dragging:
				{
					Vector2 dragPosition = myController.DragPosition;
					WireNode wireNode2 = CheckRightSide(dragPosition);
					if (Object.op_Implicit((Object)(object)wireNode2))
					{
						dragPosition = Vector2.op_Implicit(((Component)wireNode2).transform.position);
						ActualWires[wire4.WireId] = wireNode2.WireId;
					}
					else
					{
						Vector2 val3 = dragPosition;
						Vector2 baseWorldPos = wire4.BaseWorldPos;
						dragPosition = val3 - baseWorldPos.normalized * 0.05f;
						ActualWires[wire4.WireId] = -1;
					}
					wire4.ResetLine(Vector2.op_Implicit(dragPosition));
					break;
				}
				case DragState.Released:
					if (ActualWires[wire4.WireId] == -1)
					{
						wire4.ResetLine(Vector2.op_Implicit(wire4.BaseWorldPos), reset: true);
					}
					else if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(WireSounds.Random(), loop: false);
					}
					CheckTask();
					break;
				}
			}
		}
		UpdateLights();
	}

	private void UpdateLights()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ActualWires.Length; i++)
		{
			Color yellow = Color.yellow;
			yellow *= 1f - Mathf.PerlinNoise((float)i, Time.time * 35f) * 0.3f;
			yellow.a = 1f;
			if (ActualWires[i] != ExpectedWires[i])
			{
				RightLights[ExpectedWires[i]].color = new Color(0.2f, 0.2f, 0.2f);
			}
			else
			{
				RightLights[ExpectedWires[i]].color = yellow;
			}
			LeftLights[i].color = yellow;
		}
	}

	private WireNode CheckRightSide(Vector2 pos)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < RightNodes.Length; i++)
		{
			WireNode wireNode = RightNodes[i];
			if (wireNode.hitbox.OverlapPoint(pos))
			{
				return wireNode;
			}
		}
		return null;
	}

	private void CheckTask()
	{
		bool flag = true;
		for (int i = 0; i < ActualWires.Length; i++)
		{
			if (ActualWires[i] != ExpectedWires[i])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			MyNormTask.NextStep();
			Close();
		}
	}

	public WireMinigame()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
	}

	static WireMinigame()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
	}
}
