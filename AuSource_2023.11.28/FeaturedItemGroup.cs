using System;
using System.Collections.Generic;

[Serializable]
public class FeaturedItemGroup
{
	public LimitedTimeStartEnd limitedTime;

	public List<CosmeticData> featuredItems;

	public List<BundleData> featuredBundles;

	public List<CosmicubeData> featuredCubes;

	public bool IsAlwaysAvailable => !limitedTime.IsLimitedTime();

	public LimitedTimeStartEnd LimitedTimeAvailable => limitedTime;
}
