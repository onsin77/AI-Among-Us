using Innersloth.Assets;
using UnityEngine;

public class StoreChip : ColorChip
{
	public CosmeticData product;

	public StoreMannequin storeMannequin;

	public SpriteRenderer NameplateConfirmPreview;

	public StoreMenu Parent { get; set; }

	public void SetData(CosmeticData prod)
	{
		product = prod;
		if (prod is NamePlateData)
		{
			((Component)storeMannequin).gameObject.SetActive(false);
			((Component)NameplateConfirmPreview).gameObject.SetActive(true);
			AddressableAsset<NamePlateViewData> viewAsset = (prod as NamePlateData).CreateAddressableAsset();
			((MonoBehaviour)this).StartCoroutine(((MonoBehaviour)(object)this).CoLoadAssetAsync(viewAsset, () =>
			{
				NameplateConfirmPreview.sprite = viewAsset.GetAsset()?.Image;
			}));
		}
		else
		{
			((Component)storeMannequin).gameObject.SetActive(true);
			((Component)NameplateConfirmPreview).gameObject.SetActive(false);
			storeMannequin.WearThing(prod, PlayerMaterial.MaskType.ScrollingUI);
		}
	}
}
