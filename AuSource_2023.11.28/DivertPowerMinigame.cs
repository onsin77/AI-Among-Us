using Rewired;
using UnityEngine;

public class DivertPowerMinigame : Minigame
{
	public SystemTypes[] SliderOrder = new SystemTypes[8]
	{
		SystemTypes.LowerEngine,
		SystemTypes.UpperEngine,
		SystemTypes.Weapons,
		SystemTypes.Shields,
		SystemTypes.Nav,
		SystemTypes.Comms,
		SystemTypes.LifeSupp,
		SystemTypes.Security
	};

	public Collider2D[] Sliders;

	public LineRenderer[] Wires;

	public VerticalGauge[] Gauges;

	private int sliderId;

	public FloatRange SliderY = new FloatRange(-1f, 1f);

	private Controller myController = new Controller();

	public ActionMapGlyphDisplay glyphDisplay;

	private int inputJoystick;

	private bool prevHadInput;

	public override void Begin(PlayerTask task)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		DivertPowerTask powerTask = (DivertPowerTask)task;
		sliderId = SliderOrder.IndexOf((SystemTypes t) => t == powerTask.TargetSystem);
		for (int num = 0; num < Sliders.Length; num++)
		{
			if (num != sliderId)
			{
				((Component)Sliders[num]).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
			}
		}
		SetupInput(disableCursor: true);
		if (sliderId < 4)
		{
			inputJoystick = 14;
			glyphDisplay.actionToDisplayMappedGlyphFor = RewiredConstsEnum.Action.TaskLVertical;
		}
		else
		{
			inputJoystick = 17;
			glyphDisplay.actionToDisplayMappedGlyphFor = RewiredConstsEnum.Action.TaskRVertical;
		}
		glyphDisplay.UpdateGlyphDisplay();
		((Component)glyphDisplay).transform.SetParent(((Component)Sliders[sliderId]).transform, false);
	}

	public void FixedUpdate()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		myController.Update();
		if (sliderId >= 0)
		{
			float axisRaw = ReInput.players.GetPlayer(0).GetAxisRaw(inputJoystick);
			Collider2D val = Sliders[sliderId];
			Vector2 val2 = Vector2.op_Implicit(((Component)val).transform.localPosition);
			if (Mathf.Abs(axisRaw) > 0.01f)
			{
				prevHadInput = true;
				val2.y = SliderY.Clamp(val2.y + axisRaw * Time.deltaTime * 2f);
				((Component)val).transform.localPosition = Vector2.op_Implicit(val2);
			}
			else
			{
				if (prevHadInput && SliderY.max - val2.y < 0.05f)
				{
					MyNormTask.NextStep();
					((MonoBehaviour)this).StartCoroutine(CoStartClose());
					sliderId = -1;
					((Component)val).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
				}
				prevHadInput = false;
			}
		}
		float num = 0f;
		for (int i = 0; i < Sliders.Length; i++)
		{
			num += SliderY.ReverseLerp(((Component)Sliders[i]).transform.localPosition.y) / (float)Sliders.Length;
		}
		for (int j = 0; j < Sliders.Length; j++)
		{
			float num2 = SliderY.ReverseLerp(((Component)Sliders[j]).transform.localPosition.y);
			float num3 = num2 / num / 1.6f;
			Gauges[j].value = num3 + (Mathf.PerlinNoise((float)j, Time.time * 51f) - 0.5f) * 0.04f;
			Color val3 = Color.Lerp(Color.gray, Color.yellow, num2 * num2);
			val3.a = ((!(num3 < 0.1f)) ? 1 : 0);
			Vector2 textureOffset = ((Renderer)Wires[j]).material.GetTextureOffset("_MainTex");
			textureOffset.x -= Time.fixedDeltaTime * 3f * Mathf.Lerp(0.1f, 2f, num3);
			((Renderer)Wires[j]).material.SetTextureOffset("_MainTex", textureOffset);
			((Renderer)Wires[j]).material.SetColor("_Color", val3);
		}
		if (sliderId < 0)
		{
			return;
		}
		Collider2D val4 = Sliders[sliderId];
		Vector2 val5 = Vector2.op_Implicit(((Component)val4).transform.localPosition);
		switch (myController.CheckDrag(val4))
		{
		case DragState.Dragging:
		{
			Vector2 val6 = myController.DragPosition - Vector2.op_Implicit(((Component)val4).transform.parent.position);
			val6.y = SliderY.Clamp(val6.y);
			val5.y = val6.y;
			((Component)val4).transform.localPosition = Vector2.op_Implicit(val5);
			break;
		}
		case DragState.Released:
			if (SliderY.max - val5.y < 0.05f)
			{
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
				sliderId = -1;
				((Component)val4).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
			}
			break;
		}
	}
}
