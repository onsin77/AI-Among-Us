using UnityEngine;

public static class PlayerMaterial
{
	public struct Properties
	{
		public bool IsLocalPlayer;

		public MaskType MaskType;

		public int MaskLayer;

		public int ColorId;
	}

	public enum MaskType
	{
		None,
		SimpleUI,
		ComplexUI,
		Exile,
		ScrollingUI
	}

	public static readonly int MaskLayer = Shader.PropertyToID("_MaskLayer");

	private static readonly int BackColor = Shader.PropertyToID("_BackColor");

	private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");

	private static readonly int VisorColor = Shader.PropertyToID("_VisorColor");

	public static void SetMaskLayerBasedOnLocalPlayer(SpriteRenderer sprite, bool isLocalPlayer)
	{
		((Renderer)sprite).material.SetInt(MaskLayer, (!isLocalPlayer) ? 1 : 2);
	}

	public static void SetColors(int colorId, Renderer rend)
	{
		if (Object.op_Implicit((Object)(object)rend))
		{
			SetColors(colorId, rend.material);
		}
	}

	public static void SetColors(Color color, Renderer rend)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rend))
		{
			SetColors(color, rend.material);
		}
	}

	public static void SetColors(int colorId, Material material)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)material == (Object)null) && colorId >= 0 && colorId < Palette.PlayerColors.Length)
		{
			material.SetColor(BackColor, Color32.op_Implicit(Palette.ShadowColors[colorId]));
			material.SetColor(BodyColor, Color32.op_Implicit(Palette.PlayerColors[colorId]));
			material.SetColor(VisorColor, Color32.op_Implicit(Palette.VisorColor));
		}
	}

	public static void SetColors(Color color, Material material)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)material == (Object)null))
		{
			material.SetColor(BackColor, color);
			material.SetColor(BodyColor, color);
			material.SetColor(VisorColor, Color32.op_Implicit(Palette.VisorColor));
		}
	}
}
