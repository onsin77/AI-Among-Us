using AmongUs.Data;
using UnityEngine;

public class PlayerCustomization
{
	public static void Equip(CosmeticData cosmetic)
	{
		if (cosmetic is PetData)
		{
			EquipPet(cosmetic as PetData);
		}
		else if (cosmetic is HatData)
		{
			EquipHat(cosmetic as HatData);
		}
		else if (cosmetic is VisorData)
		{
			EquipVisor(cosmetic as VisorData);
		}
		else if (cosmetic is SkinData)
		{
			EquipSkin(cosmetic as SkinData);
		}
		else if (cosmetic is NamePlateData)
		{
			EquipNameplate(cosmetic as NamePlateData);
		}
	}

	public static void EquipPet(PetData pet)
	{
		DataManager.Player.Customization.Pet = pet.ProdId;
		DataManager.Player.Save();
		if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
		{
			PlayerControl.LocalPlayer.RpcSetPet(pet.ProdId);
		}
	}

	public static void EquipSkin(SkinData skin)
	{
		DataManager.Player.Customization.Skin = skin.ProdId;
		if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
		{
			PlayerControl.LocalPlayer.RpcSetSkin(skin.ProdId);
		}
		DataManager.Player.Save();
	}

	public static void EquipHat(HatData hat)
	{
		DataManager.Player.Customization.Hat = hat.ProdId;
		if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
		{
			PlayerControl.LocalPlayer.RpcSetHat(hat.ProdId);
		}
		if (hat.BlocksVisors)
		{
			DataManager.Player.Customization.Visor = "";
			if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				PlayerControl.LocalPlayer.RpcSetVisor("");
			}
		}
		DataManager.Player.Save();
	}

	public static void EquipNameplate(NamePlateData nameplate)
	{
		DataManager.Player.Customization.NamePlate = nameplate.ProdId;
		if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
		{
			PlayerControl.LocalPlayer.RpcSetNamePlate(nameplate.ProdId);
		}
		DataManager.Player.Save();
	}

	public static void EquipVisor(VisorData visor)
	{
		DataManager.Player.Customization.Visor = visor.ProdId;
		if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
		{
			PlayerControl.LocalPlayer.RpcSetVisor(visor.ProdId);
		}
		if (DestroyableSingleton<HatManager>.Instance.GetHatById(((Object)(object)PlayerControl.LocalPlayer != (Object)null) ? PlayerControl.LocalPlayer.Data.DefaultOutfit.HatId : DataManager.Player.Customization.Hat).BlocksVisors)
		{
			DataManager.Player.Customization.Hat = "";
			if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				PlayerControl.LocalPlayer.RpcSetHat("");
			}
		}
		DataManager.Player.Save();
	}

	public static bool IsEquipped(CosmeticData cosmetic)
	{
		if (cosmetic is PetData)
		{
			return DataManager.Player.Customization.Pet == cosmetic.ProdId;
		}
		if (cosmetic is HatData)
		{
			return DataManager.Player.Customization.Hat == cosmetic.ProdId;
		}
		if (cosmetic is VisorData)
		{
			return DataManager.Player.Customization.Visor == cosmetic.ProdId;
		}
		if (cosmetic is SkinData)
		{
			return DataManager.Player.Customization.Skin == cosmetic.ProdId;
		}
		if (cosmetic is NamePlateData)
		{
			return DataManager.Player.Customization.NamePlate == cosmetic.ProdId;
		}
		return false;
	}

	public static void TryUnlockAll()
	{
	}
}
