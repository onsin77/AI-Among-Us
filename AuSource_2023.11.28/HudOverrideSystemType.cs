using Hazel;

internal class HudOverrideSystemType : ISystemType, IActivatable
{
	public const byte DamageBit = 128;

	public const byte TaskMask = 127;

	public bool IsActive { get; private set; }

	public bool IsDirty { get; private set; }

	public void Deteriorate(float deltaTime)
	{
		if (IsActive && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.Comms);
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		if ((msgReader.ReadByte() & 0x80) != 0)
		{
			IsActive = true;
		}
		else
		{
			IsActive = false;
		}
		IsDirty = true;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(IsActive);
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		IsActive = reader.ReadBoolean();
	}
}
