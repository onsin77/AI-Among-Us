using AmongUs.Data;
using Assets.InnerNet;
using TMPro;
using UnityEngine;

public class BundlePurchaseButton : PurchaseButton
{
	public BundleData ourBundle;

	public void UpdateBundle()
	{
		Purchased = DataManager.Player.Purchases.GetPurchase(ProductId, "");
		((Renderer)PurchasedIcon).enabled = Purchased;
		((Renderer)PurchasedOverlay).enabled = Purchased;
		RefreshPriceText();
	}

	protected override void Start()
	{
		base.Start();
		Parent = DestroyableSingleton<StoreMenu>.Instance;
		Product = ourBundle;
		Name = ((Object)this).name;
		ProductId = ourBundle.ProdId;
		Purchased = DataManager.Player.Purchases.GetPurchase(ProductId, "");
		((Object)this).name = ourBundle.ProdId;
		((Renderer)PurchasedIcon).enabled = Purchased;
		((Renderer)PurchasedOverlay).enabled = Purchased;
		((TMP_Text)NameText).text = ourBundle.GetItemName();
		HatImage.FrontLayer.sprite = ourBundle.StoreImage;
		RefreshPriceText();
	}

	private void OnDisable()
	{
		PassiveButton.ReceiveMouseOut();
	}

	private void RefreshPriceText()
	{
		if (Purchased)
		{
			((TMP_Text)PriceText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Owned);
		}
		else if (ourBundle.StarCost > 0)
		{
			Price = ourBundle.StarCost.ToString();
			((TMP_Text)PriceText).text = "<sprite name=\"UI_Currency_Star\"> " + Price;
		}
		else if (ourBundle.BeanCost > 0)
		{
			Price = ourBundle.BeanCost.ToString();
			((TMP_Text)PriceText).text = "<sprite name=\"UI_Currency_Bean\"> " + Price;
		}
	}

	public override void SetItem(IBuyable product, string productId, string name, string price, bool purchased, CurrencyTypes currency)
	{
		base.SetItem(product, productId, name, price, purchased, currency);
		if (Product is BundleData)
		{
			BundleData bundleData = (BundleData)Product;
			((Renderer)ImageMask).enabled = true;
			ourBundle = bundleData;
		}
	}
}
