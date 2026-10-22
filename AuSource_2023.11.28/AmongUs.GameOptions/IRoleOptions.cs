using Hazel;

namespace AmongUs.GameOptions;

public interface IRoleOptions
{
	RoleTypes Type { get; }

	void Serialize(MessageWriter writer);

	void Deserialize(MessageReader reader);

	void SetRecommended();
}
