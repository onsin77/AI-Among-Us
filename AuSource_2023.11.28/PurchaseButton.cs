using System;
using Assets.InnerNet;
using Innersloth.Assets;
using TMPro;
using UnityEngine;

public class PurchaseButton : MonoBehaviour
{
	private const float normalSize = 1.75f;

	[Header("Icons")]
	public SpriteRenderer PurchasedIcon;

	public SpriteRenderer LimitedTimeIcon;

	public UpdateProductTimerDisplay LimitedTimeText;

	public SpriteRenderer CrewHeadImage;

	public HatParent HatImage;

	public VisorLayer VisorLayer;

	public Sprite MannequinFrame;

	public LoadingSpinner LoadingIndicator;

	[Header("Background")]
	public SpriteRenderer Background;

	public SpriteRenderer SelectionHighlight;

	public SpriteRenderer GlassSprite;

	public SpriteRenderer GlassMask;

	public SpriteRenderer ImageMask;

	public SpriteRenderer PurchasedOverlay;

	[Header("Button")]
	public BoxCollider2D clickCollider;

	public PassiveButton PassiveButton;

	[Header("Product Info")]
	public TextMeshPro NameText;

	public TextMeshPro PriceText;

	public bool Purchased;

	public string Name;

	public string Price;

	public string ProductId;

	public GameObject PriceLabel;

	[Header("Label")]
	public SpriteRenderer LabelMask;

	public SpriteRenderer LabelBackground;

	public SpriteRenderer LabelGlass;

	public IBuyable Product;

	private Action onDestroy;

	public StoreMenu Parent { get; set; }

	protected virtual void Start()
	{
		if (Object.op_Implicit((Object)(object)ImageMask))
		{
			int num = 47;
			if (Object.op_Implicit((Object)(object)GlassMask))
			{
				((Renderer)GlassMask).material.SetInt(PlayerMaterial.MaskLayer, num);
			}
			((Renderer)ImageMask).material.SetInt(PlayerMaterial.MaskLayer, num);
			if (Object.op_Implicit((Object)(object)HatImage?.FrontLayer))
			{
				((Renderer)HatImage.FrontLayer).material.SetInt(PlayerMaterial.MaskLayer, num);
			}
			if (Object.op_Implicit((Object)(object)HatImage?.BackLayer))
			{
				((Renderer)HatImage.BackLayer).material.SetInt(PlayerMaterial.MaskLayer, num);
			}
			if (Object.op_Implicit((Object)(object)LoadingIndicator))
			{
				LoadingIndicator.SetMaskLayer(num);
			}
		}
		if (Object.op_Implicit((Object)(object)LabelMask))
		{
			int num2 = 46;
			((Renderer)LabelMask).material.SetInt(PlayerMaterial.MaskLayer, num2);
			((Renderer)LabelBackground).material.SetInt(PlayerMaterial.MaskLayer, num2);
			((Renderer)LabelGlass).material.SetInt(PlayerMaterial.MaskLayer, num2);
		}
	}

	public virtual void SetItem(IBuyable product, string productId, string name, string price, bool purchased, CurrencyTypes currency)
	{
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		Product = product;
		Purchased = purchased;
		Name = name;
		Price = price;
		ProductId = productId;
		((Object)this).name = productId;
		((Renderer)PurchasedIcon).enabled = Purchased;
		((Renderer)ImageMask).enabled = false;
		if ((Object)(object)PurchasedOverlay != (Object)null)
		{
			((Renderer)PurchasedOverlay).enabled = Purchased;
		}
		if (!product.LimitedTimeAvailable.HasEndDateAndAvailable() || Purchased)
		{
			((Renderer)LimitedTimeIcon).enabled = false;
		}
		else
		{
			((Renderer)LimitedTimeIcon).enabled = true;
		}
		LimitedTimeText.SetProduct(Product);
		((Component)LimitedTimeText).gameObject.SetActive(((Renderer)LimitedTimeIcon).enabled);
		if (Purchased)
		{
			((TMP_Text)PriceText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Owned);
		}
		else
		{
			switch (currency)
			{
			case CurrencyTypes.Beans:
				((TMP_Text)PriceText).text = "<sprite name=\"UI_Currency_Bean\"> " + Price;
				break;
			case CurrencyTypes.Stars:
				((TMP_Text)PriceText).text = "<sprite name=\"UI_Currency_Star\"> " + Price;
				break;
			}
		}
		if (Product is HatData)
		{
			HatData prod = (HatData)Product;
			AddressableAssetExtensions.LoadAssetAsync<HatViewData>((MonoBehaviour)(object)this, (IAddressableAssetProvider<HatViewData>)prod, (Action<HatViewData>)((HatViewData viewAsset) =>
			{
				//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
				if (!((MonoBehaviour)(object)this).IsDestroyedOrNull() && !((Component)this).gameObject.IsDestroyedOrNull() && !((MonoBehaviour)(object)HatImage).IsDestroyedOrNull())
				{
					HatImage.SetHat(prod, 0);
					HatImage.SetMaskType(PlayerMaterial.MaskType.ScrollingUI);
					((Component)HatImage.FrontLayer).transform.localPosition = new Vector3(0f, 0f, (prod.InFront || (Object.op_Implicit((Object)(object)viewAsset) && Object.op_Implicit((Object)(object)viewAsset.MainImage) && Object.op_Implicit((Object)(object)viewAsset.BackImage))) ? (-0.01f) : 0.01f);
					((Component)HatImage.FrontLayer).transform.localScale = Vector3.one * 0.5f;
				}
			}), (Action)null);
			((Component)HatImage.BackLayer).transform.localPosition = new Vector3(0f, 0f, 0.001f);
			((Component)HatImage.BackLayer).transform.localScale = Vector3.one * 0.5f;
			((TMP_Text)NameText).text = prod.GetItemName();
			SetSquare();
		}
		if (Product is NamePlateData)
		{
			NamePlateData namePlateData = (NamePlateData)Product;
			AddressableAssetExtensions.LoadAssetAsync<NamePlateViewData>((MonoBehaviour)(object)this, (IAddressableAssetProvider<NamePlateViewData>)namePlateData, (Action<NamePlateViewData>)((NamePlateViewData viewAsset) =>
			{
				if (!((MonoBehaviour)(object)this).IsDestroyedOrNull() && !((Component)this).gameObject.IsDestroyedOrNull() && !((Component)CrewHeadImage).gameObject.IsDestroyedOrNull())
				{
					CrewHeadImage.sprite = viewAsset.Image;
				}
			}), (Action)null);
			((TMP_Text)NameText).text = namePlateData.GetItemName();
		}
		else if (Product is SkinData)
		{
			SkinData skinData = (SkinData)Product;
			CrewHeadImage.sprite = MannequinFrame;
			((Component)CrewHeadImage).transform.localPosition = new Vector3(0f, 0.2f, -0.01f);
			((Component)CrewHeadImage).transform.localScale = Vector3.one * 0.5f;
			((Component)HatImage.FrontLayer).transform.localPosition = new Vector3(0f, 0f, -1f);
			((Component)HatImage.FrontLayer).transform.localScale = Vector3.one;
			PlayerControl.SetSkinImageAsync(skinData, 0, HatImage.FrontLayer);
			((TMP_Text)NameText).text = skinData.GetItemName();
			SetSquare();
		}
		else if (Product is VisorData)
		{
			VisorData visorData = (VisorData)Product;
			CrewHeadImage.sprite = MannequinFrame;
			((Component)CrewHeadImage).transform.localPosition = new Vector3(0f, 0.2f, -0.01f);
			((Component)CrewHeadImage).transform.localScale = Vector3.one * 0.5f;
			((Component)VisorLayer).transform.localPosition = new Vector3(0f, 0.59f, -0.1f);
			((Component)VisorLayer).transform.localScale = Vector3.one;
			VisorLayer.SetVisor(visorData.ProdId, 0);
			VisorLayer.SetMaskType(PlayerMaterial.MaskType.ScrollingUI);
			((TMP_Text)NameText).text = visorData.GetItemName();
			SetSquare();
		}
		else if (Product is PetData)
		{
			PetData petData = (PetData)Product;
			((Renderer)CrewHeadImage).enabled = false;
			HatImage.FrontLayer.sprite = petData.SpritePreview;
			((Component)HatImage.FrontLayer).transform.localPosition = new Vector3(0f, 0.35f, -0.01f);
			((Component)HatImage.FrontLayer).transform.localScale = Vector3.one * 0.5f;
			((TMP_Text)NameText).text = petData.GetItemName();
			SetSquare();
		}
		else if (Product is CosmicubeData)
		{
			CosmicubeData cosmicubeData = (CosmicubeData)Product;
			((TMP_Text)NameText).text = cosmicubeData.GetCosmicubeName();
			LoadingIndicator.Toggle(on: true);
			((Behaviour)PassiveButton).enabled = false;
			PriceLabel.SetActive(false);
			((MonoBehaviour)Parent).StartCoroutine(((MonoBehaviour)(object)this).CoLoadAssetAsync<Sprite>(cosmicubeData.StoreImageRef, (Action<Sprite>)((Sprite sprite) =>
			{
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				PriceLabel.SetActive(true);
				((Behaviour)PassiveButton).enabled = true;
				LoadingIndicator.Toggle(on: false);
				((Component)HatImage.FrontLayer).transform.localRotation = Quaternion.identity;
				HatImage.FrontLayer.sprite = sprite;
			})));
			HatImage.FrontLayer.maskInteraction = (SpriteMaskInteraction)0;
			((Component)HatImage.FrontLayer).transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			((Component)HatImage.FrontLayer).transform.localPosition = new Vector3(0f, 0.2f, -0.0001f);
			((Renderer)ImageMask).enabled = true;
			((Renderer)CrewHeadImage).enabled = false;
			SetDoubleBig();
		}
		else if (Product is StarBundle)
		{
			StarBundle starBundle = (StarBundle)Product;
			((TMP_Text)PriceText).text = "$" + starBundle.price;
			((TMP_Text)NameText).text = starBundle.GetItemName();
			HatImage.FrontLayer.sprite = starBundle.StoreImage;
			HatImage.BackLayer.sprite = starBundle.StoreBackImage;
			((Component)HatImage.FrontLayer).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
			((Component)HatImage.FrontLayer).transform.localPosition = new Vector3(0f, 0.39f, -1f);
			HatImage.FrontLayer.maskInteraction = (SpriteMaskInteraction)0;
			((Component)HatImage.BackLayer).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
			((Component)HatImage.BackLayer).transform.localPosition = new Vector3(0f, 0.39f, -0.001f);
			HatImage.BackLayer.maskInteraction = (SpriteMaskInteraction)0;
			((Renderer)ImageMask).enabled = true;
			((Renderer)CrewHeadImage).enabled = false;
			SetDoubleBig();
		}
	}

	private void SetDoubleBig()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PurchasedOverlay != (Object)null)
		{
			PurchasedOverlay.size = new Vector2(3.5f, 4f);
		}
		Background.size = new Vector2(3.5f, 4f);
		((Component)Background).GetComponent<BoxCollider2D>().size = new Vector2(3.5f, 4f);
		SelectionHighlight.size = new Vector2(3.5f, 4f);
		LabelMask.size = new Vector2(3.5f, 2f);
		((Component)GlassSprite).transform.localScale = Vector2.op_Implicit(new Vector2(((Component)GlassSprite).transform.localScale.x * 2f, ((Component)GlassSprite).transform.localScale.y));
		((Component)ImageMask).transform.localScale = new Vector3(0.62f, 0.62f, 1f);
		ImageMask.size = new Vector2(5.41f, 6.15f);
		((Component)ImageMask).transform.localPosition = Vector2.op_Implicit(new Vector2(0f, 0.23f));
		BoxCollider2D val = clickCollider;
		val.size *= 2f;
		((Component)PurchasedIcon).transform.localPosition = new Vector3(1.92f, 1.35f, ((Component)PurchasedIcon).transform.localPosition.z);
		((Component)LimitedTimeIcon).transform.localPosition = new Vector3(1.92f, 1.35f, ((Component)PurchasedIcon).transform.localPosition.z);
		((Component)LimitedTimeText).transform.localPosition = new Vector3(1.68f, 0.85f, ((Component)LimitedTimeText).transform.localPosition.z);
		((Component)LimitedTimeText).transform.localScale = new Vector3(1.1f, 1f, ((Component)LimitedTimeText).transform.localScale.z);
		PriceLabel.transform.localPosition = new Vector3(0f, -0.97f, PriceLabel.transform.localPosition.z);
	}

	private void SetSquare()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)NameText).enabled = false;
		if ((Object)(object)PurchasedOverlay != (Object)null)
		{
			PurchasedOverlay.size = new Vector2(1.75f, 2f);
		}
		Background.size = new Vector2(1.75f, 2f);
		((Component)Background).GetComponent<BoxCollider2D>().size = new Vector2(1.75f, 2f);
		SelectionHighlight.size = new Vector2(1.75f, 2f);
		LabelMask.size = new Vector2(1.75f, 2f);
	}

	public void DoPurchase()
	{
		Parent.ShowConfirm(this);
	}

	private void OnDestroy()
	{
		if (onDestroy != null)
		{
			onDestroy();
			onDestroy = null;
		}
	}
}
