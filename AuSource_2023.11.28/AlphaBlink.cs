using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlphaBlink : MonoBehaviour
{
	public float Period = 1f;

	public float Ratio = 0.5f;

	private SpriteRenderer rend;

	private MeshRenderer mesh;

	private TextMeshPro tmPro;

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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		mesh = ((Component)this).GetComponent<MeshRenderer>();
		rend = ((Component)this).GetComponent<SpriteRenderer>();
		tmPro = ((Component)this).GetComponent<TextMeshPro>();
		if (Object.op_Implicit((Object)(object)tmPro))
		{
			baseColor = ((Graphic)tmPro).color;
		}
	}

	public void Update()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time % Period / Period;
		num = ((num < Ratio) ? 1 : 0);
		if (Object.op_Implicit((Object)(object)rend))
		{
			rend.color = new Color(baseColor.r, baseColor.g, baseColor.b, AlphaRange.Lerp(num));
		}
		if (Object.op_Implicit((Object)(object)tmPro))
		{
			((Graphic)tmPro).color = new Color(baseColor.r, baseColor.g, baseColor.b, AlphaRange.Lerp(num));
		}
		else if (Object.op_Implicit((Object)(object)mesh))
		{
			((Renderer)mesh).material.SetColor("_Color", new Color(baseColor.r, baseColor.g, baseColor.b, AlphaRange.Lerp(num)));
		}
	}

	public AlphaBlink()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
	}
}
