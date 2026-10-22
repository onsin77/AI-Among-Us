using System;
using Hazel;

public class SwitchSystem : ISystemType, IActivatable
{
	public const byte MaxValue = byte.MaxValue;

	public const int NumSwitches = 5;

	public const byte DamageSystem = 128;

	public const byte SwitchesMask = 31;

	public float DetoriorationTime = 0.03f;

	public byte Value = byte.MaxValue;

	private float timer;

	public byte ExpectedSwitches;

	public byte ActualSwitches;

	public bool IsDirty { get; private set; }

	public float Level => (float)(int)Value / 255f;

	public bool IsActive => ExpectedSwitches != ActualSwitches;

	public SwitchSystem()
	{
		Random random = new Random();
		ExpectedSwitches = (byte)(random.Next() & 0x1F);
		ActualSwitches = ExpectedSwitches;
	}

	public void Deteriorate(float deltaTime)
	{
		timer += deltaTime;
		if (!(timer >= DetoriorationTime))
		{
			return;
		}
		timer = 0f;
		if (ExpectedSwitches != ActualSwitches)
		{
			if (Value > 0)
			{
				Value = (byte)Math.Max(Value - 3, 0);
			}
			if (!HasTask<ElectricTask>())
			{
				PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.Electrical);
			}
		}
		else if (Value < byte.MaxValue)
		{
			Value = (byte)Math.Min(Value + 3, 255);
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte b = msgReader.ReadByte();
		if (b.HasBit(128))
		{
			ActualSwitches ^= (byte)(b & 0x1F);
		}
		else
		{
			ActualSwitches ^= (byte)(1 << (int)b);
		}
		IsDirty = true;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(ExpectedSwitches);
		writer.Write(ActualSwitches);
		writer.Write(Value);
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		ExpectedSwitches = reader.ReadByte();
		ActualSwitches = reader.ReadByte();
		Value = reader.ReadByte();
	}

	protected static bool HasTask<T>()
	{
		for (int num = PlayerControl.LocalPlayer.myTasks.Count - 1; num > 0; num--)
		{
			if (PlayerControl.LocalPlayer.myTasks[num] is T)
			{
				return true;
			}
		}
		return false;
	}
}
