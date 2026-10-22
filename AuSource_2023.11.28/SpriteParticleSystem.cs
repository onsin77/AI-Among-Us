using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SpriteParticleSystem : MonoBehaviour
{
	public Sprite sprite;

	public ParticleSystemRenderer ren;

	private MaterialPropertyBlock block;

	private void OnEnable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		block = new MaterialPropertyBlock();
		Renderer component = ((Component)this).GetComponent<Renderer>();
		ren = (ParticleSystemRenderer)(object)((component is ParticleSystemRenderer) ? component : null);
		SetPropertyBlock();
	}

	private void SetPropertyBlock()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		if (block == null)
		{
			block = new MaterialPropertyBlock();
		}
		((Renderer)ren).GetPropertyBlock(block);
		block.SetTexture("_MainTex", (Texture)(object)sprite.texture);
		((Renderer)ren).SetPropertyBlock(block);
	}

	private void OnValidate()
	{
		SetPropertyBlock();
	}
}
