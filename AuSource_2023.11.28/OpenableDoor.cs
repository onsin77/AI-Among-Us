using Hazel;

public abstract class OpenableDoor : SomeKindaDoor
{
	public int Id;

	public SystemTypes Room;

	public abstract bool IsOpen { get; }

	public abstract void Serialize(MessageWriter writer);

	public abstract void Deserialize(MessageReader reader);

	public abstract bool DoUpdate(float deltaTime);
}
