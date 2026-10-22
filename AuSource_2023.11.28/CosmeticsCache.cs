using System.Collections;
using System.Collections.Generic;
using Innersloth.Assets;
using UnityEngine;

public class CosmeticsCache
{
	private readonly AddressableAssetGroup allCachedAssets = new AddressableAssetGroup();

	private Dictionary<string, AddressableAsset<HatViewData>> hats;

	public bool initialized;

	private Dictionary<string, AddressableAsset<NamePlateViewData>> nameplates;

	private Dictionary<string, AddressableAsset<PetBehaviour>> pets;

	private Dictionary<string, AddressableAsset<SkinViewData>> skins;

	private Dictionary<string, AddressableAsset<VisorViewData>> visors;

	public IEnumerator PopulateFromPlayers()
	{
		HashSet<string> hashSet = new HashSet<string> { "hat_NoHat" };
		HashSet<string> skinIds = new HashSet<string> { "skin_None" };
		HashSet<string> visorIds = new HashSet<string> { "visor_EmptyVisor" };
		HashSet<string> namePlateIds = new HashSet<string> { "nameplate_NoPlate" };
		HashSet<string> petIds = new HashSet<string> { "pet_EmptyPet" };
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			if (!((Object)(object)allPlayerControl == (Object)null) && allPlayerControl.CurrentOutfit != null && !((Object)(object)((Component)allPlayerControl).gameObject == (Object)null))
			{
				GameData.PlayerOutfit currentOutfit = allPlayerControl.CurrentOutfit;
				hashSet.Add(currentOutfit.HatId);
				skinIds.Add(currentOutfit.SkinId);
				visorIds.Add(currentOutfit.VisorId);
				namePlateIds.Add(currentOutfit.NamePlateId);
				petIds.Add(currentOutfit.PetId);
			}
		}
		hashSet.RemoveWhere((string x) => string.IsNullOrWhiteSpace(x));
		skinIds.RemoveWhere((string x) => string.IsNullOrWhiteSpace(x));
		visorIds.RemoveWhere((string x) => string.IsNullOrWhiteSpace(x));
		namePlateIds.RemoveWhere((string x) => string.IsNullOrWhiteSpace(x));
		petIds.RemoveWhere((string x) => string.IsNullOrWhiteSpace(x));
		hats = new Dictionary<string, AddressableAsset<HatViewData>>();
		skins = new Dictionary<string, AddressableAsset<SkinViewData>>();
		nameplates = new Dictionary<string, AddressableAsset<NamePlateViewData>>();
		visors = new Dictionary<string, AddressableAsset<VisorViewData>>();
		pets = new Dictionary<string, AddressableAsset<PetBehaviour>>();
		initialized = true;
		foreach (string item in hashSet)
		{
			yield return CoAddHat(item);
		}
		foreach (string item2 in skinIds)
		{
			yield return CoAddSkin(item2);
		}
		foreach (string item3 in visorIds)
		{
			yield return CoAddVisor(item3);
		}
		foreach (string item4 in petIds)
		{
			yield return CoAddPet(item4);
		}
		foreach (string item5 in namePlateIds)
		{
			yield return CoAddNameplate(item5);
		}
		yield return null;
	}

	public IEnumerator CoAddNameplate(string namePlateId)
	{
		if (!nameplates.ContainsKey(namePlateId))
		{
			NamePlateData namePlateById = DestroyableSingleton<HatManager>.Instance.GetNamePlateById(namePlateId);
			if ((Object)(object)namePlateById != (Object)null)
			{
				AddressableAsset<NamePlateViewData> asset = namePlateById.CreateAddressableAsset();
				allCachedAssets.Add(asset);
				yield return asset.CoLoadAsync();
				nameplates[namePlateId] = asset;
			}
		}
	}

	public IEnumerator CoAddPet(string petId)
	{
		if (!pets.ContainsKey(petId))
		{
			PetData petById = DestroyableSingleton<HatManager>.Instance.GetPetById(petId);
			if ((Object)(object)petById != (Object)null)
			{
				AddressableAsset<PetBehaviour> asset = petById.CreateAddressableAsset();
				allCachedAssets.Add(asset);
				yield return asset.CoLoadAsync();
				pets[petId] = asset;
			}
		}
	}

	public IEnumerator CoAddVisor(string visorId)
	{
		if (!visors.ContainsKey(visorId))
		{
			VisorData visorById = DestroyableSingleton<HatManager>.Instance.GetVisorById(visorId);
			if ((Object)(object)visorById != (Object)null)
			{
				AddressableAsset<VisorViewData> asset = visorById.CreateAddressableAsset();
				allCachedAssets.Add(asset);
				yield return asset.CoLoadAsync();
				visors[visorId] = asset;
			}
		}
	}

	public IEnumerator CoAddSkin(string id)
	{
		if (!skins.ContainsKey(id))
		{
			SkinData skinById = DestroyableSingleton<HatManager>.Instance.GetSkinById(id);
			if ((Object)(object)skinById != (Object)null)
			{
				AddressableAsset<SkinViewData> asset = skinById.CreateAddressableAsset();
				allCachedAssets.Add(asset);
				yield return asset.CoLoadAsync();
				skins[id] = asset;
			}
		}
	}

	public IEnumerator CoAddHat(string id)
	{
		if (!hats.ContainsKey(id))
		{
			HatData hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(id);
			if ((Object)(object)hatById != (Object)null)
			{
				AddressableAsset<HatViewData> asset = hatById.CreateAddressableAsset();
				allCachedAssets.Add(asset);
				yield return asset.CoLoadAsync();
				hats[id] = asset;
			}
		}
	}

	public SkinViewData GetSkin(string id)
	{
		if (!skins.TryGetValue(id, out var value) || value == null)
		{
			Debug.LogWarning((object)("CosmeticsCache did not contain skin " + id));
			value = skins["skin_None"];
		}
		return value.GetAsset();
	}

	public HatViewData GetHat(string id)
	{
		if (!hats.TryGetValue(id, out var value) || value == null)
		{
			Debug.LogWarning((object)("CosmeticsCache did not contain hat " + id));
			value = hats["hat_NoHat"];
		}
		return value.GetAsset();
	}

	public PetBehaviour GetPet(string id)
	{
		if (!pets.TryGetValue(id, out var value) || value == null)
		{
			Debug.LogWarning((object)("CosmeticsCache did not contain pet " + id));
			value = pets["pet_EmptyPet"];
		}
		return value.GetAsset();
	}

	public VisorViewData GetVisor(string id)
	{
		if (!visors.TryGetValue(id, out var value) || value == null)
		{
			Debug.LogWarning((object)("CosmeticsCache did not contain visor " + id));
			value = visors["visor_EmptyVisor"];
		}
		return value.GetAsset();
	}

	public NamePlateViewData GetNameplate(string id)
	{
		if (!nameplates.TryGetValue(id, out var value) || value == null)
		{
			Debug.LogWarning((object)("CosmeticsCache did not contain nameplate " + id));
			value = nameplates["nameplate_NoPlate"];
		}
		return value.GetAsset();
	}

	public void Destroy()
	{
		hats?.Clear();
		skins?.Clear();
		nameplates?.Clear();
		pets?.Clear();
		visors?.Clear();
		allCachedAssets?.Destroy();
	}

	public IEnumerator CoPopulateFromOutfit(GameData.PlayerOutfit outfit)
	{
		yield return CoAddHat(outfit.HatId);
		yield return CoAddVisor(outfit.VisorId);
		yield return CoAddSkin(outfit.SkinId);
		yield return CoAddPet(outfit.PetId);
		yield return CoAddNameplate(outfit.NamePlateId);
	}
}
