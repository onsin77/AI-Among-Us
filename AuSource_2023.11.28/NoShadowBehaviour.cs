using UnityEngine;

public class NoShadowBehaviour : MonoBehaviour
{
	public Renderer rend;

	public bool didHit;

	public Renderer shadowChild;

	public Collider2D hitOverride;

	private BoxCollider2D bc;

	private bool isBox;

	private bool verticalBox;

	private Vector2[] boxCheckPoints;

	private const int edgePoints = 4;

	private const int totalPointsPerEdge = 6;

	public void Start()
	{
		LightSource.NoShadows.Add(((Component)this).gameObject, this);
		SetMaskFunction(8);
		CalculateBoxEdgeCheckPoints();
	}

	private void CalculateBoxEdgeCheckPoints()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		bc = ((Component)this).GetComponent<BoxCollider2D>();
		if (Object.op_Implicit((Object)(object)bc))
		{
			isBox = true;
			verticalBox = false;
			boxCheckPoints = new Vector2[8];
			Vector2 val = bc.size * 0.5f;
			Vector2 val2 = new Vector2(val.x, 0f);
			Vector2 val3 = new Vector2(0f, val.y);
			if (val.y > val.x)
			{
				verticalBox = true;
				val3 = val2;
				val2 = new Vector2(0f, val.y);
			}
			int num = 1;
			int num2 = 0;
			while (num < 5)
			{
				Vector2 val4 = Vector2.Lerp(-val2, val2, (float)num / 5f);
				boxCheckPoints[num2] = val4 + val3 + ((Collider2D)bc).offset;
				boxCheckPoints[num2 + 4] = val4 - val3 + ((Collider2D)bc).offset;
				num++;
				num2++;
			}
		}
	}

	public void OnDestroy()
	{
		LightSource.NoShadows.Remove(((Component)this).gameObject);
	}

	private void SetMaskFunction(int func)
	{
		rend.material.SetInt("_Mask", func);
		if (Object.op_Implicit((Object)(object)shadowChild))
		{
			shadowChild.material.SetInt("_Mask", func);
		}
	}

	internal void CheckHit(float lightRadius, Vector2 lightPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		if (!(Vector2.Distance(val, lightPosition) < lightRadius + 1f))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)hitOverride))
		{
			if (!PhysicsHelpers.AnythingBetween(lightPosition, val, Constants.ShadowMask, useTriggers: false, hitOverride, ((Component)this).transform))
			{
				didHit = true;
			}
		}
		else if (!PhysicsHelpers.AnythingBetween(lightPosition, val, Constants.ShadowMask, useTriggers: false, null, ((Component)this).transform))
		{
			didHit = true;
		}
		if (didHit || !isBox)
		{
			return;
		}
		int num = 0;
		int num2 = 4;
		if (verticalBox)
		{
			if (lightPosition.x < ((Component)this).transform.position.x)
			{
				num += 4;
				num2 += 4;
			}
		}
		else if (lightPosition.y < ((Component)this).transform.position.y)
		{
			num += 4;
			num2 += 4;
		}
		if (Object.op_Implicit((Object)(object)hitOverride))
		{
			for (int i = num; i < num2; i++)
			{
				if (!PhysicsHelpers.AnythingBetween(lightPosition, Vector2.op_Implicit(((Component)this).transform.TransformPoint(Vector2.op_Implicit(boxCheckPoints[i]))), Constants.ShadowMask, useTriggers: false, hitOverride, ((Component)this).transform))
				{
					didHit = true;
					break;
				}
			}
			return;
		}
		for (int j = num; j < num2; j++)
		{
			if (!PhysicsHelpers.AnythingBetween(lightPosition, Vector2.op_Implicit(((Component)this).transform.TransformPoint(Vector2.op_Implicit(boxCheckPoints[j]))), Constants.ShadowMask, useTriggers: false, (Collider2D)(object)bc, ((Component)this).transform))
			{
				didHit = true;
				break;
			}
		}
	}
}
