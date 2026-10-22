using System;
using System.Linq;
using AmongUs.Data;
using Innersloth.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CubesTab : InventoryTab
{
	public CosmicubeMenu cosmicubeMenu;

	public GameObject viewButton;

	public PassiveButton activateButton;

	public TextMeshPro activateText;

	public TextMeshPro itemName;

	public SpriteRenderer podIcon;

	private CosmicubeData currentCube;

	private void Awake()
	{
		NumRowsVisible = 2;
	}

	public override void OnEnable()
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected Obj, but got Unknown
		base.OnEnable();
		((Component)PlayerPreview).gameObject.SetActive(false);
		CosmicubeData[] unlockedCubes = DestroyableSingleton<CosmicubeManager>.Instance.GetUnlockedCubes();
		unlockedCubes = (from x in unlockedCubes
			orderby x.ProdId == DataManager.Player.Store.ActiveCosmicube descending, DestroyableSingleton<CosmicubeManager>.Instance.IsCompleted(x), DestroyableSingleton<CosmicubeManager>.Instance.GetCompletionProgress(x) descending, x.GetDateCreated() descending
			select x).ToArray();
		for (int num = 0; num < unlockedCubes.Length; num++)
		{
			CosmicubeData cube = unlockedCubes[num];
			float num2 = YStart - (float)(num / NumPerRow) * YOffset;
			ColorChip chip = Object.Instantiate<ColorChip>(ColorTabPrefab, scroller.Inner);
			((Component)chip).transform.localPosition = new Vector3(-0.29f, num2, -1f);
			chip.Button.ClickMask = scroller.Hitbox;
			((UnityEvent)chip.Button.OnClick).AddListener((UnityAction)(() =>
			{
				SelectCube(chip, cube);
			}));
			(chip as CubeChip).SetUp(cube, 16 + num);
			ColorChips.Add(chip);
		}
		if (unlockedCubes.Length != 0)
		{
			GetDefaultSelectable().PlayerEquippedForeground.SetActive(true);
			SelectCube(ColorChips[0], unlockedCubes[0]);
		}
		SetScrollerBounds();
	}

	private void Update()
	{
		viewButton.SetActive((Object)(object)currentCube != (Object)null);
		((Component)activateButton).gameObject.SetActive((Object)(object)currentCube != (Object)null);
		CosmicubeData cubeDataByID = DestroyableSingleton<CosmicubeManager>.Instance.GetCubeDataByID(DataManager.Player.Store.ActiveCosmicube);
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			colorChip.PlayerEquippedForeground.SetActive(cubeDataByID.ProdId == (colorChip as CubeChip).ProductId);
			((Component)colorChip.SelectionHighlight).gameObject.SetActive(currentCube.ProdId == (colorChip as CubeChip).ProductId);
		}
	}

	private void SelectCube(ColorChip chip, CosmicubeData cube)
	{
		currentCube = cube;
		bool flag = cube.ProdId == DataManager.Player.Store.ActiveCosmicube;
		((TMP_Text)activateText).text = (flag ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Deactivate) : DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Activate));
		((TMP_Text)itemName).text = currentCube.GetCosmicubeName();
		((MonoBehaviour)this).StartCoroutine(((MonoBehaviour)(object)this).CoLoadAssetAsync<Sprite>(cube.podIconRef, (Action<Sprite>)((Sprite sprite) =>
		{
			podIcon.sprite = sprite;
		})));
		activateButton.SetButtonEnableState(!DestroyableSingleton<CosmicubeManager>.Instance.IsCompleted(cube) | flag);
		for (int num = 0; num < ColorChips.Count; num++)
		{
			ColorChip colorChip = ColorChips[num];
			colorChip.PlayerEquippedForeground.SetActive((Object)(object)chip == (Object)(object)colorChip);
		}
	}

	public override ColorChip GetDefaultSelectable()
	{
		if (ColorChips.Count > 0)
		{
			return ColorChips[0];
		}
		return null;
	}

	public void ViewCube()
	{
		cosmicubeMenu.Show(currentCube.ProdId);
		((Component)this).gameObject.SetActive(false);
	}

	public override void ClickEquip()
	{
		if (currentCube.ProdId == DataManager.Player.Store.ActiveCosmicube)
		{
			DeactivateCube();
		}
		else
		{
			ActivateCube();
		}
	}

	public override bool IsSelectedItemEquipped()
	{
		return true;
	}

	private void ActivateCube()
	{
		CosmicubeData cubeDataByID = DestroyableSingleton<CosmicubeManager>.Instance.GetCubeDataByID(currentCube.ProdId);
		if (!DestroyableSingleton<CosmicubeManager>.Instance.IsCompleted(cubeDataByID))
		{
			DataManager.Player.Store.ActiveCosmicube = currentCube.ProdId;
			PlayerStorageManager.CloudPlayerPrefs playerPrefs = DestroyableSingleton<PlayerStorageManager>.Instance.PlayerPrefs;
			playerPrefs.ActivePodType = currentCube.podId;
			DestroyableSingleton<PlayerStorageManager>.Instance.SaveChanges(playerPrefs);
			if (AmongUsClient.Instance.AmConnected)
			{
				AmongUsClient.Instance.SetActivePodType(playerPrefs.ActivePodType);
			}
			((TMP_Text)activateText).text = ((currentCube.ProdId == DataManager.Player.Store.ActiveCosmicube) ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Deactivate) : DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Activate));
			Logger.GlobalInstance.Info("PlayerCustomizationMenu::Activated cube " + currentCube.ProdId);
		}
	}

	private void DeactivateCube()
	{
		CosmicubeData cubeDataByID = DestroyableSingleton<CosmicubeManager>.Instance.GetCubeDataByID(DataManager.Player.Store.ActiveCosmicube);
		activateButton.SetButtonEnableState(!DestroyableSingleton<CosmicubeManager>.Instance.IsCompleted(cubeDataByID));
		DataManager.Player.Store.ActiveCosmicube = "";
		DataManager.Player.Save();
		PlayerStorageManager.CloudPlayerPrefs playerPrefs = DestroyableSingleton<PlayerStorageManager>.Instance.PlayerPrefs;
		playerPrefs.ActivePodType = "";
		DestroyableSingleton<PlayerStorageManager>.Instance.SaveChanges(playerPrefs);
		if (AmongUsClient.Instance.AmConnected)
		{
			AmongUsClient.Instance.SetActivePodType(playerPrefs.ActivePodType);
		}
		((TMP_Text)activateText).text = ((currentCube.ProdId == DataManager.Player.Store.ActiveCosmicube) ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Deactivate) : DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Activate));
		Logger.GlobalInstance.Info("PlayerCustomizationMenu::Deactivated cube " + currentCube.ProdId);
	}
}
