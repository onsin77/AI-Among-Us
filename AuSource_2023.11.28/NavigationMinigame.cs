using System.Collections;
using Rewired;
using UnityEngine;

public class NavigationMinigame : Minigame
{
	public MeshRenderer TwoAxisImage;

	public SpriteRenderer CrossHairImage;

	public Collider2D hitbox;

	private Controller myController = new Controller();

	private Vector2 crossHair;

	private Vector2 half = new Vector2(0.5f, 0.5f);

	private Vector2 initialPos;

	private TouchpadBehavior touchpad;

	private bool prevHadInput;

	public override void Begin(PlayerTask task)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		crossHair = insideUnitCircle.normalized / 2f * 0.6f;
		float x = crossHair.x;
		Bounds bounds = ((Renderer)TwoAxisImage).bounds;
		float num = x * bounds.size.x;
		float y = crossHair.y;
		bounds = ((Renderer)TwoAxisImage).bounds;
		Vector3 localPosition = new Vector3(num, y * bounds.size.y, -2f);
		((Component)CrossHairImage).transform.localPosition = localPosition;
		((Renderer)TwoAxisImage).material.SetVector("_CrossHair", Vector4.op_Implicit(crossHair + half));
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public void FixedUpdate()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)MyNormTask) && MyNormTask.IsComplete)
		{
			return;
		}
		myController.Update();
		Bounds bounds;
		Vector2 val2;
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 zero = Vector2.zero;
			if (player.GetButton(11))
			{
				if (!prevHadInput)
				{
					inputHandler.disableVirtualCursor = false;
					VirtualCursor.instance.SetWorldPosition(Vector2.op_Implicit(((Component)CrossHairImage).transform.position));
				}
				zero = VirtualCursor.currentPosition;
				Vector2 val = zero;
				Vector3 position = ((Component)TwoAxisImage).transform.position;
				bounds = ((Renderer)TwoAxisImage).bounds;
				Vector2 a = val - Vector2.op_Implicit(position - bounds.size / 2f);
				bounds = ((Renderer)TwoAxisImage).bounds;
				crossHair = a.Div(Vector2.op_Implicit(bounds.size));
				val2 = crossHair - half;
				if (val2.magnitude < 0.45f)
				{
					Vector3 localPosition = Vector2.op_Implicit(zero - Vector2.op_Implicit(((Component)this).transform.position));
					localPosition.z = -2f;
					((Component)CrossHairImage).transform.localPosition = localPosition;
					((Renderer)TwoAxisImage).material.SetVector("_CrossHair", Vector4.op_Implicit(crossHair));
				}
				prevHadInput = true;
				return;
			}
			if (touchpad.IsTouching())
			{
				if (touchpad.IsFirstTouch())
				{
					initialPos = Vector4.op_Implicit(((Renderer)TwoAxisImage).material.GetVector("_CrossHair"));
				}
				crossHair = touchpad.GetTouchVector() + initialPos;
				val2 = crossHair - half;
				if (val2.magnitude < 0.45f)
				{
					((Component)CrossHairImage).transform.localPosition = Vector2.op_Implicit((crossHair - half) * 5f);
					((Renderer)TwoAxisImage).material.SetVector("_CrossHair", Vector4.op_Implicit(crossHair));
				}
				prevHadInput = true;
				return;
			}
			if (prevHadInput)
			{
				inputHandler.disableVirtualCursor = true;
				val2 = crossHair - half;
				if (val2.magnitude < 0.05f)
				{
					((MonoBehaviour)this).StartCoroutine(CompleteGame());
					MyNormTask.NextStep();
				}
			}
			prevHadInput = false;
			return;
		}
		switch (myController.CheckDrag(hitbox))
		{
		case DragState.TouchStart:
		case DragState.Dragging:
		{
			Vector2 dragPosition = myController.DragPosition;
			Vector3 position2 = ((Component)TwoAxisImage).transform.position;
			bounds = ((Renderer)TwoAxisImage).bounds;
			Vector2 a2 = dragPosition - Vector2.op_Implicit(position2 - bounds.size / 2f);
			bounds = ((Renderer)TwoAxisImage).bounds;
			crossHair = a2.Div(Vector2.op_Implicit(bounds.size));
			val2 = crossHair - half;
			if (val2.magnitude < 0.45f)
			{
				Vector3 localPosition2 = Vector2.op_Implicit(dragPosition - Vector2.op_Implicit(((Component)this).transform.position));
				localPosition2.z = -2f;
				((Component)CrossHairImage).transform.localPosition = localPosition2;
				((Renderer)TwoAxisImage).material.SetVector("_CrossHair", Vector4.op_Implicit(crossHair));
			}
			break;
		}
		case DragState.Released:
			val2 = crossHair - half;
			if (val2.magnitude < 0.05f)
			{
				((MonoBehaviour)this).StartCoroutine(CompleteGame());
				MyNormTask.NextStep();
			}
			break;
		case DragState.Holding:
			break;
		}
	}

	private IEnumerator CompleteGame()
	{
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		Color green = new Color(0f, 0.8f, 0f, 1f);
		Color32 yellow = new Color32(byte.MaxValue, (byte)202, (byte)0, byte.MaxValue);
		((Component)CrossHairImage).transform.localPosition = new Vector3(0f, 0f, -2f);
		((Renderer)TwoAxisImage).material.SetVector("_CrossHair", Vector4.op_Implicit(half));
		CrossHairImage.color = Color32.op_Implicit(yellow);
		((Renderer)TwoAxisImage).material.SetColor("_CrossColor", Color32.op_Implicit(yellow));
		yield return wait;
		CrossHairImage.color = Color.white;
		((Renderer)TwoAxisImage).material.SetColor("_CrossColor", Color.white);
		yield return wait;
		CrossHairImage.color = Color32.op_Implicit(yellow);
		((Renderer)TwoAxisImage).material.SetColor("_CrossColor", Color32.op_Implicit(yellow));
		yield return wait;
		CrossHairImage.color = Color.white;
		((Renderer)TwoAxisImage).material.SetColor("_CrossColor", Color.white);
		yield return wait;
		CrossHairImage.color = green;
		((Renderer)TwoAxisImage).material.SetColor("_CrossColor", green);
		yield return CoStartClose();
	}

	public NavigationMinigame()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
	}
}
