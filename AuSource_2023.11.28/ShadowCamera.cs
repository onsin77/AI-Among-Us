using UnityEngine;

public class ShadowCamera : MonoBehaviour
{
	public Shader Shadozer;

	public void OnEnable()
	{
		((Component)this).GetComponent<Camera>().SetReplacementShader(Shadozer, "RenderType");
	}

	public void OnDisable()
	{
		((Component)this).GetComponent<Camera>().ResetReplacementShader();
	}
}
