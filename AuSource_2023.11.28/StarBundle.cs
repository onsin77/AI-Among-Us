using UnityEngine;

[CreateAssetMenu]
public class StarBundle : ScriptableObject, IBuyable
{
	public const string TranslationPrefix = "Bundle.";

	public string epicId = "";

	public string productId;

	public string SteamAppId;

	public string SteamPrice;

	public float price;

	public int starAmount;

	public Sprite StoreImage;

	public Sprite StoreBackImage;

	public string ProdId => productId;

	public int BeanCost => 0;

	public int StarCost => 0;

	public string EpicId => epicId;

	public bool PaidOnMobile => false;

	public LimitedTimeStartEnd LimitedTimeAvailable => default;

	public string GetItemName()
	{
		int num = ProdId.LastIndexOf("_");
		string text = ProdId;
		if (num > -1)
		{
			text = text.Substring(0, num);
		}
		string text2 = DestroyableSingleton<TranslationController>.Instance.GetString("Bundle." + text, ((Object)this).name);
		if (string.Equals(text2, ((Object)this).name))
		{
			return text2;
		}
		return $"{text2} {starAmount}";
	}

	public string GetPurchasingProductId()
	{
		return SteamAppId;
	}
}
