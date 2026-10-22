using TMPro;
using UnityEngine;

public class RedeemCosmeticPopup : MonoBehaviour
{
	public TextMeshPro cosmeticText;

	public StoreMannequin storeMannequin;

	public void SetRedeemablePopup(CosmeticData cosmetic)
	{
		((TMP_Text)cosmeticText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RedeemPopup, cosmetic.GetItemName());
		storeMannequin.WearThing(cosmetic, PlayerMaterial.MaskType.None, cosmetic.redeemPopUpColor);
	}
}
