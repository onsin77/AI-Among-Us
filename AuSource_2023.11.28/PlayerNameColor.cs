using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerNameColor
{
	public static Color Get(GameData.PlayerInfo pc)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Get(pc?.Role);
	}

	public static void Set(PlayerControl pc)
	{
		Set(pc, pc.Data?.Role);
	}

	public static void Set(GameData.PlayerInfo playerInfo, TextMeshPro name)
	{
		Set(name, playerInfo?.Role);
	}

	public static void SetForRoleDirectly(PlayerControl player, RoleBehaviour role)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (player.AmOwner)
		{
			player.cosmetics.SetNameColor(role.NameColor);
		}
	}

	private static Color Get(RoleBehaviour otherPlayerRole)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || PlayerControl.LocalPlayer.Data == null || (Object)(object)PlayerControl.LocalPlayer.Data.Role == (Object)null || (Object)(object)otherPlayerRole == (Object)null)
		{
			return Color.white;
		}
		bool flag = PlayerControl.LocalPlayer.Data.Role.TeamType == otherPlayerRole.TeamType;
		bool revealTeams = GameManager.Instance.RevealTeams;
		if (!(flag | revealTeams))
		{
			return Color.white;
		}
		return otherPlayerRole.NameColor;
	}

	private static void Set(PlayerControl otherPlayer, RoleBehaviour otherPlayerRole)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		otherPlayer.cosmetics.SetNameColor(Get(otherPlayerRole));
	}

	private static void Set(TextMeshPro nameText, RoleBehaviour otherPlayerRole)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)nameText).color = Get(otherPlayerRole);
	}
}
