using Hazel;

namespace AmongUs.GameOptions;

public class ScientistRoleOptionsV07 : IRoleOptions
{
	public float ScientistCooldown = 15f;

	public float ScientistBatteryCharge = 5f;

	public RoleTypes Type { get; } = RoleTypes.Scientist;

	public void Serialize(MessageWriter writer)
	{
		writer.Write((byte)ScientistCooldown);
		writer.Write((byte)ScientistBatteryCharge);
	}

	public void Deserialize(MessageReader reader)
	{
		ScientistCooldown = (int)reader.ReadByte();
		ScientistBatteryCharge = (int)reader.ReadByte();
	}

	public void SetRecommended()
	{
		ScientistCooldown = 15f;
		ScientistBatteryCharge = 5f;
	}
}
