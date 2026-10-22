using UnityEngine;

public class SpriteScroller : MonoBehaviour
{
	public Renderer rend;

	public float XRate = 1f;

	public float YRate = 1f;

	private void Update()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rend))
		{
			rend.material.SetTextureOffset("_MainTex", new Vector2(Time.time * XRate, Time.time * YRate));
		}
	}
}
