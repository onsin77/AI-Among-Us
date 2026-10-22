using System.Collections;
using Rewired;
using UnityEngine;

public class AlignGame : Minigame
{
	private Controller myController = new Controller();

	public FloatRange YRange = new FloatRange(-0.425f, 0.425f);

	public AnimationCurve curve;

	public LineRenderer centerline;

	public LineRenderer[] guidelines;

	public SpriteRenderer engine;

	public Collider2D col;

	private float pulseTimer;

	private bool wasPushingJoystick;

	private float initialY;

	private TouchpadBehavior touchpad;

	public override void Begin(PlayerTask task)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		float v = FromByte(MyNormTask.Data[ConsoleId]);
		Vector3 localPosition = ((Component)col).transform.localPosition;
		localPosition.y = YRange.Lerp(v);
		float num = YRange.ReverseLerp(localPosition.y);
		localPosition.x = curve.Evaluate(num);
		((Component)col).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(20f, -20f, num));
		((Component)engine).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(45f, -45f, num));
		((Component)col).transform.localPosition = localPosition;
		((Renderer)centerline).material.SetColor("_Color", Color.red);
		engine.color = Color.red;
		((Renderer)guidelines[0]).enabled = false;
		((Renderer)guidelines[1]).enabled = false;
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_067d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_066b: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)centerline).material.SetTextureOffset("_MainTex", new Vector2(Time.time, 0f));
		((Renderer)guidelines[0]).material.SetTextureOffset("_MainTex", new Vector2(Time.time, 0f));
		((Renderer)guidelines[1]).material.SetTextureOffset("_MainTex", new Vector2(Time.time, 0f));
		if (amClosing != CloseState.None || (Object.op_Implicit((Object)(object)MyTask) && MyNormTask.IsComplete))
		{
			return;
		}
		Vector3 localPosition = ((Component)col).transform.localPosition;
		Player player = ReInput.players.GetPlayer(0);
		Vector2 val = new Vector2(player.GetAxis(13), player.GetAxis(14));
		if (val.magnitude > 0.01f)
		{
			wasPushingJoystick = true;
			float num = YRange.ReverseLerp(localPosition.y);
			float num2 = Mathf.Clamp01(num + val.y * Time.deltaTime);
			localPosition.y = YRange.Lerp(num2);
			localPosition.x = curve.Evaluate(num2);
			((Component)col).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(20f, -20f, num2));
			((Component)engine).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(45f, -45f, num2));
			MyNormTask.Data[ConsoleId] = ToByte(num2);
			((Renderer)centerline).material.SetColor("_Color", ShouldComplete(MyNormTask.Data[ConsoleId]) ? Color.green : Color.red);
			if (Mathf.Abs(num2 - num) > 0.001f)
			{
				pulseTimer += Time.deltaTime * 25f;
				switch ((int)pulseTimer % 3)
				{
				case 0:
				case 1:
					engine.color = Color.red;
					break;
				case 2:
					engine.color = Color.clear;
					break;
				}
			}
			else
			{
				engine.color = Color.red;
			}
		}
		else if (touchpad.IsTouching())
		{
			wasPushingJoystick = true;
			if (touchpad.IsFirstTouch())
			{
				initialY = localPosition.y;
			}
			float y = touchpad.GetTouchVector().y;
			float num3 = YRange.ReverseLerp(initialY);
			float num4 = Mathf.Clamp01(num3 + y);
			localPosition.y = YRange.Lerp(num4);
			localPosition.x = curve.Evaluate(num4);
			((Component)col).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(20f, -20f, num4));
			((Component)engine).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(45f, -45f, num4));
			MyNormTask.Data[ConsoleId] = ToByte(num4);
			((Renderer)centerline).material.SetColor("_Color", ShouldComplete(MyNormTask.Data[ConsoleId]) ? Color.green : Color.red);
			if (Mathf.Abs(num4 - num3) > 0.001f)
			{
				pulseTimer += Time.deltaTime * 25f;
				switch ((int)pulseTimer % 3)
				{
				case 0:
				case 1:
					engine.color = Color.red;
					break;
				case 2:
					engine.color = Color.clear;
					break;
				}
			}
			else
			{
				engine.color = Color.red;
			}
		}
		else if (wasPushingJoystick)
		{
			wasPushingJoystick = false;
			if (ShouldComplete(MyNormTask.Data[ConsoleId]))
			{
				MyNormTask.NextStep();
				MyNormTask.Data[ConsoleId + 2] = 1;
				((MonoBehaviour)this).StartCoroutine(LockEngine());
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
			else
			{
				engine.color = Color.red;
			}
		}
		myController.Update();
		switch (myController.CheckDrag(col))
		{
		case DragState.TouchStart:
			pulseTimer = 0f;
			break;
		case DragState.Dragging:
		{
			Vector2 val2 = myController.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
			float num5 = YRange.ReverseLerp(localPosition.y);
			localPosition.y = YRange.Clamp(val2.y);
			float num6 = YRange.ReverseLerp(localPosition.y);
			localPosition.x = curve.Evaluate(num6);
			((Component)col).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(20f, -20f, num6));
			((Component)engine).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(45f, -45f, num6));
			MyNormTask.Data[ConsoleId] = ToByte(num6);
			((Renderer)centerline).material.SetColor("_Color", ShouldComplete(MyNormTask.Data[ConsoleId]) ? Color.green : Color.red);
			if (Mathf.Abs(num6 - num5) > 0.001f)
			{
				pulseTimer += Time.deltaTime * 25f;
				switch ((int)pulseTimer % 3)
				{
				case 0:
				case 1:
					engine.color = Color.red;
					break;
				case 2:
					engine.color = Color.clear;
					break;
				}
			}
			else
			{
				engine.color = Color.red;
			}
			break;
		}
		case DragState.Released:
			if (ShouldComplete(MyNormTask.Data[ConsoleId]))
			{
				MyNormTask.NextStep();
				MyNormTask.Data[ConsoleId + 2] = 1;
				((MonoBehaviour)this).StartCoroutine(LockEngine());
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
			else
			{
				engine.color = Color.red;
			}
			break;
		}
		((Component)col).transform.localPosition = localPosition;
	}

	private IEnumerator LockEngine()
	{
		int i = 0;
		while (i < 3)
		{
			((Renderer)guidelines[0]).enabled = true;
			((Renderer)guidelines[1]).enabled = true;
			yield return (object)new WaitForSeconds(0.1f);
			((Renderer)guidelines[0]).enabled = false;
			((Renderer)guidelines[1]).enabled = false;
			yield return (object)new WaitForSeconds(0.1f);
			int num = i + 1;
			i = num;
		}
		Color green = new Color(0f, 0.7f, 0f);
		yield return new WaitForLerp(1f, (float t) =>
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			engine.color = Color.Lerp(Color.white, green, t);
		});
		((Renderer)guidelines[0]).enabled = true;
		((Renderer)guidelines[1]).enabled = true;
	}

	public static float FromByte(byte b)
	{
		return (float)(int)b / 256f;
	}

	public static byte ToByte(float y)
	{
		return (byte)(y * 255f);
	}

	public static bool IsSuccess(byte b)
	{
		return b != 0;
	}

	public static bool ShouldComplete(byte b)
	{
		return (float)Mathf.Abs(b - 128) < 12.75f;
	}
}
