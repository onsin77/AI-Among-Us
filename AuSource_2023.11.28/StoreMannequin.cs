using UnityEngine;

public class StoreMannequin : MonoBehaviour
{
	[Header("Cosmetic Slots")]
	[SerializeField]
	private HatParent hat;

	[SerializeField]
	private VisorLayer visor;

	[SerializeField]
	private SpriteRenderer skin;

	[SerializeField]
	private SpriteRenderer pet;

	[SerializeField]
	[Header("Materials")]
	private Material defaultMaterial;

	[SerializeField]
	private Material playerMaterial;

	public void WearThing(IBuyable product, PlayerMaterial.MaskType maskType, int outfitColorOverride = -1)
	{
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Expected Obj, but got Unknown
		int num = ((outfitColorOverride >= 0) ? outfitColorOverride : 2);
		if (product is HatData hatData)
		{
			((Component)hat).gameObject.SetActive(true);
			((Component)visor).gameObject.SetActive(false);
			((Component)skin).gameObject.SetActive(false);
			((Component)pet).gameObject.SetActive(false);
			hat.SetHat(hatData, num);
			hat.SetMaskType(maskType);
		}
		else if (product is VisorData data)
		{
			((Component)hat).gameObject.SetActive(false);
			((Component)visor).gameObject.SetActive(true);
			((Component)skin).gameObject.SetActive(false);
			((Component)pet).gameObject.SetActive(false);
			visor.SetVisor(data, num);
			visor.SetMaskType(maskType);
		}
		else if (product is SkinData skinData)
		{
			((Component)hat).gameObject.SetActive(false);
			((Component)visor).gameObject.SetActive(false);
			((Component)skin).gameObject.SetActive(true);
			((Component)pet).gameObject.SetActive(false);
			PlayerControl.SetSkinImageAsync(skinData, num, skin);
		}
		else if (product is PetData petData)
		{
			((Component)hat).gameObject.SetActive(false);
			((Component)visor).gameObject.SetActive(false);
			((Component)skin).gameObject.SetActive(false);
			((Component)pet).gameObject.SetActive(true);
			pet.sprite = petData.SpritePreview;
			((Renderer)pet).material = new Material(petData.PreviewCrewmateColor ? playerMaterial : defaultMaterial);
			PlayerMaterial.SetColors(num, (Renderer)(object)pet);
		}
	}
}
