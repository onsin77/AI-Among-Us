using Hazel;

public class PlatformSpecificData
{
	public Platforms Platform;

	public string PlatformName = "";

	public ulong XboxPlatformId;

	public ulong PsnPlatformId;

	public void Deserialize(MessageReader reader)
	{
		Platform = (Platforms)reader.Tag;
		PlatformName = reader.ReadString();
		switch (Platform)
		{
		case Platforms.StandaloneWin10:
		case Platforms.Xbox:
			XboxPlatformId = reader.ReadUInt64();
			break;
		case Platforms.Playstation:
			PsnPlatformId = reader.ReadUInt64();
			break;
		}
	}

	public void Serialize(MessageWriter writer)
	{
		writer.StartMessage((byte)Platform);
		writer.Write(PlatformName);
		switch (Platform)
		{
		case Platforms.StandaloneWin10:
		case Platforms.Xbox:
			writer.Write(XboxPlatformId);
			break;
		case Platforms.Playstation:
			writer.Write(PsnPlatformId);
			break;
		}
		writer.EndMessage();
	}
}
