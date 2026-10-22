using Hazel;

namespace AmongUs.GameOptions;

public class GuardianAngelRoleOptionsV07 : IRoleOptions
{
	public float GuardianAngelCooldown = 60f;

	public float ProtectionDurationSeconds = 10f;

	public bool ImpostorsCanSeeProtect;

	public RoleTypes Type { get; } = RoleTypes.GuardianAngel;

	public void Serialize(MessageWriter writer)
	{
		writer.Write((byte)GuardianAngelCooldown);
		writer.Write((byte)ProtectionDurationSeconds);
		writer.Write(ImpostorsCanSeeProtect);
	}

	public void Deserialize(MessageReader reader)
	{
		GuardianAngelCooldown = (int)reader.ReadByte();
		ProtectionDurationSeconds = (int)reader.ReadByte();
		ImpostorsCanSeeProtect = reader.ReadBoolean();
	}

	public void SetRecommended()
	{
		ImpostorsCanSeeProtect = false;
		GuardianAngelCooldown = 60f;
		ProtectionDurationSeconds = 10f;
	}
}
