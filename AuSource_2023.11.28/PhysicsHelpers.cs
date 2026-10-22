using UnityEngine;

public static class PhysicsHelpers
{
	private static Collider2D[] colliderHits = new Collider2D[20];

	private static RaycastHit2D[] castHits = new RaycastHit2D[20];

	private static Vector2 temp = default;

	private static ContactFilter2D filter = new ContactFilter2D
	{
		useLayerMask = true
	};

	public static bool CircleContains(Vector2 source, float radius, int layerMask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Physics2D.OverlapCircleNonAlloc(source, radius, colliderHits, layerMask) > 0;
	}

	public static bool AnyEdgeTriggerBetween(Vector2 source, Vector2 target, int layerMask)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		filter.layerMask = LayerMask.op_Implicit(layerMask);
		filter.useTriggers = true;
		temp.x = target.x - source.x;
		temp.y = target.y - source.y;
		int num = Physics2D.Raycast(source, temp, filter, castHits, temp.magnitude);
		for (int i = 0; i < num; i++)
		{
			if (castHits[i].collider.isTrigger)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnythingBetween(Vector2 source, Vector2 target, int layerMask, bool useTriggers)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		filter.layerMask = LayerMask.op_Implicit(layerMask);
		filter.useTriggers = useTriggers;
		temp.x = target.x - source.x;
		temp.y = target.y - source.y;
		return Physics2D.Raycast(source, temp, filter, castHits, temp.magnitude) > 0;
	}

	public static bool AnythingBetween(Collider2D castObject, Vector2 source, Vector2 target, int layerMask, bool useTriggers)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		filter.layerMask = LayerMask.op_Implicit(layerMask);
		filter.useTriggers = useTriggers;
		temp.x = target.x - source.x;
		temp.y = target.y - source.y;
		return castObject.Cast(temp, filter, castHits, temp.magnitude) > 0;
	}

	public static bool AnyNonTriggersBetween(Vector2 source, Vector2 dirNorm, float mag, int layerMask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		int num = Physics2D.RaycastNonAlloc(source, dirNorm, castHits, mag, layerMask);
		bool result = false;
		for (int i = 0; i < num; i++)
		{
			if (!castHits[i].collider.isTrigger)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool AnythingBetween(Vector2 source, Vector2 target, int layerMask, bool useTriggers, Collider2D itemToIgnore, Transform objectToIgnore)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		filter.layerMask = LayerMask.op_Implicit(layerMask);
		filter.useTriggers = useTriggers;
		temp.x = target.x - source.x;
		temp.y = target.y - source.y;
		if (Physics2D.Raycast(source, temp, filter, castHits, temp.magnitude) > 0)
		{
			if ((Object)(object)castHits[0].collider != (Object)(object)itemToIgnore)
			{
				return (Object)(object)((Component)castHits[0].collider).transform != (Object)(object)objectToIgnore;
			}
			return false;
		}
		return false;
	}

	static PhysicsHelpers()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
	}
}
