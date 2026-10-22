using UnityEngine;

public class BuildDefineConditionalObject : MonoBehaviour
{
	public bool isDefined;

	private void Awake()
	{
		((Component)this).gameObject.SetActive(isDefined);
	}

	private void OnEnable()
	{
		if (!isDefined)
		{
			((Component)this).gameObject.SetActive(isDefined);
		}
	}
}
