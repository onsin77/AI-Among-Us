using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
	public static void SetAllGameObjectsActive<T>(this IList<T> self, bool isActive) where T : MonoBehaviour
	{
		for (int i = 0; i < self.Count; i++)
		{
			((Component)(object)self[i]).gameObject.SetActive(isActive);
		}
	}

	public static T Find<T>(this List<T> self, GameObject toFind) where T : MonoBehaviour
	{
		for (int i = 0; i < self.Count; i++)
		{
			T val = self[i];
			if ((Object)(object)((Component)(object)val).gameObject == (Object)(object)toFind)
			{
				return val;
			}
		}
		return default;
	}

	public static void SetLocalX(this Transform self, float x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = self.localPosition;
		localPosition.x = x;
		self.localPosition = localPosition;
	}

	public static void SetLocalY(this Transform self, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = self.localPosition;
		localPosition.y = y;
		self.localPosition = localPosition;
	}

	public static void SetLocalZ(this Transform self, float z)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = self.localPosition;
		localPosition.z = z;
		self.localPosition = localPosition;
	}

	public static void SetWorldZ(this Transform self, float z)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = self.position;
		position.z = z;
		self.position = position;
	}

	public static void LookAt2d(this Transform self, Vector3 target)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = target - ((Component)self).transform.position;
		val.Normalize();
		float num = Mathf.Atan2(val.y, val.x);
		if (((Component)self).transform.lossyScale.x < 0f)
		{
			num += (float)Math.PI;
		}
		((Component)self).transform.rotation = Quaternion.Euler(0f, 0f, num * 57.29578f);
	}

	public static void LookAt2d(this Transform self, Transform target)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		self.LookAt2d(((Component)target).transform.position);
	}

	public static void DestroyChildren(this Transform self)
	{
		for (int num = self.childCount - 1; num > -1; num--)
		{
			Transform child = self.GetChild(num);
			((Component)child).transform.SetParent((Transform)null);
			Object.Destroy((Object)(object)((Component)child).gameObject);
		}
	}

	public static void DestroyChildren(this MonoBehaviour self)
	{
		for (int num = ((Component)self).transform.childCount - 1; num > -1; num--)
		{
			Object.Destroy((Object)(object)((Component)((Component)self).transform.GetChild(num)).gameObject);
		}
	}

	public static bool IsDestroyedOrNull(this MonoBehaviour self)
	{
		return (Object)(object)self == (Object)null;
	}

	public static bool IsDestroyedOrNull(this GameObject self)
	{
		return (Object)(object)self == (Object)null;
	}

	public static void ForEachChild(this GameObject self, Action<GameObject> todo)
	{
		for (int num = self.transform.childCount - 1; num > -1; num--)
		{
			todo(((Component)self.transform.GetChild(num)).gameObject);
		}
	}

	public static void ForEachChildBehavior<T>(this MonoBehaviour self, Action<T> todo) where T : MonoBehaviour
	{
		for (int num = ((Component)self).transform.childCount - 1; num > -1; num--)
		{
			T component = ((Component)((Component)self).transform.GetChild(num)).GetComponent<T>();
			if (Object.op_Implicit((Object)(object)component))
			{
				todo(component);
			}
		}
	}
}
