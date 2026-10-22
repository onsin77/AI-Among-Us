using System;
using UnityEngine;

public class CosmeticGroups : ScriptableObject
{
	[SerializeField]
	private CosmeticVariantGroup[] variantGroups;

	[SerializeField]
	private CosmeticReleaseGroup[] releaseGroups;

	public CosmeticVariantGroup[] VariantGroups => variantGroups;

	public CosmeticReleaseGroup[] ReleaseGroups => releaseGroups;

	public bool TryGetVariantGroup(CosmeticData cosmetic, out CosmeticVariantGroup output)
	{
		return TryGetGroup(variantGroups, cosmetic, out output);
	}

	public bool TryGetReleaseGroup(CosmeticData cosmetic, out CosmeticReleaseGroup output)
	{
		return TryGetGroup(releaseGroups, cosmetic, out output);
	}

	public DateTime GetReleaseGroupDate(CosmeticData cosmetic)
	{
		CosmeticReleaseGroup[] array = releaseGroups;
		foreach (CosmeticReleaseGroup cosmeticReleaseGroup in array)
		{
			if (!string.IsNullOrWhiteSpace(cosmeticReleaseGroup.DateString) && cosmeticReleaseGroup.Contains(cosmetic.ProductId))
			{
				return Constants.ParseDate(cosmeticReleaseGroup.DateString);
			}
		}
		return DateTime.MinValue;
	}

	private bool TryGetGroup<T>(T[] groups, CosmeticData cosmetic, out T output) where T : AbstractCosmeticGroup
	{
		foreach (T val in groups)
		{
			if (val.Contains(cosmetic))
			{
				output = val;
				return true;
			}
		}
		output = null;
		return false;
	}

	public void RebuildGroups(CosmeticVariantGroup[] variantGroups, CosmeticReleaseGroup[] releaseGroups)
	{
		this.variantGroups = variantGroups;
		this.releaseGroups = releaseGroups;
	}
}
