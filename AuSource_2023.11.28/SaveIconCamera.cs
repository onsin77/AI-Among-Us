using System;
using AmongUs.Data;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SaveIconCamera : DestroyableSingleton<SaveIconCamera>
{
	private Camera cam;

	public ConditionalRenderTexture platformRenderTextures;

	private RenderTexture targetTexture;

	public PlayerControl saveIconDummy;

	public LayerMask layerToRenderWith;

	private int layerIndex = -1;

	private static object lockObject = 1;

	private static volatile bool needsRender = false;

	private static byte[] renderedPNG;

	private new void Awake()
	{
		targetTexture = platformRenderTextures;
	}

	private void Start()
	{
		cam = ((Component)this).GetComponent<Camera>();
		if (Object.op_Implicit((Object)(object)cam))
		{
			((Behaviour)cam).enabled = false;
		}
		((Component)saveIconDummy).gameObject.SetActive(false);
	}

	private void LateUpdate()
	{
		lock (lockObject)
		{
			if (needsRender)
			{
				renderedPNG = RenderSaveIconLocal();
				needsRender = false;
			}
		}
	}

	[ContextMenu("Test Render Icon")]
	private void TestIcon()
	{
		RenderSaveIconLocal();
	}

	public static byte[] RenderSaveIcon()
	{
		lock (lockObject)
		{
			needsRender = true;
		}
		while (needsRender)
		{
			Debug.Log((object)"Waiting for render");
		}
		return renderedPNG;
	}

	private byte[] RenderSaveIconLocal()
	{
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Expected Obj, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		((Component)saveIconDummy).gameObject.SetActive(true);
		cam.targetTexture = targetTexture;
		GameData instance = GameData.Instance;
		GameData.Instance = null;
		try
		{
			saveIconDummy.SetAppearanceFromSaveData();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Error setting appearance from save data\nHat=" + DataManager.Player.Customization.Hat + "\nSkin=" + DataManager.Player.Customization.Skin + "\nVisor=" + DataManager.Player.Customization.Visor + "\nPet=" + DataManager.Player.Customization.Pet + "\nNameplate=" + DataManager.Player.Customization.NamePlate));
			Debug.LogException(ex);
		}
		GameData.Instance = instance;
		if (layerIndex == -1)
		{
			for (int i = 0; i < 32; i++)
			{
				if (((1 << i) & LayerMask.op_Implicit(layerToRenderWith)) != 0)
				{
					layerIndex = i;
					break;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)saveIconDummy.GetPet()))
		{
			cam.cullingMask = LayerMask.op_Implicit(layerToRenderWith);
			PetBehaviour pet = saveIconDummy.GetPet();
			pet.SetCrewmateColor(DataManager.Player.Customization.Color);
			((Behaviour)pet).enabled = false;
			((Component)pet).transform.SetParent(((Component)saveIconDummy).transform);
			((Component)pet).transform.position = ((Component)saveIconDummy).transform.position + new Vector3(-0.5f, 0f, -5f);
			SpriteRenderer[] componentsInChildren = ((Component)saveIconDummy).GetComponentsInChildren<SpriteRenderer>(true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				((Component)componentsInChildren[j]).gameObject.layer = layerIndex;
			}
			componentsInChildren = ((Component)pet).GetComponentsInChildren<SpriteRenderer>(true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				((Component)componentsInChildren[j]).gameObject.layer = layerIndex;
			}
		}
		saveIconDummy.cosmetics.SetNameMask(alive: false);
		cam.Render();
		((Component)saveIconDummy).gameObject.SetActive(false);
		RenderTexture.active = targetTexture;
		Texture2D val = new Texture2D(((Texture)targetTexture).width, ((Texture)targetTexture).height, (TextureFormat)3, false);
		val.ReadPixels(new Rect(0f, 0f, (float)((Texture)targetTexture).width, (float)((Texture)targetTexture).height), 0, 0);
		RenderTexture.active = null;
		return ImageConversion.EncodeToPNG(val);
	}
}
