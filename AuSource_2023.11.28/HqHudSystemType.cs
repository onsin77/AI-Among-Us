using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;

internal class HqHudSystemType : ISystemType, IActivatable
{
	public enum Tags
	{
		DamageBit = 128,
		ActiveBit = 64,
		DeactiveBit = 32,
		FixBit = 16
	}

	public const byte TagMask = 240;

	public const byte IdMask = 15;

	private HashSet<Tuple<byte, byte>> ActiveConsoles = new HashSet<Tuple<byte, byte>>();

	private HashSet<byte> CompletedConsoles = new HashSet<byte>();

	private const float ActiveTime = 10f;

	private float Timer;

	public int TargetNumber;

	public bool IsActive => CompletedConsoles.Count < 2;

	public float NumComplete => CompletedConsoles.Count;

	public float PercentActive => Timer / 10f;

	public bool IsDirty { get; private set; }

	public HqHudSystemType()
	{
		CompletedConsoles.Add(0);
		CompletedConsoles.Add(1);
	}

	public void Deteriorate(float deltaTime)
	{
		if (IsActive)
		{
			Timer -= deltaTime;
			if (Timer <= 0f)
			{
				TargetNumber = IntRange.Next(0, 99999);
				Timer = 10f;
				CompletedConsoles.Clear();
			}
			if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.Comms);
			}
		}
	}

	internal bool IsConsoleActive(int consoleId)
	{
		return ActiveConsoles.Any((Tuple<byte, byte> s) => s.Item2 == (byte)consoleId);
	}

	internal bool IsConsoleOkay(int consoleId)
	{
		return CompletedConsoles.Contains((byte)consoleId);
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte b = msgReader.ReadByte();
		byte b2 = (byte)(b & 0xF);
		switch ((Tags)(b & 0xF0))
		{
		case Tags.DamageBit:
			Timer = -1f;
			CompletedConsoles.Clear();
			ActiveConsoles.Clear();
			break;
		case Tags.ActiveBit:
			ActiveConsoles.Add(new Tuple<byte, byte>(player.PlayerId, b2));
			break;
		case Tags.DeactiveBit:
			ActiveConsoles.Remove(new Tuple<byte, byte>(player.PlayerId, b2));
			break;
		case Tags.FixBit:
			Timer = 10f;
			CompletedConsoles.Add(b2);
			break;
		}
		IsDirty = true;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(ActiveConsoles.Count);
		foreach (Tuple<byte, byte> activeConsole in ActiveConsoles)
		{
			writer.Write(activeConsole.Item1);
			writer.Write(activeConsole.Item2);
		}
		writer.WritePacked(CompletedConsoles.Count);
		foreach (byte completedConsole in CompletedConsoles)
		{
			writer.Write(completedConsole);
		}
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int num = reader.ReadPackedInt32();
		ActiveConsoles.Clear();
		for (int i = 0; i < num; i++)
		{
			ActiveConsoles.Add(new Tuple<byte, byte>(reader.ReadByte(), reader.ReadByte()));
		}
		int num2 = reader.ReadPackedInt32();
		CompletedConsoles.Clear();
		for (int j = 0; j < num2; j++)
		{
			CompletedConsoles.Add(reader.ReadByte());
		}
	}
}
