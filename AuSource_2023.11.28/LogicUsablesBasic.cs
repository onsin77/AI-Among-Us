using Hazel;

public class LogicUsablesBasic : LogicUsables
{
	public LogicUsablesBasic(GameManager manager)
		: base(manager)
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

	public override bool CanUse(IUsable usable, PlayerControl player)
	{
		return true;
	}
}
