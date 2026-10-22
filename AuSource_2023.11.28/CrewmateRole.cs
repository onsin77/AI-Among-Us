using System.Text;

public class CrewmateRole : RoleBehaviour
{
	public override bool IsDead => false;

	public override bool CanUse(IUsable console)
	{
		return console is Console;
	}

	public override bool DidWin(GameOverReason gameOverReason)
	{
		return GameManager.Instance.DidHumansWin(gameOverReason);
	}

	public override void AppendTaskHint(StringBuilder taskStringBuilder)
	{
	}
}
