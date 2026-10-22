using Hazel;

public class LogicMinigame : GameLogicComponent
{
	public LogicMinigame(GameManager manager)
		: base(manager)
	{
	}

	public virtual void OnMinigameOpen()
	{
	}

	public virtual void OnMinigameClose()
	{
	}

	public override void OnGameStart()
	{
	}

	public override void OnGameEnd()
	{
	}

	public override void FixedUpdate()
	{
	}

	public override void OnDestroy()
	{
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
