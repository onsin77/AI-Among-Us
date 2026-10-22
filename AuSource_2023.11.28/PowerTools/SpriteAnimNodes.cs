using UnityEngine;

namespace PowerTools;

public class SpriteAnimNodes : MonoBehaviour
{
	public static readonly int NUM_NODES = 10;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_node0 = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private Vector2 m_node1 = Vector2.zero;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_node2 = Vector2.zero;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_node3 = Vector2.zero;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_node4 = Vector2.zero;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_node5 = Vector2.zero;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_node6 = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private Vector2 m_node7 = Vector2.zero;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_node8 = Vector2.zero;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_node9 = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private float m_ang0;

	[HideInInspector]
	[SerializeField]
	private float m_ang1;

	[HideInInspector]
	[SerializeField]
	private float m_ang2;

	[SerializeField]
	[HideInInspector]
	private float m_ang3;

	[SerializeField]
	[HideInInspector]
	private float m_ang4;

	[SerializeField]
	[HideInInspector]
	private float m_ang5;

	[SerializeField]
	[HideInInspector]
	private float m_ang6;

	[SerializeField]
	[HideInInspector]
	private float m_ang7;

	[SerializeField]
	[HideInInspector]
	private float m_ang8;

	[HideInInspector]
	[SerializeField]
	private float m_ang9;

	private SpriteRenderer m_spriteRenderer;

	public Vector3 GetPosition(int nodeId, bool ignoredPivot = false)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_spriteRenderer == (Object)null)
		{
			m_spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		}
		if ((Object)(object)m_spriteRenderer == (Object)null || (Object)(object)m_spriteRenderer.sprite == (Object)null)
		{
			return Vector2.op_Implicit(Vector2.zero);
		}
		Vector3 val = GetLocalPosition(nodeId, ignoredPivot);
		val = ((Component)this).transform.rotation * val;
		val.Scale(((Component)this).transform.lossyScale);
		val += ((Component)this).transform.position;
		return val;
	}

	public Vector3 GetLocalPosition(int nodeId, bool ignoredPivot = false)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_spriteRenderer == (Object)null)
		{
			m_spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		}
		if ((Object)(object)m_spriteRenderer == (Object)null || (Object)(object)m_spriteRenderer.sprite == (Object)null)
		{
			return Vector2.op_Implicit(Vector2.zero);
		}
		Vector3 val = Vector2.op_Implicit(GetPositionRaw(nodeId));
		val.y = 0f - val.y;
		if (ignoredPivot)
		{
			Vector3 val2 = val;
			Rect rect = m_spriteRenderer.sprite.rect;
			val = val2 + Vector2.op_Implicit(rect.size * 0.5f - m_spriteRenderer.sprite.pivot);
		}
		val *= 1f / m_spriteRenderer.sprite.pixelsPerUnit;
		if (m_spriteRenderer.flipX)
		{
			val.x = 0f - val.x;
		}
		if (m_spriteRenderer.flipY)
		{
			val.y = 0f - val.y;
		}
		return val;
	}

	public float GetAngle(int nodeId)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		float angleRaw = GetAngleRaw(nodeId);
		if ((Object)(object)m_spriteRenderer == (Object)null)
		{
			m_spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		}
		if ((Object)(object)m_spriteRenderer == (Object)null || (Object)(object)m_spriteRenderer.sprite == (Object)null)
		{
			return 0f;
		}
		return angleRaw + ((Component)this).transform.eulerAngles.z;
	}

	public Vector2 GetPositionRaw(int nodeId)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		return nodeId switch
		{
			0 => m_node0, 
			1 => m_node1, 
			2 => m_node2, 
			3 => m_node3, 
			4 => m_node4, 
			5 => m_node5, 
			6 => m_node6, 
			7 => m_node7, 
			8 => m_node8, 
			9 => m_node9, 
			_ => Vector2.zero, 
		};
	}

	public float GetAngleRaw(int nodeId)
	{
		return nodeId switch
		{
			0 => m_ang0, 
			1 => m_ang1, 
			2 => m_ang2, 
			3 => m_ang3, 
			4 => m_ang4, 
			5 => m_ang5, 
			6 => m_ang6, 
			7 => m_ang7, 
			8 => m_ang8, 
			9 => m_ang9, 
			_ => 0f, 
		};
	}

	public void Reset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		m_node0 = Vector2.zero;
		m_node1 = Vector2.zero;
		m_node2 = Vector2.zero;
		m_node3 = Vector2.zero;
		m_node4 = Vector2.zero;
		m_node5 = Vector2.zero;
		m_node6 = Vector2.zero;
		m_node7 = Vector2.zero;
		m_node8 = Vector2.zero;
		m_node9 = Vector2.zero;
		m_ang0 = 0f;
		m_ang1 = 0f;
		m_ang2 = 0f;
		m_ang3 = 0f;
		m_ang4 = 0f;
		m_ang5 = 0f;
		m_ang6 = 0f;
		m_ang7 = 0f;
		m_ang8 = 0f;
		m_ang9 = 0f;
	}

	public SpriteAnimNodes()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
	}
}
