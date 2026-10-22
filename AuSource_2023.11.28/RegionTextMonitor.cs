using System.Collections;
using TMPro;
using UnityEngine;

public class RegionTextMonitor : MonoBehaviour
{
	private void Start()
	{
		((MonoBehaviour)this).StartCoroutine(GetRegionText());
	}

	private IEnumerator GetRegionText()
	{
		while (DestroyableSingleton<ServerManager>.Instance.CurrentRegion == null)
		{
			yield return null;
		}
		((TMP_Text)((Component)this).GetComponent<TextMeshPro>()).text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName, DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name);
	}
}
