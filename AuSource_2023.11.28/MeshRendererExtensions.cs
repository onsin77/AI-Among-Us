using UnityEngine;

public static class MeshRendererExtensions
{
	public static void SetSprite(this MeshRenderer self, Texture2D spr)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)spr != (Object)null)
		{
			self.SetCutout(spr);
			((Renderer)self).material.color = Color.white;
		}
		else
		{
			self.SetCutout(null);
			((Renderer)self).material.color = Color.clear;
		}
	}

	public static void SetCutout(this MeshRenderer self, Texture2D txt)
	{
		((Renderer)self).material.SetTexture("_MainTex", (Texture)(object)txt);
		((Renderer)self).material.SetTexture("_EmissionMap", (Texture)(object)txt);
	}
}
