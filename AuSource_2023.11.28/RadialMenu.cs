using System;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RadialMenu : MonoBehaviour
{
	public struct CachedButtonObject
	{
		public TextMeshPro tr;

		public Transform textHandle;

		public RectTransform trRT;

		public GameObject gameObject;

		public PassiveButton button;

		public ButtonRolloverHandler rolloverHandler;

		public bool isNew;

		public PolygonCollider2D buttonCollider;

		public List<Vector2> colliderPoints;

		public int buttonIndex;

		public SpriteRenderer iconSR;

		private Vector3 baseTextPos;

		public Vector3 buttonDir;

		public CachedButtonObject(GameObject go, int bIndex)
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			tr = go.GetComponentInChildren<TextMeshPro>(true);
			textHandle = go.transform.GetChild(0);
			trRT = ((Component)tr).GetComponent<RectTransform>();
			button = go.GetComponent<PassiveButton>();
			iconSR = go.GetComponentInChildren<SpriteRenderer>(true);
			buttonCollider = ((Component)button).GetComponent<PolygonCollider2D>();
			rolloverHandler = go.GetComponent<ButtonRolloverHandler>();
			gameObject = go;
			isNew = true;
			colliderPoints = new List<Vector2>();
			buttonIndex = bIndex;
			baseTextPos = tr.transform.localPosition;
			buttonDir = Vector3.zero;
		}

		public void AddIcon(Sprite iconSprite)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			((Renderer)iconSR).enabled = true;
			iconSR.sprite = iconSprite;
			Vector3 val = textHandle.localPosition;
			float num = Mathf.Abs(val.y);
			float num2 = Mathf.Lerp(0.35f, 0.15f, num);
			val.z = 0f;
			val = val.normalized;
			tr.transform.localPosition = baseTextPos + val * num2;
			((Component)iconSR).transform.localPosition = val * (0f - num2);
		}

		public void ResetIcon()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			((Renderer)iconSR).enabled = false;
			tr.transform.localPosition = baseTextPos;
		}
	}

	public Mesh mesh;

	public GameObject perButtonTemplateObject;

	private MeshFilter mf;

	private MeshRenderer mr;

	private int radialDivisions = -1;

	public float sliceDegreesPerVert = 10f;

	public float textPositionDistance = 0.5f;

	public bool angleText;

	public int testRadialDivs = 5;

	private MaterialPropertyBlock mbp;

	private Controller cont = new Controller();

	public Transform arrowRotateHandle;

	public Transform arrowScaleHandle;

	public Transform inputGlyph;

	public List<CachedButtonObject> cachedButtons = new List<CachedButtonObject>();

	[NonSerialized]
	public Vector3[] buttonCenters;

	[NonSerialized]
	public Vector3[] buttonLeftEdges;

	[NonSerialized]
	public Vector3[] buttonRightEdges;

	private const float startOffset = -(float)Math.PI / 2f;

	public static int instances;

	[HideInInspector]
	public int prevSelectedButton = -1;

	[HideInInspector]
	public int mouseSelectedButton = -1;

	private const int cutoffLength = 12;

	public float horizontalSqueezedWidth;

	private void OnEnable()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected Obj, but got Unknown
		mf = ((Component)this).GetComponent<MeshFilter>();
		mr = ((Component)this).GetComponent<MeshRenderer>();
		mbp = new MaterialPropertyBlock();
		instances++;
	}

	private void OnDisable()
	{
		instances--;
	}

	public void ButtonMouseOver(int which)
	{
		if (Controller.currentTouchType != Controller.TouchType.Joystick)
		{
			mouseSelectedButton = which;
		}
	}

	public void ButtonMouseExit()
	{
		if (Controller.currentTouchType != Controller.TouchType.Joystick)
		{
			mouseSelectedButton = -1;
		}
	}

	private void Update()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		cont.Update();
		Player player = ReInput.players.GetPlayer(0);
		Vector2 val = player.GetAxis2DRaw(13, 14);
		int num = -1;
		if (Controller.currentTouchType != Controller.TouchType.Joystick)
		{
			num = mouseSelectedButton;
			if (((Component)arrowRotateHandle).gameObject.activeSelf)
			{
				((Component)arrowRotateHandle).gameObject.SetActive(false);
			}
		}
		else
		{
			mouseSelectedButton = -1;
			if (!((Component)arrowRotateHandle).gameObject.activeSelf)
			{
				((Component)arrowRotateHandle).gameObject.SetActive(true);
			}
		}
		float magnitude = val.magnitude;
		if (magnitude > 0.025f)
		{
			arrowScaleHandle.localScale = new Vector3(magnitude, 1f);
			val = val.normalized;
			float num2 = Mathf.Atan2(val.y, val.x);
			if (num2 < 0f)
			{
				num2 += (float)Math.PI * 2f;
			}
			arrowRotateHandle.localRotation = Quaternion.Euler(0f, 0f, num2 * 57.29578f);
			if (magnitude > 0.5f)
			{
				num2 += -(float)Math.PI / 2f;
				num = (int)((float)radialDivisions * (num2 / ((float)Math.PI * 2f) + 0.5f));
				if (num < 0)
				{
					num += radialDivisions;
				}
				else if (num >= radialDivisions)
				{
					num -= radialDivisions;
				}
			}
		}
		else
		{
			arrowScaleHandle.localScale = new Vector3(0f, 1f);
		}
		if (prevSelectedButton != num)
		{
			if (prevSelectedButton != -1)
			{
				cachedButtons[prevSelectedButton].button.OnMouseOut.Invoke();
			}
			if (num != -1)
			{
				cachedButtons[num].button.OnMouseOver.Invoke();
				if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
				{
					if (!((Component)inputGlyph).gameObject.activeSelf)
					{
						((Component)inputGlyph).gameObject.SetActive(true);
					}
				}
				else if (((Component)inputGlyph).gameObject.activeSelf)
				{
					((Component)inputGlyph).gameObject.SetActive(false);
				}
				Vector3 buttonDir = cachedButtons[num].buttonDir;
				buttonDir.z = inputGlyph.localPosition.z;
				inputGlyph.localPosition = buttonDir;
			}
			else if (((Component)inputGlyph).gameObject.activeSelf)
			{
				((Component)inputGlyph).gameObject.SetActive(false);
			}
		}
		if (num != -1 && player.GetButtonDown(11))
		{
			cachedButtons[num].button.ReceiveClickDown();
		}
		((Renderer)mr).GetPropertyBlock(mbp);
		mbp.SetInt("_SelectedSlice", num);
		((Renderer)mr).SetPropertyBlock(mbp);
		prevSelectedButton = num;
	}

	public CachedButtonObject[] CreateButtonsForStrings(string[] strings)
	{
		CacheButtons(strings.Length);
		BuildMesh(strings.Length);
		AlignTexts(strings);
		CachedButtonObject[] array = new CachedButtonObject[strings.Length];
		for (int i = 0; i < strings.Length; i++)
		{
			array[i] = cachedButtons[i];
		}
		return array;
	}

	private void CacheButtons(int numButtons)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected Obj, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected Obj, but got Unknown
		while (cachedButtons.Count < numButtons)
		{
			int tempButtonCount = cachedButtons.Count;
			CachedButtonObject item = new CachedButtonObject(Object.Instantiate<GameObject>(perButtonTemplateObject, ((Component)this).transform), cachedButtons.Count);
			cachedButtons.Add(item);
			item.button.OnMouseOver.AddListener((UnityAction)(() =>
			{
				ButtonMouseOver(tempButtonCount);
			}));
			item.button.OnMouseOut.AddListener((UnityAction)(() =>
			{
				ButtonMouseExit();
			}));
		}
		for (int num = 0; num < numButtons; num++)
		{
			if (!cachedButtons[num].gameObject.activeSelf)
			{
				cachedButtons[num].gameObject.SetActive(true);
			}
		}
		for (int num2 = numButtons; num2 < cachedButtons.Count; num2++)
		{
			cachedButtons[num2].gameObject.SetActive(false);
		}
	}

	public float GetButtonMaxStringWidth(int whichButton, float scale)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		CachedButtonObject cachedButtonObject = cachedButtons[whichButton];
		float y = ((Component)cachedButtonObject.textHandle).transform.localPosition.y;
		float num = Mathf.Sqrt(1f - y * y);
		Vector3 val = buttonLeftEdges[whichButton];
		Vector3 val2 = buttonRightEdges[whichButton];
		Vector3 val3 = new Vector3(0f - num, y);
		Vector3 val4 = new Vector3(num, y);
		if (val2.x < val.x)
		{
			Vector3 val5 = val;
			val = val2;
			val2 = val5;
		}
		if (Mathf.Abs(val.y) > 0.001f)
		{
			float num2 = y / val.y;
			if (num2 > 0f && num2 < 1f)
			{
				val3 = val * num2;
			}
		}
		if (Mathf.Abs(val2.y) > 0.001f)
		{
			float num3 = y / val2.y;
			if (num3 > 0f && num3 < 1f)
			{
				val4 = val2 * num3;
			}
		}
		if (Mathf.Abs(y) <= 0.001f)
		{
			if (((Component)cachedButtonObject.textHandle).transform.localPosition.x > 0f)
			{
				val3 = Vector3.zero;
				val4 = Vector3.right;
			}
			else
			{
				val3 = -Vector3.right;
				val4 = Vector3.zero;
			}
		}
		float num4 = Mathf.Abs((val3.x + val4.x) * 0.5f - ((Component)cachedButtonObject.textHandle).transform.localPosition.x) * 2f;
		return 0.95f * (Mathf.Abs(val3.x - val4.x) - num4) / scale;
	}

	private void AlignTexts(string[] strings)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f / ((Component)this).transform.localScale.x;
		horizontalSqueezedWidth = Mathf.Min(1f, (float)Math.PI * 2f / (float)strings.Length);
		for (int i = 0; i < strings.Length; i++)
		{
			Transform textHandle = cachedButtons[i].textHandle;
			textHandle.localPosition = buttonCenters[i];
			_ = strings[i].Length;
			Vector2 val = Vector2.op_Implicit(textHandle.localPosition);
			val = val.normalized;
			float num2 = Mathf.Abs(val.y);
			num2 = num2 * num2 * num2;
			Vector3 localPosition = textHandle.localPosition;
			localPosition.x += (1f - num2) * 0.1f * Mathf.Sign(localPosition.x);
			textHandle.localPosition = localPosition;
			float buttonMaxStringWidth = GetButtonMaxStringWidth(i, num);
			Vector2 sizeDelta = cachedButtons[i].trRT.sizeDelta;
			sizeDelta.x = 19f + buttonMaxStringWidth;
			cachedButtons[i].trRT.sizeDelta = sizeDelta;
			((TMP_Text)cachedButtons[i].tr).text = strings[i];
			cachedButtons[i].buttonCollider.points = cachedButtons[i].colliderPoints.ToArray();
			cachedButtons[i].ResetIcon();
			CachedButtonObject value = cachedButtons[i];
			value.buttonDir = buttonCenters[i].normalized;
			if (cachedButtons[i].isNew)
			{
				value.isNew = false;
				textHandle.localScale *= num;
			}
			cachedButtons[i] = value;
			if (angleText)
			{
				if ((double)Mathf.Abs(textHandle.localPosition.x) <= 0.001)
				{
					textHandle.localRotation = Quaternion.identity;
					continue;
				}
				float num3 = ((!(textHandle.localPosition.x < 0f)) ? Mathf.Atan2(textHandle.localPosition.y, textHandle.localPosition.x) : Mathf.Atan2(0f - textHandle.localPosition.y, 0f - textHandle.localPosition.x));
				textHandle.localRotation = Quaternion.Euler(0f, 0f, num3 * 57.29578f);
			}
		}
	}

	private void BuildMesh(int newRadialDivisions)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected Obj, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Expected Obj, but got Unknown
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)mesh))
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear();
		}
		radialDivisions = newRadialDivisions;
		buttonCenters = new Vector3[radialDivisions];
		buttonLeftEdges = new Vector3[radialDivisions];
		buttonRightEdges = new Vector3[radialDivisions];
		float num = (float)Math.PI * 2f / (float)radialDivisions;
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Color> list3 = new List<Color>();
		List<Vector2> list4 = new List<Vector2>();
		float num2 = 360f / (float)radialDivisions;
		_ = (float)Math.PI * 2f / (float)radialDivisions;
		float num3 = 1f / (float)(radialDivisions - 1);
		int num4 = (int)(num2 / sliceDegreesPerVert);
		num4 = Mathf.Max(1, num4 - 2);
		float num5 = ((radialDivisions != 1) ? 0f : 100f);
		for (int i = 0; i < radialDivisions; i++)
		{
			float num6 = (float)i * num3;
			float num7 = (float)i * num + -(float)Math.PI / 2f;
			float num8 = (float)(i + 1) * num + -(float)Math.PI / 2f;
			float num9 = (num7 + num8) * 0.5f;
			buttonCenters[i] = new Vector3(Mathf.Cos(num9), Mathf.Sin(num9)) * textPositionDistance;
			cachedButtons[i].colliderPoints.Clear();
			Color item = Color.HSVToRGB(num7 / ((float)Math.PI * 2f), 1f, 1f);
			int count = list.Count;
			int num10 = count + 1;
			int num11 = num10 + 1;
			list.Add(Vector3.zero);
			list3.Add(item);
			list4.Add(new Vector2(0f, num6));
			list.Add(new Vector3(Mathf.Cos(num7), Mathf.Sin(num7)));
			list.Add(new Vector3(Mathf.Cos(num8), Mathf.Sin(num8)));
			list3.Add(item);
			list3.Add(item);
			list4.Add(new Vector2(num5, num6));
			list4.Add(new Vector2(num5, num6));
			buttonLeftEdges[i] = list[num11];
			buttonRightEdges[i] = list[num10];
			Vector3 val = new Vector3(0f - list[num10].y, list[num10].x, 0f);
			Vector3 val2 = new Vector3(list[num11].y, 0f - list[num11].x, 0f);
			if (num4 == 0)
			{
				list2.Add(count);
				list2.Add(num11);
				list2.Add(num10);
			}
			else
			{
				int item2 = num10;
				for (int j = 0; j <= num4; j++)
				{
					int count2 = list.Count;
					float num12 = Mathf.Lerp(num7, num8, ((float)j + 1f) / (float)(num4 + 2));
					Vector3 val3 = new Vector3(Mathf.Cos(num12), Mathf.Sin(num12));
					float num13 = Mathf.Min(Vector3.Dot(val3, val), Vector3.Dot(val3, val2));
					num13 *= 4.712389f;
					if (radialDivisions == 1)
					{
						num13 = 100f;
					}
					list.Add(val3);
					list3.Add(item);
					list4.Add(new Vector2(num13, num6));
					list2.Add(count);
					list2.Add(count2);
					list2.Add(item2);
					item2 = count2;
				}
				list2.Add(count);
				list2.Add(num11);
				list2.Add(item2);
			}
			cachedButtons[i].colliderPoints.Add(Vector2.op_Implicit(list[count]));
			cachedButtons[i].colliderPoints.Add(Vector2.op_Implicit(list[num10]));
			for (int k = 0; k <= num4; k++)
			{
				cachedButtons[i].colliderPoints.Add(Vector2.op_Implicit(list[count + 3 + k]));
			}
			cachedButtons[i].colliderPoints.Add(Vector2.op_Implicit(list[num11]));
		}
		mesh.SetVertices(list);
		mesh.SetTriangles(list2.ToArray(), 0);
		mesh.SetColors(list3);
		mesh.SetUVs(0, list4.ToArray());
		if (!Object.op_Implicit((Object)(object)mf))
		{
			mf = ((Component)this).GetComponent<MeshFilter>();
		}
		if (!Object.op_Implicit((Object)(object)mr))
		{
			mr = ((Component)this).GetComponent<MeshRenderer>();
		}
		if (mbp == null)
		{
			mbp = new MaterialPropertyBlock();
		}
		mf.sharedMesh = mesh;
		((Renderer)mr).GetPropertyBlock(mbp);
		mbp.SetInt("_NumSlices", radialDivisions - 1);
		((Renderer)mr).SetPropertyBlock(mbp);
	}
}
