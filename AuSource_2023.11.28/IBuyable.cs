public interface IBuyable
{
	string ProdId { get; }

	int BeanCost { get; }

	int StarCost { get; }

	bool PaidOnMobile { get; }

	LimitedTimeStartEnd LimitedTimeAvailable { get; }
}
