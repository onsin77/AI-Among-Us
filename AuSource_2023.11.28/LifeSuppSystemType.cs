using System.Collections.Generic;
using Hazel;

public class LifeSuppSystemType : ISystemType, IActivatable
{
	private const float SyncRate = 2f;

	private float timer;

	public const byte StartCountdown = 128;

	public const byte AddUserOp = 64;

	public const byte ClearCountdown = 16;

	public const float CountdownStopped = 10000f;

	public readonly float LifeSuppDuration = 45f;

	public const byte ConsoleIdMask = 3;

	public const byte RequiredUserCount = 2;

	public float Countdown = 10000f;

	private HashSet<int> CompletedConsoles = new HashSet<int>();

	public bool IsDirty { get; private set; }

	public int UserCount => CompletedConsoles.Count;

	public bool IsActive => Countdown < 10000f;

	public LifeSuppSystemType(float duration)
	{
		LifeSuppDuration = duration;
	}

	public bool GetConsoleComplete(int consoleId)
	{
		return CompletedConsoles.Contains(consoleId);
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte b = msgReader.ReadByte();
		int item = b & 3;
		if (b == 128 && !IsActive)
		{
			Countdown = LifeSuppDuration;
			CompletedConsoles.Clear();
		}
		else if (b == 16)
		{
			Countdown = 10000f;
		}
		else if (b.HasAnyBit((byte)64))
		{
			CompletedConsoles.Add(item);
		}
		IsDirty = true;
	}

	public void Deteriorate(float deltaTime)
	{
		if (IsActive)
		{
			if (DestroyableSingleton<HudManager>.Instance.OxyFlash == null)
			{
				PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.LifeSupp);
			}
			Countdown -= deltaTime;
			if (UserCount >= 2)
			{
				Countdown = 10000f;
				IsDirty = true;
				return;
			}
			timer += deltaTime;
			if (timer > 2f)
			{
				timer = 0f;
				IsDirty = true;
			}
		}
		else if (DestroyableSingleton<HudManager>.Instance.OxyFlash != null)
		{
			DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(Countdown);
		writer.WritePacked(CompletedConsoles.Count);
		foreach (int completedConsole in CompletedConsoles)
		{
			writer.WritePacked(completedConsole);
		}
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		Countdown = reader.ReadSingle();
		if (reader.Position < reader.Length)
		{
			CompletedConsoles.Clear();
			int num = reader.ReadPackedInt32();
			for (int i = 0; i < num; i++)
			{
				CompletedConsoles.Add(reader.ReadPackedInt32());
			}
		}
	}
}
