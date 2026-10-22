using System;
using System.Collections;
using System.Collections.Generic;
using Innersloth.Assets;
using UnityEngine;

public class CosmeticData : ScriptableObject, IBuyable
{
	public const string TranslationPrefix = "Cosmetic.";

	public List<Platforms> unlockOnSelectPlatforms;

	public bool freeRedeemableCosmetic;

	public int redeemPopUpColor = -1;

	public string epicId = "";

	public string BundleId;

	public string ProductId;

	public Vector2 ChipOffset;

	public int beanCost;

	public int starCost;

	public bool paidOnMobile;

	public LimitedTimeStartEnd limitedTime;

	public int displayOrder;

	public bool NotInStore;

	public bool Free;

	public Sprite SpritePreview;

	public bool PreviewCrewmateColor;

	public string ProdId => ProductId;

	public int BeanCost => beanCost;

	public int StarCost => starCost;

	public string EpicId => epicId;

	public bool PaidOnMobile => paidOnMobile;

	public LimitedTimeStartEnd LimitedTimeAvailable => limitedTime;

	public virtual void PreviewOnPlayer(PoolablePlayer p, int colorId)
	{
		p.ResetCosmetics();
	}

	public string GetItemName()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString("Cosmetic." + ProductId, ((Object)this).name);
	}

	public virtual string GetItemCategory()
	{
		return string.Empty;
	}

	public virtual void SetProdId()
	{
	}

	public virtual IEnumerator CoLoadIcon(Action<Sprite, AddressableAsset> onLoaded)
	{
		yield return null;
		onLoaded(null, null);
	}

	public void SetPreview(SpriteRenderer renderer, int color)
	{
		renderer.sprite = SpritePreview;
		if (Application.isPlaying)
		{
			PlayerMaterial.SetColors(color, (Renderer)(object)renderer);
		}
	}
}
