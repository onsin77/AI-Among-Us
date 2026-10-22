using UnityEngine;

public class BoughtAdsBuyable : Object, IBuyable
{
	public string ProdId => "bought_ads";

	public int BeanCost => 0;

	public int StarCost => 0;

	public string EpicId => "";

	public bool PaidOnMobile => true;

	public LimitedTimeStartEnd LimitedTimeAvailable => default;
}
