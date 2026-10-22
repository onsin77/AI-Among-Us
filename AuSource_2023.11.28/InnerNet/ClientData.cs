using System;

namespace InnerNet;

[Serializable]
public class ClientData
{
	public int Id;

	public bool InScene;

	public bool IsReady;

	public bool HasBeenReported;

	public PlayerControl Character;

	public readonly uint PlayerLevel = 1u;

	public readonly PlatformSpecificData PlatformData;

	public string PlayerName = "???";

	public string ProductUserId = string.Empty;

	public string FriendCode = string.Empty;

	public int ColorId;

	public ClientData(int id, string playerName, PlatformSpecificData platformData, uint playerLevel, string productUserId, string friendCode)
	{
		Id = id;
		PlayerName = playerName;
		ProductUserId = productUserId;
		FriendCode = friendCode;
		PlayerLevel = playerLevel;
		PlatformData = platformData;
	}

	public void UpdatePlayerName(string playerName)
	{
		PlayerName = playerName;
	}
}
