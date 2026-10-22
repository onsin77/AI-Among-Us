using System.Runtime.InteropServices;
using Rewired;
using UnityEngine;

public class CourseMinigame : Minigame
{
	[StructLayout(LayoutKind.Explicit)]
	private struct UIntFloat
	{
		[FieldOffset(0)]
		public float FloatValue;

		[FieldOffset(0)]
		public int IntValue;

		public float GetFloat(byte[] bytes)
		{
			IntValue = bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
			return FloatValue;
		}

		public void GetBytes(float value, byte[] bytes)
		{
			FloatValue = value;
			bytes[0] = (byte)(IntValue & 0xFF);
			bytes[1] = (byte)((IntValue >> 8) & 0xFF);
			bytes[2] = (byte)((IntValue >> 16) & 0xFF);
			bytes[3] = (byte)((IntValue >> 24) & 0xFF);
		}
	}

	public CourseStarBehaviour StarPrefab;

	public CourseStarBehaviour[] Stars;

	public SpriteRenderer DotPrefab;

	public Sprite DotLight;

	public SpriteRenderer[] Dots;

	public Collider2D Ship;

	public CourseStarBehaviour Destination;

	public Vector3[] PathPoints;

	public int NumPoints;

	public FloatRange XRange;

	public FloatRange YRange;

	public LineRenderer Path;

	public Controller myController = new Controller();

	public float lineTimer;

	private UIntFloat Converter;

	public AudioClip SetCourseSound;

	public AudioClip SetCourseLastSound;

	private TouchpadBehavior touchpad;

	private float initialCurVec;

	private float targetCurVec;

	public override void Begin(PlayerTask task)
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		PathPoints = new Vector3[NumPoints];
		Stars = new CourseStarBehaviour[NumPoints];
		Dots = new SpriteRenderer[NumPoints];
		for (int i = 0; i < PathPoints.Length; i++)
		{
			PathPoints[i].x = XRange.Lerp((float)i / ((float)PathPoints.Length - 1f));
			do
			{
				PathPoints[i].y = YRange.Next();
			}
			while (i > 0 && Mathf.Abs(PathPoints[i - 1].y - PathPoints[i].y) < YRange.Width / 4f);
			Dots[i] = Object.Instantiate<SpriteRenderer>(DotPrefab, ((Component)this).transform);
			((Component)Dots[i]).transform.localPosition = PathPoints[i];
			switch (i)
			{
			case 0:
				Dots[i].sprite = DotLight;
				continue;
			case 1:
				((Component)Ship).transform.localPosition = PathPoints[0];
				((Component)Ship).transform.eulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(Vector2.op_Implicit(PathPoints[1] - PathPoints[0])));
				break;
			}
			Stars[i] = Object.Instantiate<CourseStarBehaviour>(StarPrefab, ((Component)this).transform);
			((Component)Stars[i]).transform.localPosition = PathPoints[i];
			if (i == PathPoints.Length - 1)
			{
				((Component)Destination).transform.localPosition = PathPoints[i];
			}
		}
		Path.positionCount = PathPoints.Length;
		Path.SetPositions(PathPoints);
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public void FixedUpdate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		float num = Converter.GetFloat(MyNormTask.Data);
		int num2 = (int)num;
		Vector2 val = Vector2.op_Implicit(PathPoints[num2]);
		myController.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 val2 = new Vector2(player.GetAxis(13), player.GetAxis(14));
			float magnitude = val2.magnitude;
			if (magnitude > 0.1f)
			{
				if (num < (float)(PathPoints.Length - 1))
				{
					Vector2 vector = Vector2.op_Implicit(PathPoints[num2 + 1]) - val;
					Vector2 normalized = vector.normalized;
					Vector2 normalized2 = val2.normalized;
					float num3 = Vector2.Dot(normalized, normalized2);
					num = ((!(num3 > 0.7f)) ? Mathf.Max((float)num2, Mathf.Lerp(num, (float)num2, Time.deltaTime * 5f)) : (num + magnitude * num3 * Time.deltaTime * 1.75f));
					((Component)Ship).transform.eulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(vector));
				}
			}
			else if (touchpad.IsTouching())
			{
				if (num < (float)(PathPoints.Length - 1))
				{
					if (touchpad.IsFirstTouch())
					{
						initialCurVec = num;
						targetCurVec = Mathf.Floor(num) + 1f;
					}
					Vector2 vector2 = Vector2.op_Implicit(PathPoints[num2 + 1]) - val;
					Vector2 touchVector = touchpad.GetTouchVector();
					float num4 = Vector2.Dot(vector2.normalized, touchVector.normalized);
					if (num4 > 0.7f)
					{
						num = (touchVector.x + initialCurVec) * num4;
						if (num > targetCurVec)
						{
							num = (initialCurVec = targetCurVec + 0.01f);
							targetCurVec = Mathf.Floor(num) + 1f;
							touchpad.ResetTouch();
						}
					}
					else
					{
						num = Mathf.Max((float)num2, Mathf.Lerp(num, (float)num2, Time.deltaTime * 5f));
					}
					if (num < initialCurVec)
					{
						num = initialCurVec;
					}
					((Component)Ship).transform.eulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(vector2));
				}
			}
			else
			{
				num = Mathf.Max((float)num2, Mathf.Lerp(num, (float)num2, Time.deltaTime * 5f));
			}
			if (num < (float)(PathPoints.Length - 1))
			{
				float num5 = num - Mathf.Floor(num);
				int num6 = (int)num;
				Vector3 localPosition = Vector2.op_Implicit(Vector2.Lerp(Vector2.op_Implicit(PathPoints[num6]), Vector2.op_Implicit(PathPoints[num6 + 1]), num5));
				localPosition.z = -1f;
				((Component)Ship).transform.localPosition = localPosition;
			}
			else
			{
				Vector3 localPosition2 = PathPoints[PathPoints.Length - 1];
				localPosition2.z = -1f;
				((Component)Ship).transform.localPosition = localPosition2;
			}
		}
		else
		{
			switch (myController.CheckDrag(Ship))
			{
			case DragState.Dragging:
				if (num < (float)(PathPoints.Length - 1))
				{
					Vector2 val5 = Vector2.op_Implicit(PathPoints[num2 + 1]) - val;
					Vector2 val6 = new Vector2(1f, val5.y / val5.x);
					Vector2 val7 = Vector2.op_Implicit(((Component)this).transform.InverseTransformPoint(Vector2.op_Implicit(myController.DragPosition))) - val;
					if (val7.x > 0f)
					{
						Vector2 val8 = val6 * val7.x;
						if (Mathf.Abs(val8.y - val7.y) < 0.5f)
						{
							num = (float)num2 + Mathf.Min(1f, val7.x / val5.x);
							Vector3 localPosition5 = Vector2.op_Implicit(val8 + val);
							localPosition5.z = -1f;
							((Component)Ship).transform.localPosition = localPosition5;
							((Component)Ship).transform.localPosition = localPosition5;
							((Component)Ship).transform.eulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(val5));
						}
						else
						{
							myController.Reset();
						}
					}
				}
				else
				{
					Vector3 localPosition6 = PathPoints[PathPoints.Length - 1];
					localPosition6.z = -1f;
					((Component)Ship).transform.localPosition = localPosition6;
				}
				break;
			case DragState.NoTouch:
				if (num < (float)(PathPoints.Length - 1))
				{
					Vector2 val3 = Vector2.op_Implicit(PathPoints[num2 + 1]) - val;
					Vector2 val4 = new Vector2(1f, val3.y / val3.x);
					num = Mathf.Max((float)num2, Mathf.Lerp(num, (float)num2, Time.deltaTime * 5f));
					Vector3 localPosition3 = Vector2.op_Implicit(val4 * (num - (float)num2) + val);
					localPosition3.z = -1f;
					((Component)Ship).transform.localPosition = localPosition3;
				}
				else
				{
					Vector3 localPosition4 = PathPoints[PathPoints.Length - 1];
					localPosition4.z = -1f;
					((Component)Ship).transform.localPosition = localPosition4;
				}
				break;
			}
		}
		if ((int)num > num2 && Object.op_Implicit((Object)(object)Stars[num2 + 1]))
		{
			Object.Destroy((Object)(object)((Component)Stars[num2 + 1]).gameObject);
			Dots[num2 + 1].sprite = DotLight;
			if (num2 == PathPoints.Length - 2)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(SetCourseLastSound, loop: false).volume = 0.7f;
				}
				Destination.Speed *= 5f;
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
			else if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(SetCourseSound, loop: false).volume = 0.7f;
			}
		}
		Converter.GetBytes(num, MyNormTask.Data);
		SetLineDivision(num);
	}

	private void SetLineDivision(float curVec)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)curVec;
		float num2 = 0f;
		for (int i = 0; (float)i <= curVec && i < PathPoints.Length - 1; i++)
		{
			float num3 = Vector2.Distance(Vector2.op_Implicit(PathPoints[i]), Vector2.op_Implicit(PathPoints[i + 1]));
			if (i == num)
			{
				num3 *= curVec - (float)i;
			}
			num2 += num3;
		}
		lineTimer -= Time.fixedDeltaTime;
		Vector2 val = new Vector2(lineTimer, 0f);
		((Renderer)Path).material.SetTextureOffset("_MainTex", val);
		((Renderer)Path).material.SetTextureOffset("_AltTex", val);
		((Renderer)Path).material.SetFloat("_Perc", num2 + lineTimer / 8f);
	}
}
