using UnityEngine;

public class AlertFlash : MonoBehaviour
{
	private const string FLASH_TRIGGER = "OnFlash";

	private const string OVERLAY_BOOL = "Overlay";

	[SerializeField]
	[Range(0f, 1f)]
	private float opacity = 1f;

	[SerializeField]
	[Space(10f)]
	private Animator animator;

	[SerializeField]
	private MeshRenderer screenSpaceRenderer;

	private void Awake()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		((Renderer)screenSpaceRenderer).material = new Material(((Renderer)screenSpaceRenderer).sharedMaterial);
	}

	private void LateUpdate()
	{
		bool flag = opacity > 0.01f;
		if (((Renderer)screenSpaceRenderer).enabled != flag)
		{
			((Renderer)screenSpaceRenderer).enabled = flag;
		}
		if (flag)
		{
			((Renderer)screenSpaceRenderer).material.SetFloat("_Opacity", opacity);
		}
	}

	public void Flash()
	{
		animator.SetTrigger("OnFlash");
	}

	public void SetOverlay(bool enabled)
	{
		animator.SetBool("Overlay", enabled);
	}
}
