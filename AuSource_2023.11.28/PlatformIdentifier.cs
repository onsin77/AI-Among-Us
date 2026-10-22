using InnerNet;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlatformIdentifier : MonoBehaviour
{
	public SpriteRenderer iconRenderer;

	public TextMeshPro platformIDText;

	public bool isOnHUD;

	public Sprite genericConsole;

	public Sprite mobile;

	public Sprite pc;

	public Sprite nSwitch;

	public Sprite ps4;

	public Sprite xbox;

	public bool Masked;

	private bool Visible
	{
		set
		{
			((Component)this).gameObject.SetActive(value);
		}
	}

	private bool UsernameVisible
	{
		set
		{
			((Component)platformIDText).gameObject.SetActive(value);
		}
	}

	public static bool SupportsPlatformUsername(Platforms platformTypeID)
	{
		if ((uint)(platformTypeID - 8) <= 2u)
		{
			return true;
		}
		return false;
	}

	public void Awake()
	{
		((Renderer)iconRenderer).enabled = false;
		if (Masked)
		{
			((TMP_Text)platformIDText).fontMaterial.SetFloat("_StencilComp", 3f);
			((TMP_Text)platformIDText).fontMaterial.SetFloat("_Stencil", 51f);
		}
	}

	public void SetInfo(ClientData clientData)
	{
		if (clientData == null)
		{
			Visible = false;
		}
		else
		{
			SetInfo(clientData.PlatformData.Platform, clientData.PlatformData.PlatformName);
		}
	}

	public void SetInfo(GameListing gameListingData)
	{
		SetInfo(gameListingData.Platform, gameListingData.HostPlatformName);
	}

	public void SetInfo(Platforms platform, string platformName)
	{
		Visible = true;
		bool flag = Constants.GetPlatformType() == platform;
		if ((SupportsPlatformUsername(platform) & flag) && !string.IsNullOrEmpty(platformName))
		{
			UsernameVisible = true;
			((TMP_Text)platformIDText).text = platformName;
		}
		else
		{
			UsernameVisible = false;
		}
		SetIcon(platform);
	}

	private void SetIcon(Platforms platform)
	{
		((Renderer)iconRenderer).enabled = true;
		Sprite val = ValidateIcon(GetPrevalidationIcon(platform));
		if (Object.op_Implicit((Object)(object)val))
		{
			iconRenderer.sprite = val;
		}
		else
		{
			((Renderer)iconRenderer).enabled = false;
		}
		if (!isOnHUD)
		{
			UpdatePosition();
		}
	}

	[ContextMenu("Update pos")]
	public void UpdatePosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)iconRenderer).transform.localPosition;
		Bounds val;
		Rect rect;
		if ((int)((TMP_Text)platformIDText).horizontalAlignment == 1)
		{
			val = ((TMP_Text)platformIDText).textBounds;
			localPosition.y = val.center.y;
			val = ((TMP_Text)platformIDText).bounds;
			float num = val.min.x * platformIDText.transform.localScale.x;
			float num2 = 0.5f * ((Component)iconRenderer).transform.localScale.x;
			rect = iconRenderer.sprite.rect;
			localPosition.x = num - num2 * rect.width / iconRenderer.sprite.pixelsPerUnit;
		}
		else
		{
			Vector2 renderedValues = ((TMP_Text)platformIDText).GetRenderedValues(true);
			val = ((TMP_Text)platformIDText).textBounds;
			localPosition.y = val.center.y;
			float num3 = renderedValues.x * -0.5f;
			float num4 = 0.5f * ((Component)iconRenderer).transform.localScale.x;
			rect = iconRenderer.sprite.rect;
			localPosition.x = num3 - num4 * rect.width / iconRenderer.sprite.pixelsPerUnit;
		}
		((Component)iconRenderer).transform.localPosition = localPosition;
	}

	private Sprite GetPrevalidationIcon(Platforms platform)
	{
		switch (platform)
		{
		default:
			return pc;
		case Platforms.IPhone:
		case Platforms.Android:
			return mobile;
		case Platforms.Xbox:
			return xbox;
		case Platforms.Switch:
			return nSwitch;
		case Platforms.Playstation:
			return ps4;
		}
	}

	private Sprite ValidateIcon(Sprite icon)
	{
		if ((Object)(object)icon == (Object)(object)ps4 || (Object)(object)icon == (Object)(object)xbox || (Object)(object)icon == (Object)(object)nSwitch)
		{
			return genericConsole;
		}
		return icon;
	}
}
