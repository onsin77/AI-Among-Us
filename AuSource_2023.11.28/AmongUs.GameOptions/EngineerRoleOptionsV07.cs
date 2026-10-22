using Hazel;

namespace AmongUs.GameOptions;

public class EngineerRoleOptionsV07 : IRoleOptions
{
	public float EngineerCooldown = 30f;

	public float EngineerInVentMaxTime = 15f;

	public RoleTypes Type { get; } = RoleTypes.Engineer;

	public void Serialize(MessageWriter writer)
	{
		writer.Write((byte)EngineerCooldown);
		writer.Write((byte)EngineerInVentMaxTime);
	}

	public void Deserialize(MessageReader reader)
	{
		EngineerCooldown = (int)reader.ReadByte();
		EngineerInVentMaxTime = (int)reader.ReadByte();
	}

	public void SetRecommended()
	{
		EngineerCooldown = 30f;
		EngineerInVentMaxTime = 15f;
	}
}
