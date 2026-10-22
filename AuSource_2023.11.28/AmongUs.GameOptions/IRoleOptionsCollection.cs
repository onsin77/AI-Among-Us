namespace AmongUs.GameOptions;

public interface IRoleOptionsCollection
{
	int GetNumPerGame(RoleTypes role);

	int GetChancePerGame(RoleTypes role);

	void SetRoleRate(RoleTypes role, int maxCount, int chance);

	void SetRoleRecommended(RoleTypes role);
}
