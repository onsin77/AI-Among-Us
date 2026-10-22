using TMPro;
using UnityEngine;

public class UpdateProductTimerDisplay : MonoBehaviour
{
	private const float TimeBetweenRefresh = 1f;

	public TextMeshPro Tmp;

	private IBuyable currentProduct;

	private float lastUpdate = float.MinValue;

	[SerializeField]
	private bool DisplayShort;

	private void OnEnable()
	{
		((TMP_Text)Tmp).text = "";
		RefreshTimer();
	}

	private void FixedUpdate()
	{
		if (Time.realtimeSinceStartup > lastUpdate + 1f)
		{
			RefreshTimer();
			lastUpdate = Time.realtimeSinceStartup;
		}
	}

	public void SetProduct(IBuyable product)
	{
		currentProduct = product;
	}

	private void RefreshTimer()
	{
		if (currentProduct != null)
		{
			if (DisplayShort)
			{
				((TMP_Text)Tmp).text = currentProduct.LimitedTimeAvailable.timeEnd.TimeLeftFromNowShort();
			}
			else
			{
				((TMP_Text)Tmp).text = currentProduct.LimitedTimeAvailable.timeEnd.TimeLeftFromNow();
			}
		}
	}
}
