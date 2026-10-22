using Hazel;
using UnityEngine;

public class LogicHnSDeathPopup : GameLogicComponent
{
	private HideAndSeekDeathPopup prefab;

	private int deadPlayerCount;

	public LogicHnSDeathPopup(HideAndSeekManager manager, HideAndSeekDeathPopup deathPopupPrefab)
		: base(manager)
	{
		prefab = deathPopupPrefab;
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

	public void OnPlayerDeath(PlayerControl player)
	{
		if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			deadPlayerCount++;
			Object.Instantiate<HideAndSeekDeathPopup>(prefab, ((Component)DestroyableSingleton<HudManager>.Instance).transform.parent).Show(player, deadPlayerCount);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
