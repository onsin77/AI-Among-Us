public interface IUsableCoolDown : IUsable
{
	float CoolDown { get; set; }

	float MaxCoolDown { get; }

	bool IsCoolingDown();
}
