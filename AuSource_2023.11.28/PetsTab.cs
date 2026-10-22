using AmongUs.Data;
using UnityEngine;
using UnityEngine.Events;

public class PetsTab : InventoryTab
{
	private string petId;

	private bool currentPetIsEquipped;

	private bool initialized;

	public override void OnEnable()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected Obj, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected Obj, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected Obj, but got Unknown
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Expected Obj, but got Unknown
		base.OnEnable();
		initialized = false;
		PetData[] unlockedPets = DestroyableSingleton<HatManager>.Instance.GetUnlockedPets();
		for (int i = 0; i < unlockedPets.Length; i++)
		{
			PetData pet = unlockedPets[i];
			float num = XRange.Lerp((float)(i % NumPerRow) / ((float)NumPerRow - 1f));
			float num2 = YStart - (float)(i / NumPerRow) * YOffset;
			ColorChip chip = Object.Instantiate<ColorChip>(ColorTabPrefab, scroller.Inner);
			((Component)chip).transform.localPosition = new Vector3(num, num2, -1f);
			chip.PlayerEquippedForeground.SetActive(pet.ProdId == DataManager.Player.Customization.Pet);
			((Component)chip.SelectionHighlight).gameObject.SetActive(false);
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
			{
				chip.Button.OnMouseOver.AddListener((UnityAction)(() =>
				{
					SelectPet(chip, pet);
				}));
				chip.Button.OnMouseOut.AddListener((UnityAction)(() =>
				{
					SelectPet(chip, DestroyableSingleton<HatManager>.Instance.GetPetById(DataManager.Player.Customization.Pet));
				}));
				((UnityEvent)chip.Button.OnClick).AddListener((UnityAction)(() =>
				{
					ClickEquip();
				}));
			}
			else
			{
				((UnityEvent)chip.Button.OnClick).AddListener((UnityAction)(() =>
				{
					SelectPet(chip, pet);
				}));
			}
			chip.Button.ClickMask = scroller.Hitbox;
			chip.Tag = pet;
			UpdateMaterials(chip.Inner.FrontLayer, pet);
			pet.SetPreview(chip.Inner.FrontLayer, HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
			ColorChips.Add(chip);
		}
		petId = DataManager.Player.Customization.Pet;
		currentPetIsEquipped = true;
		SetScrollerBounds();
		initialized = true;
	}

	public void Update()
	{
		PetData petById = DestroyableSingleton<HatManager>.Instance.GetPetById(DataManager.Player.Customization.Pet);
		PetData petById2 = DestroyableSingleton<HatManager>.Instance.GetPetById(petId);
		currentPetIsEquipped = DataManager.Player.Customization.Pet == petId;
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			colorChip.PlayerEquippedForeground.SetActive((Object)(object)petById == (Object)(object)(PetData)colorChip.Tag);
			((Component)colorChip.SelectionHighlight).gameObject.SetActive((Object)(object)petById2 == (Object)(object)(PetData)colorChip.Tag);
		}
	}

	private void SelectPet(ColorChip sender, PetData pet)
	{
		if (initialized)
		{
			petId = pet.ProdId;
			int colorId = (HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
			PlayerPreview.SetPetIdle(pet, colorId);
			PlayerCustomizationMenu.Instance.SetItemName(pet.GetItemName());
		}
	}

	public override void ClickEquip()
	{
		base.ClickEquip();
		DataManager.Player.Customization.Pet = petId;
		if (HasLocalPlayer())
		{
			PlayerControl.LocalPlayer.RpcSetPet(petId);
		}
		Logger.GlobalInstance.Info("PlayerCustomizationMenu::Equipped pet " + petId);
	}

	public override ColorChip GetDefaultSelectable()
	{
		PetData[] unlockedPets = DestroyableSingleton<HatManager>.Instance.GetUnlockedPets();
		for (int i = 0; i < ColorChips.Count; i++)
		{
			PetData petData = unlockedPets[0];
			ColorChip result = ColorChips[i];
			if (petData.ProdId == DataManager.Player.Customization.Pet)
			{
				PlayerCustomizationMenu.Instance.SetItemName(petData.GetItemName());
				return result;
			}
		}
		return ColorChips[0];
	}

	public override bool IsSelectedItemEquipped()
	{
		return currentPetIsEquipped;
	}
}
