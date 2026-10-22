using System.Collections;
using AmongUs.Data;
using Innersloth.Assets;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CosmicubeCell : MonoBehaviour
{
	private const float cellSpacing = 0.74f;

	private const float cellUnlockGlowDuration = 0.9f;

	public CosmicubeCell parent;

	public CosmicubeCell northChild;

	public CosmicubeCell southChild;

	public CosmicubeCell eastChild;

	public CosmicubeCell westChild;

	public SpriteRenderer icon;

	public TextMeshPro costText;

	public SpriteRenderer cellSprite;

	public SpriteRenderer qualitySprite;

	public SpriteRenderer glowSprite;

	public SpriteRenderer MaskArea;

	public SpriteRenderer SelectionHighlight;

	public GameObject data;

	public bool unlocked;

	public SpriteAnim Animator;

	public AnimationClip PurchaseAnimation;

	public UnlockableCosmetic cosmetic;

	[SerializeField]
	private Material iconMaterial;

	[SerializeField]
	private Material iconColoredMaterial;

	[SerializeField]
	private Material maskMaterial;

	private CosmicubeMenu menu;

	public bool HasEastCell
	{
		get
		{
			if (!((Object)(object)eastChild != (Object)null))
			{
				if ((Object)(object)parent != (Object)null)
				{
					return (Object)(object)parent.westChild == (Object)(object)this;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasWestCell
	{
		get
		{
			if (!((Object)(object)westChild != (Object)null))
			{
				if ((Object)(object)parent != (Object)null)
				{
					return (Object)(object)parent.eastChild == (Object)(object)this;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasNorthCell
	{
		get
		{
			if (!((Object)(object)northChild != (Object)null))
			{
				if ((Object)(object)parent != (Object)null)
				{
					return (Object)(object)parent.southChild == (Object)(object)this;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasSouthCell
	{
		get
		{
			if (!((Object)(object)southChild != (Object)null))
			{
				if ((Object)(object)parent != (Object)null)
				{
					return (Object)(object)parent.northChild == (Object)(object)this;
				}
				return false;
			}
			return true;
		}
	}

	public UnlockableCosmetic CosmeticData => cosmetic;

	private void Start()
	{
		if ((Object)(object)cosmetic != (Object)null)
		{
			SetData(cosmetic);
		}
		if ((Object)(object)CosmeticData == (Object)null || DataManager.Player.Purchases.GetPurchase(CosmeticData.GetProdId(), CosmeticData.GetBundleId()))
		{
			SetCellPurchased();
		}
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)parent) && !parent.unlocked)
		{
			SetUnavailable();
		}
		else if (Object.op_Implicit((Object)(object)parent) && parent.unlocked)
		{
			SetAvailable();
		}
	}

	public void SetMenu(CosmicubeMenu menu)
	{
		this.menu = menu;
	}

	public void SetParent(CosmicubeCell parent)
	{
		this.parent = parent;
	}

	public void SetEastChild(CosmicubeCell c, UnlockableCosmetic data)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		eastChild = c;
		eastChild.SetData(data);
		((Component)eastChild).transform.position = ((Component)this).transform.position + new Vector3(0.74f, 0f, 0f);
		eastChild.parent = this;
	}

	public void SetWestChild(CosmicubeCell c, UnlockableCosmetic data)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		westChild = c;
		westChild.SetData(data);
		((Component)westChild).transform.position = ((Component)this).transform.position - new Vector3(0.74f, 0f, 0f);
		westChild.parent = this;
	}

	public void SetNorthChild(CosmicubeCell c, UnlockableCosmetic data)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		northChild = c;
		northChild.SetData(data);
		((Component)northChild).transform.position = ((Component)this).transform.position + new Vector3(0f, 0.74f, 0f);
		northChild.parent = this;
	}

	public void SetSouthChild(CosmicubeCell c, UnlockableCosmetic data)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		southChild = c;
		southChild.SetData(data);
		((Component)southChild).transform.position = ((Component)this).transform.position - new Vector3(0f, 0.74f, 0f);
		southChild.parent = this;
	}

	public void SetData(UnlockableCosmetic cosmetic)
	{
		this.cosmetic = cosmetic;
		SetPositionAndScale(cosmetic);
		SetMetadata(cosmetic);
		SetMask();
		((MonoBehaviour)this).StartCoroutine(CoSetSprites(cosmetic));
	}

	public IEnumerator CoEditorSetData(UnlockableCosmetic cosmetic)
	{
		this.cosmetic = cosmetic;
		SetMetadata(cosmetic);
		SetMask();
		SetPositionAndScale(cosmetic);
		yield return CoSetSprites(cosmetic);
	}

	public void SetPositionAndScale(UnlockableCosmetic cosmetic)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)cosmetic.unlockableCosmetic))
		{
			if (!(cosmetic.unlockableCosmetic is NamePlateData))
			{
				((Component)icon).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
				((Component)icon).transform.localPosition = new Vector3(0f, 0f, -3f);
			}
			else
			{
				((Component)icon).transform.localScale = new Vector3(0.7f, 0.7f, 1f);
				((Component)icon).transform.localPosition = new Vector3(-0.65f, -0.14f, -3f);
			}
		}
	}

	public void SetMetadata(UnlockableCosmetic cosmetic)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)costText).text = cosmetic.cost.ToString();
		qualitySprite.color = CosmicubeManager.qualityColorMap[cosmetic.quality];
		glowSprite.color = CosmicubeManager.qualityColorMap[cosmetic.quality];
	}

	public void SetMask()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		CosmicubeMenu.CellMaskLayer++;
		if (!Application.isPlaying)
		{
			EditMaterialInEditorMode(MaskArea, maskMaterial, CosmicubeMenu.CellMaskLayer);
			return;
		}
		((Renderer)MaskArea).material = new Material(maskMaterial);
		((Renderer)MaskArea).material.SetInt(PlayerMaterial.MaskLayer, CosmicubeMenu.CellMaskLayer);
	}

	public IEnumerator CoSetSprites(UnlockableCosmetic cosmetic)
	{
		int cellMaskLayer = CosmicubeMenu.CellMaskLayer;
		if (Object.op_Implicit((Object)(object)cosmetic.unlockableCosmetic))
		{
			if (cosmetic.unlockableCosmetic is NamePlateData)
			{
				yield return this.cosmetic.CoLoadIcon((Sprite sprite, AddressableAsset asset) =>
				{
					//IL_0056: Unknown result type (might be due to invalid IL or missing references)
					//IL_0060: Expected Obj, but got Unknown
					icon.sprite = sprite;
					if (!Application.isPlaying)
					{
						EditMaterialInEditorMode(icon, iconMaterial, cellMaskLayer);
					}
					else
					{
						((Renderer)icon).material = new Material(iconMaterial);
						((Renderer)icon).material.SetInt(PlayerMaterial.MaskLayer, cellMaskLayer);
						AddressableAssetHandler.AddToGameObject(((Component)icon).gameObject, asset);
					}
				});
			}
			else
			{
				cosmetic.unlockableCosmetic.SetPreview(icon, 0);
				if (!Application.isPlaying)
				{
					EditMaterialInEditorMode(icon, GetIconMaterial(), cellMaskLayer);
				}
				else
				{
					((Renderer)icon).material = new Material(GetIconMaterial());
					((Renderer)icon).material.SetInt(PlayerMaterial.MaskLayer, cellMaskLayer);
					PlayerMaterial.SetColors((int)DataManager.Player.Customization.Color, ((Renderer)icon).material);
				}
			}
		}
		yield return null;
	}

	private void EditMaterialInEditorMode(SpriteRenderer sprite, Material material, int cellMaskLayer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected Obj, but got Unknown
		Material val = new Material(material);
		val.SetInt(PlayerMaterial.MaskLayer, cellMaskLayer);
		((Renderer)sprite).sharedMaterial = val;
	}

	private Material GetIconMaterial()
	{
		if ((Object)(object)cosmetic != (Object)null && (Object)(object)cosmetic.unlockableCosmetic != (Object)null && cosmetic.unlockableCosmetic.PreviewCrewmateColor)
		{
			return iconColoredMaterial;
		}
		return iconMaterial;
	}

	public void ChooseCell()
	{
		menu.SelectItem(this);
	}

	public void SetSelected()
	{
		((Renderer)SelectionHighlight).enabled = true;
	}

	public void SetDeselected()
	{
		((Renderer)SelectionHighlight).enabled = false;
	}

	public void SetCellPurchased()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unlocked = true;
		cellSprite.color = Color.white;
		((Component)costText).gameObject.SetActive(false);
	}

	public void SetUnavailable()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		cellSprite.color = Palette.DisabledGrey;
		icon.color = Color.gray;
		qualitySprite.color = CosmicubeManager.qualityColorMap[cosmetic.quality] * 0.5f;
		((Graphic)costText).color = Color.gray;
	}

	public void SetAvailable()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		cellSprite.color = (unlocked ? Color.white : Palette.CosmicubeCellLockedColor);
		icon.color = Color.white;
		qualitySprite.color = CosmicubeManager.qualityColorMap[cosmetic.quality];
		((Graphic)costText).color = Color.white;
	}

	public IEnumerator CoPurchase()
	{
		if (Object.op_Implicit((Object)(object)eastChild))
		{
			((Behaviour)eastChild).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)westChild))
		{
			((Behaviour)westChild).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)northChild))
		{
			((Behaviour)northChild).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)southChild))
		{
			((Behaviour)southChild).enabled = false;
		}
		data.GetComponentsInChildren<CosmicubeBuildHelper>().ForEach((CosmicubeBuildHelper h) =>
		{
			h.AnimateUnlock(0.9f);
		});
		SetCellPurchased();
		yield return CoPurchaseAnimation();
	}

	public bool CanBePurchased()
	{
		if (parent.unlocked && !unlocked)
		{
			return (Object)(object)cosmetic != (Object)null;
		}
		return false;
	}

	private IEnumerator CoPurchaseAnimation()
	{
		((MonoBehaviour)this).StartCoroutine((IEnumerator)new WaitForAnimationFinish(Animator, PurchaseAnimation));
		yield return Effects.Wait(0.9f);
		if (Object.op_Implicit((Object)(object)eastChild))
		{
			((Behaviour)eastChild).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)westChild))
		{
			((Behaviour)westChild).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)northChild))
		{
			((Behaviour)northChild).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)southChild))
		{
			((Behaviour)southChild).enabled = true;
		}
	}
}
