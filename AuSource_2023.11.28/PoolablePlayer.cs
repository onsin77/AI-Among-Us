using System;
using AmongUs.Data;
using PowerTools;
using UnityEngine;

public class PoolablePlayer : MonoBehaviour
{
	public SpriteRenderer[] Hands;

	public SpriteRenderer[] OtherBodySprites;

	[SerializeField]
	private CosmeticsLayer cosmetics;

	private PlayerBodyTypes bodyType;

	public string ColorBlindName => cosmetics.GetColorBlindText();

	public PlayerBodyTypes CurrentBodyType => cosmetics.CurrentBodyType;

	public int ColorId => cosmetics.ColorId;

	public void SetBodyCosmeticsVisible(bool b)
	{
		cosmetics.SetBodyCosmeticsVisible(b);
	}

	public void ToggleHat(bool b)
	{
		cosmetics.ToggleHat(b);
	}

	public void SetCustomHatPosition(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetCustomHatPosition(pos);
	}

	public void SetCustomVisorPosition(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetCustomVisorPosition(pos);
	}

	public bool HasHat()
	{
		return cosmetics.HasHat();
	}

	public SpriteRenderer GetHandHat()
	{
		return cosmetics.HatHand;
	}

	private void OnEnable()
	{
		DataManager.Settings.Accessibility.OnColorBlindModeChanged += SetColorBlindTag;
	}

	private void OnDisable()
	{
		DataManager.Settings.Accessibility.OnColorBlindModeChanged -= SetColorBlindTag;
	}

	private void SetColorBlindTag()
	{
		cosmetics.SetEnabledColorblind(DataManager.Settings.Accessibility.ColorBlindMode);
	}

	public void ResetCosmetics()
	{
		cosmetics.ResetCosmetics();
	}

	public SpriteAnim GetSkinSpriteAnim()
	{
		return cosmetics.GetSkinSpriteAnim();
	}

	public void SetMaskLayer(int layer)
	{
		cosmetics.SetMaskLayer(layer);
	}

	public void SetHatAndVisorOnFloor()
	{
		cosmetics.SetHatAndVisorOnFloor();
	}

	public void FlipHatAndVisor()
	{
		cosmetics.SetHatAndVisorOnFloor();
	}

	public void SetVisor(string visorId, int color)
	{
		cosmetics.SetVisor(visorId, color);
	}

	public void SetVisor(VisorData visorData, int color)
	{
		cosmetics.SetVisor(visorData, color);
	}

	public void SetHatWithoutChangingColor(HatData hatData)
	{
		cosmetics.SetHatWithoutChangingColor(hatData);
	}

	public void SetHat(string hatId, int color)
	{
		cosmetics.SetHat(hatId, color);
	}

	public void SetHat(HatData hatData, int color)
	{
		cosmetics.SetHat(hatData, color);
	}

	public void SetNameScale(Vector3 scale)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetNameScale(scale);
	}

	public void SetName(string nameStr, Vector3 scale, Color color, float zPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetName(nameStr, scale, color, zPosition);
	}

	public void SetName(string nameStr)
	{
		cosmetics.SetName(nameStr);
	}

	public void SetNameColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetNameColor(color);
	}

	public void SetNamePosition(Vector3 newPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetNamePosition(newPos);
	}

	public void SetBodyAsGhost()
	{
		cosmetics.SetBodyAsGhost();
	}

	public void SetBodyColor(int color)
	{
		cosmetics.SetBodyColor(color);
	}

	public void FixSkinSprite(Sprite skinSprite)
	{
		cosmetics.FixSkinSprite(skinSprite);
	}

	public void SetSkin(string skinId, int color)
	{
		cosmetics.SetSkin(skinId, color);
	}

	public void SetSkin(SkinData skin, int color)
	{
		cosmetics.SetSkin(skin, color);
	}

	public void ReplacePetParent(Transform newParent, Vector3 newPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.ReplacePetParent(newParent, newPosition);
	}

	public void ToggleName(bool active)
	{
		cosmetics.ToggleName(active);
	}

	public void TogglePet(bool active)
	{
		cosmetics.TogglePet(active);
	}

	public void SetPetColor(int color)
	{
		cosmetics.GetPet()?.SetCrewmateColor(color);
	}

	public void SetPetIdle(string petId, int colorId)
	{
		SetPetIdle(DestroyableSingleton<HatManager>.Instance.GetPetById(petId), colorId);
	}

	public void SetPetIdle(PetData petData, int colorId)
	{
		cosmetics.SetPetIdle(petData, colorId);
	}

	public void SetPetIdle(PetBehaviour petBehaviour, int colorId)
	{
		cosmetics.SetPetIdle(petBehaviour, colorId);
	}

	public void SetPetScared(PetBehaviour petBehaviour, int colorId)
	{
		cosmetics.SetPetScared(petBehaviour, colorId);
	}

	public void SetFlipX(bool flipped)
	{
		cosmetics.SetFlipX(flipped);
	}

	public void SetDeadFlipX(bool flipped)
	{
		cosmetics.SetDeadFlipX(flipped);
	}

	public void SetPetFlipX(bool flipped)
	{
		cosmetics.SetPetFlipX(flipped);
	}

	public void SetBodyType(PlayerBodyTypes bodyType)
	{
		cosmetics.EnsureInitialized(bodyType);
	}

	public void UpdateFromLocalPlayer(PlayerMaterial.MaskType maskType)
	{
		UpdateFromEitherPlayerDataOrCache(PlayerControl.LocalPlayer.Data, PlayerOutfitType.Default, maskType, includePet: true);
	}

	public void UpdateFromDataManager(PlayerMaterial.MaskType maskType)
	{
		InitBody();
		cosmetics.SetMaskType(maskType);
		cosmetics.SetBodyColor(DataManager.Player.Customization.Color);
		cosmetics.SetSkin(DataManager.Player.Customization.Skin, DataManager.Player.Customization.Color);
		cosmetics.SetPetIdle(DestroyableSingleton<HatManager>.Instance.GetPetById(DataManager.Player.Customization.Pet), DataManager.Player.Customization.Color);
		cosmetics.SetHat(DataManager.Player.Customization.Hat, DataManager.Player.Customization.Color);
		cosmetics.SetVisor(DataManager.Player.Customization.Visor, DataManager.Player.Customization.Color);
		SetColorBlindTag();
	}

	public void UpdateFromEitherPlayerDataOrCache(GameData.PlayerInfo pData, PlayerOutfitType outfitType, PlayerMaterial.MaskType maskType, bool includePet, Action OnSkinLoaded = null)
	{
		UpdateFromPlayerData(pData, outfitType, maskType, includePet, OnSkinLoaded);
	}

	public void UpdateFromPlayerData(GameData.PlayerInfo pData, PlayerOutfitType outfitType, PlayerMaterial.MaskType maskType, bool includePet, Action OnSkinLoaded = null, bool forceAlive = false)
	{
		GameData.PlayerOutfit outfit = pData.Outfits[outfitType];
		UpdateFromPlayerOutfit(outfit, maskType, pData.IsDead, includePet, OnSkinLoaded, forceAlive);
	}

	public void UpdateFromPlayerOutfit(GameData.PlayerOutfit outfit, PlayerMaterial.MaskType maskType, bool isDead, bool includePet, Action OnSkinLoaded = null, bool forceAlive = false)
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetMaskType(maskType);
		cosmetics.SetBodyColor(outfit.ColorId);
		Hands.ForEach((SpriteRenderer b) =>
		{
			((Renderer)b).sharedMaterial = CosmeticsLayer.GetBodyMaterial(maskType);
			PlayerMaterial.SetColors(outfit.ColorId, (Renderer)(object)b);
		});
		OtherBodySprites.ForEach((SpriteRenderer b) =>
		{
			((Renderer)b).sharedMaterial = CosmeticsLayer.GetBodyMaterial(maskType);
			PlayerMaterial.SetColors(outfit.ColorId, (Renderer)(object)b);
		});
		if (isDead && !forceAlive)
		{
			cosmetics.SetSkin(string.Empty, outfit.ColorId, OnSkinLoaded);
			cosmetics.SetHatColor(Palette.HalfWhite);
			cosmetics.SetVisorAlpha(Palette.HalfWhite.a);
		}
		else
		{
			cosmetics.SetSkin(outfit.SkinId, outfit.ColorId, OnSkinLoaded);
			cosmetics.SetHatColor(Palette.White);
			cosmetics.SetVisorAlpha(Palette.White.a);
		}
		cosmetics.SetHat(outfit.HatId, outfit.ColorId);
		if (includePet)
		{
			cosmetics.SetPetIdle(DestroyableSingleton<HatManager>.Instance.GetPetById(outfit.PetId), outfit.ColorId);
		}
		cosmetics.SetVisor(outfit.VisorId, outfit.ColorId);
		SetColorBlindTag();
	}

	private void UpdateFromPlayerCache(GameData.PlayerInfo pData, PlayerOutfitType outfitType, PlayerMaterial.MaskType maskType, bool includePet)
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		bool isDead = pData.IsDead;
		GameData.PlayerOutfit outfit = pData.Outfits[outfitType];
		cosmetics.SetMaskType(maskType);
		cosmetics.SetBodyColor(outfit.ColorId);
		Hands.ForEach((SpriteRenderer b) =>
		{
			((Renderer)b).sharedMaterial = CosmeticsLayer.GetBodyMaterial(maskType);
			PlayerMaterial.SetColors(outfit.ColorId, (Renderer)(object)b);
		});
		if (isDead)
		{
			SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin("skin_None");
			cosmetics.SetSkin(skin, outfit.ColorId);
			cosmetics.SetHatColor(Palette.HalfWhite);
			cosmetics.SetVisorAlpha(Palette.HalfWhite.a);
		}
		else
		{
			SkinViewData skin2 = ShipStatus.Instance.CosmeticsCache.GetSkin(outfit.SkinId);
			cosmetics.SetSkin(skin2, outfit.ColorId);
			cosmetics.SetHatColor(Palette.White);
			cosmetics.SetVisorAlpha(Palette.White.a);
		}
		ShipStatus.Instance.CosmeticsCache.GetHat(outfit.HatId);
		cosmetics.SetHat(DestroyableSingleton<HatManager>.Instance.GetHatById(outfit.HatId), outfit.ColorId);
		if (includePet)
		{
			PetBehaviour pet = ShipStatus.Instance.CosmeticsCache.GetPet(outfit.PetId);
			cosmetics.SetPetIdle(pet, outfit.ColorId);
		}
		VisorViewData visor = ShipStatus.Instance.CosmeticsCache.GetVisor(outfit.VisorId);
		cosmetics.SetVisor(DestroyableSingleton<HatManager>.Instance.GetVisorById(outfit.VisorId), visor, outfit.ColorId);
		SetColorBlindTag();
	}

	private void Awake()
	{
		InitBody();
		if (bodyType != PlayerBodyTypes.Normal && !cosmetics.AlwaysDrawNormalPlayer)
		{
			Hands.ForEach((SpriteRenderer h) =>
			{
				((Renderer)h).enabled = false;
			});
		}
	}

	private void InitBody()
	{
		bodyType = (Constants.ShouldHorseAround() ? PlayerBodyTypes.Horse : PlayerBodyTypes.Normal);
		cosmetics.EnsureInitialized(bodyType);
	}

	public void HideHatAndVisor_AnimEvent()
	{
		cosmetics.HideHatAndVisor();
	}

	public void FadeBlackAll(float duration)
	{
		cosmetics.FadeBlackCosmetics(duration);
	}
}
