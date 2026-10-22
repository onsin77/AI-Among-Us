using Innersloth.Assets;
using UnityEngine;

public class VisorLayer : MonoBehaviour
{
	private const float BackZLayer = -1.5f;

	private const float FrontZLayer = -3f;

	[SerializeField]
	private SpriteRenderer Image;

	private VisorData currentVisor;

	private AddressableAsset<VisorViewData> viewAsset;

	private PlayerMaterial.Properties matProperties;

	private VisorOptions options;

	public bool IsLoaded => viewAsset?.IsLoaded() ?? false;

	public bool Visible
	{
		set
		{
			((Renderer)Image).enabled = value;
		}
	}

	public float Alpha
	{
		set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			Image.color = Image.color.SetAlpha(value);
		}
	}

	public float ZIndexSpacing { get; set; } = 0.0001f;

	public void SetVisor(string visorId, int colorId)
	{
		if (DestroyableSingleton<HatManager>.InstanceExists)
		{
			SetVisor(DestroyableSingleton<HatManager>.Instance.GetVisorById(visorId), colorId);
		}
	}

	public void SetVisor(VisorData data, int colorId)
	{
		currentVisor = data;
		UnloadAsset();
		viewAsset = data.CreateAddressableAsset();
		((MonoBehaviour)(object)this).LoadAssetAsync(viewAsset, () =>
		{
			if (!((Object)(object)viewAsset.GetAsset() == (Object)null) && !((MonoBehaviour)(object)this).IsDestroyedOrNull() && !((Component)this).gameObject.IsDestroyedOrNull())
			{
				SetVisor(currentVisor, viewAsset.GetAsset(), colorId);
			}
		});
	}

	public void SetVisor(VisorData data, VisorViewData visorView, int colorId)
	{
		currentVisor = data;
		if (data.BehindHats)
		{
			((Component)this).transform.SetLocalZ(ZIndexSpacing * -1.5f);
		}
		else
		{
			((Component)this).transform.SetLocalZ(ZIndexSpacing * -3f);
		}
		SetFlipX(Image.flipX);
		SetMaterialColor(colorId);
	}

	public void SetOptions(VisorOptions options)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		this.options = options;
		((Component)this).transform.localScale = (options.Enabled ? Vector3.one : Vector3.zero);
	}

	public void SetFlipX(bool flipX)
	{
		Image.flipX = flipX;
		if (IsLoaded)
		{
			VisorViewData asset = viewAsset.GetAsset();
			if (flipX && Object.op_Implicit((Object)(object)asset.LeftIdleFrame))
			{
				Image.sprite = asset.LeftIdleFrame;
			}
			else
			{
				Image.sprite = asset.IdleFrame;
			}
		}
	}

	public void SetLocalPlayer(bool localPlayer)
	{
		matProperties.IsLocalPlayer = localPlayer;
		UpdateMaterial();
	}

	public void SetFloorAnim()
	{
		if (IsLoaded)
		{
			Image.sprite = viewAsset.GetAsset().FloorFrame;
		}
	}

	public void SetMaskType(PlayerMaterial.MaskType maskType)
	{
		matProperties.MaskType = maskType;
		UpdateMaterial();
	}

	public void SetMaterialColor(int color)
	{
		matProperties.ColorId = color;
		UpdateMaterial();
	}

	public void SetMaskLayer(int layer)
	{
		matProperties.MaskLayer = layer;
		UpdateMaterial();
	}

	private void UpdateMaterial()
	{
		if (IsLoaded && Object.op_Implicit((Object)(object)viewAsset.GetAsset().AltShader))
		{
			((Renderer)Image).sharedMaterial = viewAsset.GetAsset().AltShader;
		}
		else
		{
			((Renderer)Image).sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
		}
		PlayerMaterial.SetColors(matProperties.ColorId, (Renderer)(object)Image);
		switch (matProperties.MaskType)
		{
		case PlayerMaterial.MaskType.SimpleUI:
		case PlayerMaterial.MaskType.ScrollingUI:
			Image.maskInteraction = (SpriteMaskInteraction)1;
			break;
		case PlayerMaterial.MaskType.Exile:
			Image.maskInteraction = (SpriteMaskInteraction)2;
			break;
		default:
			Image.maskInteraction = (SpriteMaskInteraction)0;
			break;
		}
		if (matProperties.MaskLayer <= 0)
		{
			PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(Image, matProperties.IsLocalPlayer);
		}
		else
		{
			((Renderer)Image).material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
		}
	}

	public void SetVisorColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Image.color = color;
	}

	private void OnDestroy()
	{
		UnloadAsset();
	}

	private void UnloadAsset()
	{
		viewAsset?.Unload();
		viewAsset = null;
	}
}
