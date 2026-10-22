using System.Runtime.CompilerServices;
using UnityEngine;

public class Wire : MonoBehaviour
{
	private const int WireDepth = -14;

	public SpriteRenderer Liner;

	public SpriteRenderer ColorBase;

	public SpriteRenderer SymbolBase;

	public SpriteRenderer ColorEnd;

	public Collider2D hitbox;

	public SpriteRenderer WireTip;

	public sbyte WireId;

	public Vector2 BaseWorldPos
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		internal set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		BaseWorldPos = Vector2.op_Implicit(((Component)this).transform.position);
	}

	public void ResetLine(Vector3 targetWorldPos, bool reset = false)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (reset)
		{
			((Component)Liner).transform.localScale = new Vector3(0f, 0f, 0f);
			((Component)WireTip).transform.eulerAngles = Vector3.zero;
			((Component)WireTip).transform.position = ((Component)this).transform.position;
			return;
		}
		Vector2 val = Vector2.op_Implicit(targetWorldPos - ((Component)this).transform.position);
		Vector2 normalized = val.normalized;
		Vector3 val2 = default;
		val2 = Vector2.op_Implicit(val - normalized * 0.075f);
		val2.z = -0.01f;
		((Component)WireTip).transform.localPosition = val2;
		float magnitude = val.magnitude;
		((Component)Liner).transform.localScale = new Vector3(magnitude, 1f, 1f);
		((Component)Liner).transform.localPosition = Vector2.op_Implicit(val / 2f);
		((Component)WireTip).transform.LookAt2d(targetWorldPos);
		((Component)Liner).transform.localEulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, val));
	}

	public void ConnectRight(WireNode node)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)node).transform.position;
		ResetLine(position);
	}

	public void SetColor(Color color, Sprite symbol)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		SymbolBase.sprite = symbol;
		((Renderer)Liner).material.SetColor("_Color", color);
		ColorBase.color = color;
		ColorEnd.color = color;
	}
}
