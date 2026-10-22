using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu]
public class CosmicubeData : ScriptableObject, IBuyable
{
	public AssetReference StoreImageRef;

	public AssetReference CosmicubePrefabRef;

	public AssetReference podIconRef;

	public string DateCreated;

	public List<UnlockableCosmetic> allUnlockables;

	public int starCost;

	public int beanCost;

	public int Order;

	public bool Free;

	public bool NotInStore;

	public FloatRange XRange;

	public FloatRange YRange;

	public string podId;

	[SerializeField]
	private string productId;

	[SerializeField]
	private string cosmicubeName;

	[SerializeField]
	private LimitedTimeStartEnd limitedTime;

	[SerializeField]
	private string epicId = "";

	public string ProdId => productId;

	public int BeanCost => beanCost;

	public int StarCost => starCost;

	public string EpicId => epicId;

	public bool PaidOnMobile => false;

	public bool IsAlwaysAvailable => !limitedTime.IsLimitedTime();

	public LimitedTimeStartEnd LimitedTimeAvailable => limitedTime;

	public bool IsEmpty => cosmicubeName == "Empty";

	public string GetCosmicubeName()
	{
		return cosmicubeName + " Cosmicube";
	}

	public string GetPodName()
	{
		return cosmicubeName + " Pods";
	}

	public DateTime GetDateCreated()
	{
		if (!DateCreated.IsNullOrWhiteSpace())
		{
			return Constants.ParseDate(DateCreated);
		}
		return DateTime.MinValue;
	}
}
