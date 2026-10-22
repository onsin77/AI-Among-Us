using Innersloth.Assets;
using TMPro;
using UnityEngine;

public class CubeChip : ColorChip
{
	[SerializeField]
	private SpriteRenderer image;

	[SerializeField]
	private TextMeshPro cubeTitle;

	[SerializeField]
	private TextMeshPro percentCompleteText;

	[SerializeField]
	private TextMeshPro podAmount;

	[SerializeField]
	private SpriteRenderer ImageMask;

	[SerializeField]
	private SpriteRenderer podIcon;

	private CosmicubeData cube;

	private AddressableSpriteBehavior imageAssetHandler;

	private AddressableSpriteBehavior podIconAssetHandler;

	private void Awake()
	{
		imageAssetHandler = AddressableSpriteBehavior.AddToSpriteRenderer(image, null);
		podIconAssetHandler = AddressableSpriteBehavior.AddToSpriteRenderer(podIcon, null);
	}

	public void SetUp(CosmicubeData cube, int maskLayer)
	{
		this.cube = cube;
		imageAssetHandler.ReplaceAsset(new AddressableAsset<Sprite>(cube.StoreImageRef));
		((TMP_Text)cubeTitle).text = cube.GetCosmicubeName();
		ProductId = cube.ProdId;
		((Renderer)ImageMask).material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		((Renderer)image).material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		float completionProgress = DestroyableSingleton<CosmicubeManager>.Instance.GetCompletionProgress(cube);
		DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CosmicubeProgression, (int)(completionProgress * 100f));
		((TMP_Text)percentCompleteText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CosmicubeProgression, (int)(completionProgress * 100f));
		podIconAssetHandler.ReplaceAsset(new AddressableAsset<Sprite>(cube.podIconRef));
		RefreshText();
	}

	private void RefreshText()
	{
		if (!((Object)(object)cube == (Object)null))
		{
			float completionProgress = DestroyableSingleton<CosmicubeManager>.Instance.GetCompletionProgress(cube);
			DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CosmicubeProgression, (int)(completionProgress * 100f));
			((TMP_Text)percentCompleteText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CosmicubeProgression, (int)(completionProgress * 100f));
			((TMP_Text)podAmount).text = DestroyableSingleton<InventoryManager>.Instance.GetPodCount(cube.podId).ToString();
		}
	}

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
		RefreshText();
	}
}
