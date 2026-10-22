using Hazel;

public interface ISystemType
{
	bool IsDirty { get; }

	void Deteriorate(float deltaTime);

	void UpdateSystem(PlayerControl player, MessageReader msgReader);

	void Serialize(MessageWriter writer, bool initialState);

	void Deserialize(MessageReader reader, bool initialState);
}
