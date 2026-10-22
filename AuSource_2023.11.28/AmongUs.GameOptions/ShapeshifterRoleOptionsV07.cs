using Hazel;

namespace AmongUs.GameOptions;

public class ShapeshifterRoleOptionsV07 : IRoleOptions
{
	public bool ShapeshifterLeaveSkin;

	public float ShapeshifterCooldown = 10f;

	public float ShapeshifterDuration = 8f;

	public RoleTypes Type { get; } = RoleTypes.Shapeshifter;

	public void Serialize(MessageWriter writer)
	{
		writer.Write(ShapeshifterLeaveSkin);
		writer.Write((byte)ShapeshifterCooldown);
		writer.Write((byte)ShapeshifterDuration);
	}

	public void Deserialize(MessageReader reader)
	{
		ShapeshifterLeaveSkin = reader.ReadBoolean();
		ShapeshifterCooldown = (int)reader.ReadByte();
		ShapeshifterDuration = (int)reader.ReadByte();
	}

	public void SetRecommended()
	{
		ShapeshifterCooldown = 10f;
		ShapeshifterDuration = 30f;
		ShapeshifterLeaveSkin = false;
	}
}
