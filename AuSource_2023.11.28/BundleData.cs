using UnityEngine;

public class BundleData : ScriptableObject, IBuyable
{
	public const string TranslationPrefix = "Bundle.";

	public BundleSizes BundleSize;

	public string productId;

	public int beanCost;

	public int starCost;

	public CosmeticData[] cosmetics;

	public Sprite StoreImage;

	public bool NotInStore;

	public bool Free;

	public bool paidOnMobile;

	public string ProdId => productId;

	public int BeanCost => beanCost;

	public int StarCost => starCost;

	public bool PaidOnMobile => paidOnMobile;

	public LimitedTimeStartEnd LimitedTimeAvailable => default;

	public string GetItemName()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString("Bundle." + productId, productId);
	}

	public string GetPurchasingProductId()
	{
		return productId;
	}

	public string[] GetCosmeticIDs()
	{
		string[] array = new string[cosmetics.Length];
		for (int i = 0; i < cosmetics.Length; i++)
		{
			if (!((Object)(object)cosmetics[i] == (Object)null))
			{
				array[i] = cosmetics[i].ProductId;
			}
		}
		return array;
	}
}
