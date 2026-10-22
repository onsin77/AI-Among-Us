using UnityEngine;

public class WinningPlayerData : GameData.PlayerOutfit
{
	public bool IsYou;

	public bool IsImpostor;

	public bool IsDead;

	public WinningPlayerData(GameData.PlayerInfo player)
	{
		IsYou = (Object)(object)player.Object == (Object)(object)PlayerControl.LocalPlayer;
		PlayerName = player.PlayerName;
		IsDead = player.IsDead || player.Disconnected;
		IsImpostor = player.Role.IsImpostor;
		GameData.PlayerOutfit defaultOutfit = player.DefaultOutfit;
		ColorId = defaultOutfit.ColorId;
		SkinId = defaultOutfit.SkinId;
		PetId = defaultOutfit.PetId;
		HatId = defaultOutfit.HatId;
		VisorId = defaultOutfit.VisorId;
	}
}
