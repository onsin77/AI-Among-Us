using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCosmeticGroup : ScriptableObject
{
	[SerializeField]
	private List<string> ids = new List<string>();

	public IReadOnlyList<string> IDs => ids;

	public int Count => ids.Count;

	public bool Contains(CosmeticData cosmetic)
	{
		return Contains(cosmetic.ProductId);
	}

	public bool Contains(string id)
	{
		return ids.Contains(id);
	}
}
