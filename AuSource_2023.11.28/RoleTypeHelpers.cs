using System.Collections.Generic;
using AmongUs.GameOptions;

public static class RoleTypeHelpers
{
	public static Dictionary<RoleTypes, StringNames> RoleToName = new Dictionary<RoleTypes, StringNames>
	{
		{
			RoleTypes.Crewmate,
			StringNames.Crewmate
		},
		{
			RoleTypes.Impostor,
			StringNames.Impostor
		},
		{
			RoleTypes.Scientist,
			StringNames.ScientistRole
		},
		{
			RoleTypes.Engineer,
			StringNames.EngineerRole
		},
		{
			RoleTypes.GuardianAngel,
			StringNames.GuardianAngelRole
		},
		{
			RoleTypes.Shapeshifter,
			StringNames.ShapeshifterRole
		},
		{
			RoleTypes.CrewmateGhost,
			StringNames.CrewmateGhostRole
		},
		{
			RoleTypes.ImpostorGhost,
			StringNames.ImpostorGhostRole
		}
	};
}
