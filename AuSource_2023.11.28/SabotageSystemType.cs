using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

public class SabotageSystemType : ISystemType
{
	public const float SpecialSabDelay = 30f;

	private List<IActivatable> specials;

	private bool initialCooldown;

	public float Timer { get; private set; }

	public float PercentCool
	{
		get
		{
			if (initialCooldown)
			{
				return Timer / 10f;
			}
			return Timer / 30f;
		}
	}

	public bool AnyActive => specials.Any((IActivatable s) => s.IsActive);

	public bool IsDirty { get; private set; }

	public SabotageSystemType(IActivatable[] specials)
	{
		this.specials = new List<IActivatable>(specials);
		this.specials.RemoveAll((IActivatable d) => d is IDoorSystem);
	}

	public void Deteriorate(float deltaTime)
	{
		if (Timer > 0f && !AnyActive)
		{
			Timer -= deltaTime;
			IsDirty = true;
			if (Timer <= 0f && initialCooldown)
			{
				initialCooldown = false;
			}
		}
	}

	public void SetInitialSabotageCooldown()
	{
		Timer = 10f;
		initialCooldown = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		IsDirty = true;
		if (Timer > 0f || Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			return;
		}
		byte b = msgReader.ReadByte();
		if (AmongUsClient.Instance.AmHost)
		{
			switch ((SystemTypes)b)
			{
			case SystemTypes.Laboratory:
				ShipStatus.Instance.UpdateSystem(SystemTypes.Laboratory, player, (byte)128);
				break;
			case SystemTypes.Reactor:
				ShipStatus.Instance.UpdateSystem(SystemTypes.Reactor, player, (byte)128);
				break;
			case SystemTypes.HeliSabotage:
				ShipStatus.Instance.UpdateSystem(SystemTypes.HeliSabotage, player, (byte)128);
				break;
			case SystemTypes.LifeSupp:
				ShipStatus.Instance.UpdateSystem(SystemTypes.LifeSupp, player, (byte)128);
				break;
			case SystemTypes.Comms:
				ShipStatus.Instance.UpdateSystem(SystemTypes.Comms, player, (byte)128);
				break;
			case SystemTypes.Electrical:
			{
				byte b2 = 4;
				for (int i = 0; i < 5; i++)
				{
					if (BoolRange.Next())
					{
						b2 |= (byte)(1 << i);
					}
				}
				ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)(b2 | 0x80));
				break;
			}
			case SystemTypes.MushroomMixupSabotage:
				ShipStatus.Instance.UpdateSystem(SystemTypes.MushroomMixupSabotage, player, (byte)1);
				break;
			}
		}
		Timer = 30f;
		IsDirty = true;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(Timer);
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		Timer = reader.ReadSingle();
	}
}
