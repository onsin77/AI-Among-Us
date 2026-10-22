using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

public static class UITools
{
	public static GameObject InstantiateGUIObject<T>(GameObject prefab, Transform parent, string name) where T : Component
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = InstantiateGUIObject_Pre<T>(prefab, parent, name);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		RectTransform component = val.GetComponent<RectTransform>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)(name + " prefab is missing RectTransform component!"));
		}
		else
		{
			((Transform)component).localScale = Vector3.one;
		}
		return val;
	}

	public static GameObject InstantiateGUIObject<T>(GameObject prefab, Transform parent, string name, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition) where T : Component
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = InstantiateGUIObject_Pre<T>(prefab, parent, name);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		RectTransform component = val.GetComponent<RectTransform>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)(name + " prefab is missing RectTransform component!"));
		}
		else
		{
			((Transform)component).localScale = Vector3.one;
			component.pivot = pivot;
			component.anchorMin = anchorMin;
			component.anchorMax = anchorMax;
			component.anchoredPosition = anchoredPosition;
		}
		return val;
	}

	private static GameObject InstantiateGUIObject_Pre<T>(GameObject prefab, Transform parent, string name) where T : Component
	{
		if ((Object)(object)prefab == (Object)null)
		{
			Debug.LogError((object)(name + " prefab is null!"));
			return null;
		}
		GameObject val = Object.Instantiate<GameObject>(prefab);
		if (!string.IsNullOrEmpty(name))
		{
			((Object)val).name = name;
		}
		T component = val.GetComponent<T>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)(name + " prefab is missing the " + ((object)component).GetType().ToString() + " component!"));
			return null;
		}
		if ((Object)(object)parent != (Object)null)
		{
			val.transform.SetParent(parent, false);
		}
		return val;
	}

	public static Vector3 GetPointOnRectEdge(RectTransform rectTransform, Vector2 dir)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rectTransform == (Object)null)
		{
			return Vector3.zero;
		}
		if (dir != Vector2.zero)
		{
			dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
		}
		Rect rect = rectTransform.rect;
		dir = rect.center + Vector2.Scale(rect.size, dir * 0.5f);
		return Vector2.op_Implicit(dir);
	}

	public static Rect GetWorldSpaceRect(RectTransform rt)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rt == (Object)null)
		{
			return default;
		}
		Rect rect = rt.rect;
		Vector2 val = Vector2.op_Implicit(((Transform)rt).TransformPoint(Vector2.op_Implicit(new Vector2(rect.xMin, rect.yMin))));
		Vector2 val2 = Vector2.op_Implicit(((Transform)rt).TransformPoint(Vector2.op_Implicit(new Vector2(rect.xMin, rect.yMax))));
		Vector2 val3 = Vector2.op_Implicit(((Transform)rt).TransformPoint(Vector2.op_Implicit(new Vector2(rect.xMax, rect.yMin))));
		return new Rect(val.x, val.y, val3.x - val.x, val2.y - val.y);
	}

	public static Rect TransformRectTo(Transform from, Transform to, Rect rect)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val;
		Vector3 val2;
		Vector3 val3;
		if ((Object)(object)from != (Object)null)
		{
			val = from.TransformPoint(Vector2.op_Implicit(new Vector2(rect.xMin, rect.yMin)));
			val2 = from.TransformPoint(Vector2.op_Implicit(new Vector2(rect.xMin, rect.yMax)));
			val3 = from.TransformPoint(Vector2.op_Implicit(new Vector2(rect.xMax, rect.yMin)));
		}
		else
		{
			val = Vector2.op_Implicit(new Vector2(rect.xMin, rect.yMin));
			val2 = Vector2.op_Implicit(new Vector2(rect.xMin, rect.yMax));
			val3 = Vector2.op_Implicit(new Vector2(rect.xMax, rect.yMin));
		}
		if ((Object)(object)to != (Object)null)
		{
			val = to.InverseTransformPoint(val);
			val2 = to.InverseTransformPoint(val2);
			val3 = to.InverseTransformPoint(val3);
		}
		return new Rect(val.x, val.y, val3.x - val.x, val.y - val2.y);
	}

	public static Rect InvertY(Rect rect)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return new Rect(rect.xMin, rect.yMin, rect.width, 0f - rect.height);
	}

	public static void SetInteractable(Selectable selectable, bool state, bool playTransition)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)selectable == (Object)null)
		{
			return;
		}
		if (!playTransition)
		{
			if ((int)selectable.transition == 1)
			{
				ColorBlock colors = selectable.colors;
				float fadeDuration = colors.fadeDuration;
				colors.fadeDuration = 0f;
				selectable.colors = colors;
				selectable.interactable = state;
				colors.fadeDuration = fadeDuration;
				selectable.colors = colors;
			}
			else
			{
				selectable.interactable = state;
			}
		}
		else
		{
			selectable.interactable = state;
		}
	}
}
