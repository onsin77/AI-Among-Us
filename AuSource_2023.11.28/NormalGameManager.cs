using UnityEngine;

public class NormalGameManager : GameManager
{
	public override void InitComponents()
	{
		LogicFlow = AddGameLogic(new LogicGameFlowNormal(this));
		LogicMinigame = AddGameLogic(new LogicMinigame(this));
		LogicRoleSelection = AddGameLogic(new LogicRoleSelectionNormal(this));
		LogicUsables = AddGameLogic(new LogicUsablesBasic(this));
		LogicOptions = AddGameLogic(new LogicOptionsNormal(this));
	}

	public override MapOptions GetMapOptions()
	{
		return new MapOptions
		{
			Mode = ((!PlayerControl.LocalPlayer.Data.Role.IsImpostor || Object.op_Implicit((Object)(object)MeetingHud.Instance)) ? MapOptions.Modes.Normal : MapOptions.Modes.Sabotage)
		};
	}

	public override bool IsNormal()
	{
		return true;
	}

	public override PlayerBodyTypes GetBodyType(PlayerControl player)
	{
		if (Constants.ShouldHorseAround())
		{
			return PlayerBodyTypes.Horse;
		}
		return PlayerBodyTypes.Normal;
	}
}
