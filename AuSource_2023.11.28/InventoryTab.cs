using System.Collections.Generic;
using AmongUs.Data;
using UnityEngine;

public class InventoryTab : MonoBehaviour
{
	public ColorChip ColorTabPrefab;

	public PoolablePlayer PlayerPreview;

	public FloatRange XRange = new FloatRange(1.5f, 3f);

	public float YStart = 1.1f;

	public float YOffset = 0.8f;

	public int NumPerRow = 4;

	public Scroller scroller;

	protected int NumRowsVisible = 3;

	[HideInInspector]
	public List<ColorChip> ColorChips = new List<ColorChip>();

	protected bool HasLocalPlayer()
	{
		return (Object)(object)PlayerControl.LocalPlayer != (Object)null;
	}

	protected int GetDisplayColor()
	{
		if (!HasLocalPlayer())
		{
			return DataManager.Player.Customization.Color;
		}
		return PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId;
	}

	public virtual void ClickEquip()
	{
	}

	public virtual void OnEnable()
	{
		((Component)PlayerPreview).gameObject.SetActive(true);
		if (HasLocalPlayer())
		{
			PlayerPreview.UpdateFromLocalPlayer(PlayerMaterial.MaskType.None);
		}
		else
		{
			PlayerPreview.UpdateFromDataManager(PlayerMaterial.MaskType.None);
		}
	}

	public virtual void OnDisable()
	{
		for (int i = 0; i < ColorChips.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)ColorChips[i]).gameObject);
		}
		ColorChips.Clear();
	}

	public virtual ColorChip GetDefaultSelectable()
	{
		return ColorChips[0];
	}

	public virtual bool IsSelectedItemEquipped()
	{
		return false;
	}

	protected void UpdateMaterials(SpriteRenderer spriteRenderer, CosmeticData data)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		if (!data.PreviewCrewmateColor)
		{
			((Renderer)spriteRenderer).sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
		}
		else
		{
			((Renderer)spriteRenderer).sharedMaterial = new Material(DestroyableSingleton<HatManager>.Instance.PlayerMaterial);
		}
	}

	protected void SetScrollerBounds()
	{
		scroller.CalculateAndSetYBounds(ColorChips.Count, NumPerRow, NumRowsVisible, YOffset);
	}
}
