using Hazel;

public abstract class GameLogicComponent
{
	protected GameManager Manager;

	public bool IsDirty { get; private set; } = true;

	public GameLogicComponent(GameManager manager)
	{
		Manager = manager;
	}

	public abstract void OnGameStart();

	public abstract void OnGameEnd();

	public abstract void FixedUpdate();

	public abstract void OnDestroy();

	public virtual void OnPlayerDisconnect(PlayerControl pc)
	{
	}

	public virtual void HandleRPC(RpcCalls callId, MessageReader reader)
	{
	}

	public abstract bool Serialize(MessageWriter writer, bool initialState);

	public abstract void Deserialize(MessageReader reader, bool initialState);

	public void SetDirty()
	{
		IsDirty = true;
	}

	public void ClearDirtyFlag()
	{
		IsDirty = false;
	}
}
