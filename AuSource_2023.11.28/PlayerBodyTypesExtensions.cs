using System.Collections.Generic;

public static class PlayerBodyTypesExtensions
{
	private static readonly Dictionary<PlayerBodyTypes, HatOptions> CustomHatOptions = new Dictionary<PlayerBodyTypes, HatOptions>
	{
		{
			PlayerBodyTypes.Normal,
			new HatOptions
			{
				ShowForClimb = true,
				VisorBlockingHatsAllowed = true,
				ShowHelmets = true
			}
		},
		{
			PlayerBodyTypes.Horse,
			new HatOptions
			{
				ShowForClimb = false,
				VisorBlockingHatsAllowed = true,
				ShowHelmets = true
			}
		},
		{
			PlayerBodyTypes.Seeker,
			new HatOptions
			{
				ShowForClimb = true,
				VisorBlockingHatsAllowed = false,
				ShowHelmets = false
			}
		}
	};

	private static readonly Dictionary<PlayerBodyTypes, SkinOptions> CustomSkinOptions = new Dictionary<PlayerBodyTypes, SkinOptions>
	{
		{
			PlayerBodyTypes.Normal,
			new SkinOptions
			{
				Enabled = true
			}
		},
		{
			PlayerBodyTypes.Horse,
			new SkinOptions
			{
				Enabled = false
			}
		},
		{
			PlayerBodyTypes.Seeker,
			new SkinOptions
			{
				Enabled = false
			}
		}
	};

	private static readonly Dictionary<PlayerBodyTypes, VisorOptions> CustomVisorOptions = new Dictionary<PlayerBodyTypes, VisorOptions>
	{
		{
			PlayerBodyTypes.Normal,
			new VisorOptions
			{
				Enabled = true,
				HideDuringClimb = true
			}
		},
		{
			PlayerBodyTypes.Horse,
			new VisorOptions
			{
				Enabled = true,
				HideDuringClimb = true
			}
		},
		{
			PlayerBodyTypes.Seeker,
			new VisorOptions
			{
				Enabled = false,
				HideDuringClimb = true
			}
		}
	};

	public static HatOptions GetHatOptions(this PlayerBodyTypes bodyTypes)
	{
		return CustomHatOptions[bodyTypes];
	}

	public static SkinOptions GetSkinOptions(this PlayerBodyTypes bodyTypes)
	{
		return CustomSkinOptions[bodyTypes];
	}

	public static VisorOptions GetVisorOptions(this PlayerBodyTypes bodyTypes)
	{
		return CustomVisorOptions[bodyTypes];
	}
}
