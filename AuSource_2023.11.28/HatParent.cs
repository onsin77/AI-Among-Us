using Innersloth.Assets;
using PowerTools;
using UnityEngine;

public class HatParent : MonoBehaviour
{
	public SpriteRenderer BackLayer;

	public SpriteRenderer FrontLayer;

	public SpriteRenderer Parent;

	public SpriteAnimNodeSync SpriteSyncNode;

	private AddressableAsset<HatViewData> hatDataAsset;

	private PlayerMaterial.Properties matProperties;

	private HatOptions options;

	public bool IsLoaded => hatDataAsset.IsLoaded();

	public HatData Hat { get; set; }

	public bool Visible
	{
		set
		{
			((Component)FrontLayer).gameObject.SetActive(value);
			((Component)BackLayer).gameObject.SetActive(value);
		}
	}

	public Color SpriteColor
	{
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			BackLayer.color = value;
			FrontLayer.color = value;
		}
	}

	public bool FlipX
	{
		set
		{
			BackLayer.flipX = value;
			FrontLayer.flipX = value;
		}
	}

	public void SetHat(HatData hat, int color)
	{
		Hat = hat;
		SetHat(color);
	}

	public void SetIdleAnim(int colorId)
	{
		if (Object.op_Implicit((Object)(object)Hat))
		{
			UnloadAsset();
			hatDataAsset = Hat.CreateAddressableAsset();
			((MonoBehaviour)this).StartCoroutine(hatDataAsset.CoLoadAsync(() =>
			{
				PopulateFromHatViewData();
				SetMaterialColor(colorId);
			}));
		}
	}

	public void SetHat(string hatId, int color)
	{
		if (DestroyableSingleton<HatManager>.InstanceExists)
		{
			Hat = DestroyableSingleton<HatManager>.Instance.GetHatById(hatId);
			SetHat(color);
		}
	}

	public void SetFloorAnim()
	{
		((Renderer)BackLayer).enabled = false;
		((Renderer)FrontLayer).enabled = true;
		FrontLayer.sprite = hatDataAsset.GetAsset().FloorImage;
	}

	public void SetClimbAnim()
	{
		if (options.ShowForClimb)
		{
			((Renderer)BackLayer).enabled = false;
			((Renderer)FrontLayer).enabled = true;
			FrontLayer.sprite = hatDataAsset.GetAsset().ClimbImage;
		}
	}

	public void SetLocalPlayer(bool localPlayer)
	{
		matProperties.IsLocalPlayer = localPlayer;
		UpdateMaterial();
	}

	public void SetMaterialColor(int color)
	{
		matProperties.ColorId = color;
		UpdateMaterial();
	}

	public void SetMaskType(PlayerMaterial.MaskType maskType)
	{
		matProperties.MaskType = maskType;
		UpdateMaterial();
	}

	public void SetMaskLayer(int layer)
	{
		matProperties.MaskLayer = layer;
		UpdateMaterial();
	}

	private void OnDestroy()
	{
		UnloadAsset();
	}

	private void UnloadAsset()
	{
		hatDataAsset?.Unload();
		hatDataAsset = null;
	}

	public void SetOptions(HatOptions b)
	{
		options = b;
		options.Initialized = true;
	}

	private void UpdateMaterial()
	{
		HatViewData hatViewData = hatDataAsset?.GetAsset();
		if (Object.op_Implicit((Object)(object)hatViewData) && Object.op_Implicit((Object)(object)hatViewData.AltShader))
		{
			((Renderer)FrontLayer).sharedMaterial = hatViewData.AltShader;
			if (Object.op_Implicit((Object)(object)BackLayer))
			{
				((Renderer)BackLayer).sharedMaterial = hatViewData.AltShader;
			}
		}
		else
		{
			((Renderer)FrontLayer).sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
			if (Object.op_Implicit((Object)(object)BackLayer))
			{
				((Renderer)BackLayer).sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
			}
		}
		int colorId = matProperties.ColorId;
		PlayerMaterial.SetColors(colorId, (Renderer)(object)FrontLayer);
		if (Object.op_Implicit((Object)(object)BackLayer))
		{
			PlayerMaterial.SetColors(colorId, (Renderer)(object)BackLayer);
		}
		((Renderer)FrontLayer).material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
		if (Object.op_Implicit((Object)(object)BackLayer))
		{
			((Renderer)BackLayer).material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
		}
		switch (matProperties.MaskType)
		{
		case PlayerMaterial.MaskType.ScrollingUI:
			if (Object.op_Implicit((Object)(object)FrontLayer))
			{
				FrontLayer.maskInteraction = (SpriteMaskInteraction)1;
			}
			if (Object.op_Implicit((Object)(object)BackLayer))
			{
				BackLayer.maskInteraction = (SpriteMaskInteraction)1;
			}
			break;
		case PlayerMaterial.MaskType.Exile:
			if (Object.op_Implicit((Object)(object)FrontLayer))
			{
				FrontLayer.maskInteraction = (SpriteMaskInteraction)2;
			}
			if (Object.op_Implicit((Object)(object)BackLayer))
			{
				BackLayer.maskInteraction = (SpriteMaskInteraction)2;
			}
			break;
		default:
			if (Object.op_Implicit((Object)(object)FrontLayer))
			{
				FrontLayer.maskInteraction = (SpriteMaskInteraction)0;
			}
			if (Object.op_Implicit((Object)(object)BackLayer))
			{
				BackLayer.maskInteraction = (SpriteMaskInteraction)0;
			}
			break;
		}
		if (matProperties.MaskLayer <= 0)
		{
			PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(FrontLayer, matProperties.IsLocalPlayer);
			if (Object.op_Implicit((Object)(object)BackLayer))
			{
				PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(BackLayer, matProperties.IsLocalPlayer);
			}
		}
	}

	private void PopulateFromHatViewData()
	{
		UpdateMaterial();
		if (hatDataAsset == null)
		{
			return;
		}
		HatViewData asset = hatDataAsset.GetAsset();
		if (Object.op_Implicit((Object)(object)asset))
		{
			SpriteAnimNodeSync spriteAnimNodeSync = SpriteSyncNode ?? ((Component)this).GetComponent<SpriteAnimNodeSync>();
			if (Object.op_Implicit((Object)(object)spriteAnimNodeSync))
			{
				spriteAnimNodeSync.NodeId = (Hat.NoBounce ? 1 : 0);
			}
			if (Hat.InFront)
			{
				((Renderer)BackLayer).enabled = false;
				((Renderer)FrontLayer).enabled = true;
				FrontLayer.sprite = asset.MainImage;
			}
			else if (Object.op_Implicit((Object)(object)asset.BackImage))
			{
				((Renderer)BackLayer).enabled = true;
				((Renderer)FrontLayer).enabled = true;
				BackLayer.sprite = asset.BackImage;
				FrontLayer.sprite = asset.MainImage;
			}
			else
			{
				((Renderer)BackLayer).enabled = true;
				((Renderer)FrontLayer).enabled = false;
				FrontLayer.sprite = null;
				BackLayer.sprite = asset.MainImage;
			}
			if (options.Initialized && HideHat())
			{
				((Renderer)FrontLayer).enabled = false;
				((Renderer)BackLayer).enabled = false;
			}
		}
	}

	public bool HideHat()
	{
		if (!options.VisorBlockingHatsAllowed)
		{
			return Hat.BlocksVisors;
		}
		return false;
	}

	private void LateUpdate()
	{
		if (!Object.op_Implicit((Object)(object)Parent))
		{
			return;
		}
		HatViewData hatViewData = hatDataAsset?.GetAsset();
		if (!Object.op_Implicit((Object)(object)Hat) || !((Object)(object)hatViewData != (Object)null))
		{
			return;
		}
		if ((Object)(object)FrontLayer.sprite != (Object)(object)hatViewData.ClimbImage && (Object)(object)FrontLayer.sprite != (Object)(object)hatViewData.FloorImage)
		{
			if ((Hat.InFront || Object.op_Implicit((Object)(object)hatViewData.BackImage)) && Object.op_Implicit((Object)(object)hatViewData.LeftMainImage))
			{
				FrontLayer.sprite = (Parent.flipX ? hatViewData.LeftMainImage : hatViewData.MainImage);
			}
			if (Object.op_Implicit((Object)(object)hatViewData.BackImage) && Object.op_Implicit((Object)(object)hatViewData.LeftBackImage))
			{
				BackLayer.sprite = (Parent.flipX ? hatViewData.LeftBackImage : hatViewData.BackImage);
			}
			else if (!Object.op_Implicit((Object)(object)hatViewData.BackImage) && !Hat.InFront && Object.op_Implicit((Object)(object)hatViewData.LeftMainImage))
			{
				BackLayer.sprite = (Parent.flipX ? hatViewData.LeftMainImage : hatViewData.MainImage);
			}
		}
		else if ((Object)(object)FrontLayer.sprite == (Object)(object)hatViewData.ClimbImage || (Object)(object)FrontLayer.sprite == (Object)(object)hatViewData.LeftClimbImage)
		{
			SpriteAnimNodeSync spriteAnimNodeSync = SpriteSyncNode ?? ((Component)this).GetComponent<SpriteAnimNodeSync>();
			if (Object.op_Implicit((Object)(object)spriteAnimNodeSync))
			{
				spriteAnimNodeSync.NodeId = 0;
			}
		}
	}

	private void SetHat(int color)
	{
		if (Object.op_Implicit((Object)(object)Hat))
		{
			UnloadAsset();
			hatDataAsset = Hat.CreateAddressableAsset();
			hatDataAsset.LoadAsync(() =>
			{
				PopulateFromHatViewData();
				SetMaterialColor(color);
			});
		}
	}

	public bool HasHat()
	{
		return !Hat.IsEmpty;
	}
}
