namespace AmongUs.GameOptions;

public class RoleDataV07
{
	public RoleTypes Type { get; }

	public IRoleOptions RoleOptions { get; }

	public RoleRate Rate { get; set; }

	public RoleDataV07(IRoleOptions roleOptions, RoleRate rate)
	{
		Type = roleOptions.Type;
		RoleOptions = roleOptions;
		Rate = rate;
	}

	public RoleDataV07(RoleTypes type)
	{
		Type = type;
		switch (Type)
		{
		case RoleTypes.Engineer:
			RoleOptions = new EngineerRoleOptionsV07();
			break;
		case RoleTypes.Scientist:
			RoleOptions = new ScientistRoleOptionsV07();
			break;
		case RoleTypes.Shapeshifter:
			RoleOptions = new ShapeshifterRoleOptionsV07();
			break;
		case RoleTypes.GuardianAngel:
			RoleOptions = new GuardianAngelRoleOptionsV07();
			break;
		}
	}
}
