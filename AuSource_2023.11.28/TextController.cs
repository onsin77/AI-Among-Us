using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TextController : MonoBehaviour
{
	public float Scale = 1f;

	[Multiline]
	public string Text;

	private string displaying;

	[HideInInspector]
	private Texture2D texture;

	[HideInInspector]
	private Texture2D colorTexture;

	private MeshRenderer rend;

	private float _scale = float.NegativeInfinity;

	public Color Color = Color.white;

	private Color lastColor;

	public Vector3 Offset;

	public bool topAligned;

	public void Update()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected Obj, but got Unknown
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected Obj, but got Unknown
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)rend))
		{
			rend = ((Component)this).GetComponent<MeshRenderer>();
		}
		if (string.IsNullOrEmpty(Text))
		{
			((Renderer)rend).enabled = false;
			return;
		}
		if (displaying == null || displaying.GetHashCode() != Text.GetHashCode() || Color != lastColor)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 1;
			for (int i = 0; i < Text.Length; i++)
			{
				if (Text[i] == '\n')
				{
					num2 = 0;
					num3++;
					continue;
				}
				num2++;
				if (num2 > num)
				{
					num = num2;
				}
			}
			if (!Object.op_Implicit((Object)(object)texture) || !Object.op_Implicit((Object)(object)colorTexture))
			{
				if (!Object.op_Implicit((Object)(object)texture))
				{
					texture = new Texture2D(num, num3, (TextureFormat)5, false);
					((Texture)texture).filterMode = (FilterMode)0;
					((Texture)texture).wrapMode = (TextureWrapMode)1;
				}
				if (!Object.op_Implicit((Object)(object)colorTexture))
				{
					colorTexture = new Texture2D(num, num3, (TextureFormat)5, false);
					((Texture)colorTexture).filterMode = (FilterMode)0;
					((Texture)colorTexture).wrapMode = (TextureWrapMode)1;
				}
			}
			else if (((Texture)texture).width != num || ((Texture)texture).height != num3)
			{
				texture.Resize(num, num3, (TextureFormat)5, false);
				colorTexture.Resize(num, num3, (TextureFormat)5, false);
			}
			Color[] array = new Color[num * num3];
			array.SetAll(Color);
			colorTexture.SetPixels(array);
			array.SetAll(new Color(0.125f, 0f, 0f));
			texture.SetPixels(array);
			int num4 = 0;
			int num5 = ((Texture)texture).height - 1;
			Color color = Color;
			for (int j = 0; j < Text.Length; j++)
			{
				char c = Text[j];
				switch (c)
				{
				case '\n':
					num4 = 0;
					num5--;
					break;
				default:
					texture.SetPixel(num4, num5, new Color((float)(int)c / 256f, 0f, 0f));
					colorTexture.SetPixel(num4, num5, color);
					num4++;
					break;
				case '\r':
					break;
				}
			}
			texture.Apply(false, false);
			colorTexture.Apply(false, false);
			((Renderer)rend).enabled = true;
			((Renderer)rend).material.SetTexture("_InputTex", (Texture)(object)texture);
			((Renderer)rend).material.SetTexture("_ColorTex", (Texture)(object)colorTexture);
			_scale = float.NegativeInfinity;
			displaying = Text;
			lastColor = Color;
		}
		if (_scale != Scale)
		{
			_scale = Scale;
			((Component)this).transform.localScale = new Vector3((float)((Texture)texture).width, (float)((Texture)texture).height, 1f) * Scale;
			if (topAligned)
			{
				((Component)this).transform.localPosition = Offset + new Vector3((float)((Texture)texture).width * Scale / 2f, (float)(-((Texture)texture).height) * Scale / 2f, 0f);
			}
		}
	}

	public TextController()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}
