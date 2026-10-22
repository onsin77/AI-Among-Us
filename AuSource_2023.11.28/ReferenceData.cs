using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReferenceData", menuName = "ReferenceData", order = 1)]
public class ReferenceData : ScriptableObject
{
	public TextAsset credits;

	public List<CosmicubeData> cosmicubes;

	public List<StarBundle> starBundles;

	public List<PetData> pets;

	public List<HatData> hats;

	public List<SkinData> skins;

	public List<VisorData> visors;

	public List<NamePlateData> nameplates;

	public List<BundleData> bundles;

	public List<TranslatedImageSet> languages;

	[Header("Store Features")]
	public List<FeaturedItemGroup> FeaturedItemGroups;

	public CosmeticGroups Groups;

	[Header("Store versions. MM/DD/YYYY")]
	public string StoreBundlesVersion = "05/06/2022";

	public string StoreHatsVersion = "05/06/2022";

	public string StoreOutfitsVersion = "05/06/2022";

	public string StorePetsVersion = "05/06/2022";

	public string StoreNameplatesVersion = "05/06/2022";

	public string StoreVisorsVersion = "05/06/2022";

	public string StoreCubesVersion = "05/06/2022";
}
