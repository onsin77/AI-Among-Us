using System;
using UnityEngine;

public class AlphaPulse : MonoBehaviour
{
	public float Offset = 1f;

	public float Duration = 2.5f;

	private SpriteRenderer rend;

	private MeshRenderer mesh;

	public FloatRange AlphaRange = new FloatRange(0.2f, 0.5f);

	public Color baseColor = Color.white;

	public void SetColor(Color c)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Start();
		baseColor = c;
		Update();
	}

	private void Start()
	{
		mesh = ((Component)this).GetComponent<MeshRenderer>();
		rend = ((Component)this).GetComponent<SpriteRenderer>();
	}

	public void Update()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		float v = Mathf.Abs(Mathf.Cos((Offset + Time.time) * (float)Math.PI / Duration));
		if (Object.op_Implicit((Object)(object)rend))
		{
			rend.color = new Color(baseColor.r, baseColor.g, baseColor.b, AlphaRange.Lerp(v));
		}
		if (Object.op_Implicit((Object)(object)mesh))
		{
			((Renderer)mesh).material.SetColor("_Color", new Color(baseColor.r, baseColor.g, baseColor.b, AlphaRange.Lerp(v)));
		}
	}

	public AlphaPulse()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
	}
}
