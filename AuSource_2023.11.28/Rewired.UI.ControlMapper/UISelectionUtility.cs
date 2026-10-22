using System;
using System.Collections.Generic;
using Rewired.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

public static class UISelectionUtility
{
	private static Selectable[] s_reusableAllSelectables = new Selectable[0];

	public static Selectable FindNextSelectable(Selectable selectable, Transform transform, Vector3 direction)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		if (Selectable.allSelectableCount > s_reusableAllSelectables.Length)
		{
			s_reusableAllSelectables = new Selectable[Selectable.allSelectableCount];
		}
		int num = Selectable.AllSelectablesNoAlloc(s_reusableAllSelectables);
		IList<Selectable> list = s_reusableAllSelectables;
		direction.Normalize();
		Vector2 val2 = Vector2.op_Implicit(direction);
		Vector2 val3 = Vector2.op_Implicit(UITools.GetPointOnRectEdge(val, val2));
		bool flag = val2 == Vector2.right * -1f || val2 == Vector2.right;
		float num2 = float.PositiveInfinity;
		float num3 = float.PositiveInfinity;
		Selectable val4 = null;
		Selectable val5 = null;
		Vector2 val6 = val3 + val2 * 999999f;
		float num4 = default;
		for (int i = 0; i < num; i++)
		{
			Selectable val7 = list[i];
			if ((Object)(object)val7 == (Object)(object)selectable || (Object)(object)val7 == (Object)null)
			{
				continue;
			}
			Navigation navigation = val7.navigation;
			if ((int)navigation.mode == 0 || (!val7.IsInteractable() && !ReflectionTools.GetPrivateField<Selectable, bool>(val7, "m_GroupsAllowInteraction")))
			{
				continue;
			}
			Transform transform2 = ((Component)val7).transform;
			RectTransform val8 = (RectTransform)(object)((transform2 is RectTransform) ? transform2 : null);
			if ((Object)(object)val8 == (Object)null)
			{
				continue;
			}
			Rect val9 = UITools.InvertY(UITools.TransformRectTo((Transform)(object)val8, transform, val8.rect));
			if (MathTools.LineIntersectsRect(val3, val6, val9, ref num4))
			{
				if (flag)
				{
					num4 *= 0.25f;
				}
				if (num4 < num3)
				{
					num3 = num4;
					val5 = val7;
				}
			}
			Rect rect = val8.rect;
			Vector2 val10 = Vector2.op_Implicit(UnityTools.TransformPoint((Transform)(object)val8, transform, Vector2.op_Implicit(rect.center))) - val3;
			if (!(Mathf.Abs(Vector2.Angle(val2, val10)) > 75f))
			{
				float sqrMagnitude = val10.sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					val4 = val7;
				}
			}
		}
		if ((Object)(object)val5 != (Object)null && (Object)(object)val4 != (Object)null)
		{
			if (num3 > num2)
			{
				return val4;
			}
			return val5;
		}
		Array.Clear(s_reusableAllSelectables, 0, s_reusableAllSelectables.Length);
		return val5 ?? val4;
	}
}
