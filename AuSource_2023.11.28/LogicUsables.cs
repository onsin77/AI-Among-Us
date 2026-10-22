public abstract class LogicUsables : GameLogicComponent
{
	public LogicUsables(GameManager manager)
		: base(manager)
	{
	}

	public abstract bool CanUse(IUsable usable, PlayerControl player);
}
