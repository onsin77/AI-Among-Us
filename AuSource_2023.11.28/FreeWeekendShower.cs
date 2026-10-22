using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class FreeWeekendShower : MonoBehaviour
{
	public TextMeshPro Output;

	private void Start()
	{
		((MonoBehaviour)this).StartCoroutine(Check());
	}

	private IEnumerator Check()
	{
		WaitForSeconds wait = new WaitForSeconds(1f);
		StringBuilder txt = new StringBuilder();
		while (true)
		{
			txt.Length = 0;
			if (Constants.ShouldFlipSkeld())
			{
				txt.AppendLine("Happy April Fools! Enjoy ehT Dleks!");
			}
			if (Constants.ShouldFlipSkeld())
			{
				txt.AppendLine("Happy April Fools! Enjoy ehT Dleks!");
			}
			((TMP_Text)Output).text = txt.ToString();
			yield return wait;
		}
	}
}
