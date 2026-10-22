using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
	public float speed = 20f;

	public Transform toRotate;

	public SpriteRenderer loadingSpinner;

	public SpriteRenderer background;

	public void Toggle(bool on)
	{
		((Component)this).gameObject.SetActive(on);
	}

	public void SetMaskLayer(int layer)
	{
		((Renderer)loadingSpinner).material.SetInt(PlayerMaterial.MaskLayer, layer);
		((Renderer)background).material.SetInt(PlayerMaterial.MaskLayer, layer);
	}

	private void Start()
	{
		PlayerMaterial.SetColors(2, (Renderer)(object)loadingSpinner);
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		toRotate.RotateAround(toRotate.position, Vector3.forward, speed * Time.deltaTime);
	}
}
