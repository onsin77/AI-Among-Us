using System.Collections.Generic;
using System.IO;
using Hazel;

namespace AmongUs.GameOptions;

public class RoleOptionsData : IRoleOptionsCollection
{
	public bool ShapeshifterLeaveSkin;

	public float ShapeshifterCooldown = 10f;

	public float ShapeshifterDuration = 8f;

	public float ScientistCooldown = 15f;

	public float ScientistBatteryCharge = 5f;

	public float GuardianAngelCooldown = 60f;

	public float ProtectionDurationSeconds = 10f;

	public bool ImpostorsCanSeeProtect;

	public float EngineerCooldown = 30f;

	public float EngineerInVentMaxTime = 15f;

	private Dictionary<RoleTypes, RoleRate> roleRates = new Dictionary<RoleTypes, RoleRate>();

	public static RoleOptionsData Deserialize(BinaryReader reader)
	{
		RoleOptionsData roleOptionsData = new RoleOptionsData();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			RoleTypes key = (RoleTypes)reader.ReadInt16();
			RoleRate value = new RoleRate
			{
				MaxCount = reader.ReadByte(),
				Chance = reader.ReadByte()
			};
			roleOptionsData.roleRates[key] = value;
		}
		roleOptionsData.ShapeshifterLeaveSkin = reader.ReadBoolean();
		roleOptionsData.ShapeshifterCooldown = (int)reader.ReadByte();
		roleOptionsData.ShapeshifterDuration = (int)reader.ReadByte();
		roleOptionsData.ScientistCooldown = (int)reader.ReadByte();
		roleOptionsData.GuardianAngelCooldown = (int)reader.ReadByte();
		roleOptionsData.EngineerCooldown = (int)reader.ReadByte();
		roleOptionsData.EngineerInVentMaxTime = (int)reader.ReadByte();
		roleOptionsData.ScientistBatteryCharge = (int)reader.ReadByte();
		roleOptionsData.ProtectionDurationSeconds = (int)reader.ReadByte();
		roleOptionsData.ImpostorsCanSeeProtect = reader.ReadBoolean();
		return roleOptionsData;
	}

	public static RoleOptionsData Deserialize(MessageReader reader)
	{
		RoleOptionsData roleOptionsData = new RoleOptionsData();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			RoleTypes key = (RoleTypes)reader.ReadInt16();
			RoleRate value = new RoleRate
			{
				MaxCount = reader.ReadByte(),
				Chance = reader.ReadByte()
			};
			roleOptionsData.roleRates[key] = value;
		}
		roleOptionsData.ShapeshifterLeaveSkin = reader.ReadBoolean();
		roleOptionsData.ShapeshifterCooldown = (int)reader.ReadByte();
		roleOptionsData.ShapeshifterDuration = (int)reader.ReadByte();
		roleOptionsData.ScientistCooldown = (int)reader.ReadByte();
		roleOptionsData.GuardianAngelCooldown = (int)reader.ReadByte();
		roleOptionsData.EngineerCooldown = (int)reader.ReadByte();
		roleOptionsData.EngineerInVentMaxTime = (int)reader.ReadByte();
		roleOptionsData.ScientistBatteryCharge = (int)reader.ReadByte();
		roleOptionsData.ProtectionDurationSeconds = (int)reader.ReadByte();
		roleOptionsData.ImpostorsCanSeeProtect = reader.ReadBoolean();
		return roleOptionsData;
	}

	public void Serialize(MessageWriter writer)
	{
		writer.WritePacked(roleRates.Count);
		foreach (KeyValuePair<RoleTypes, RoleRate> roleRate in roleRates)
		{
			writer.Write((ushort)roleRate.Key);
			writer.Write((byte)roleRate.Value.MaxCount);
			writer.Write((byte)roleRate.Value.Chance);
		}
		writer.Write(ShapeshifterLeaveSkin);
		writer.Write((byte)ShapeshifterCooldown);
		writer.Write((byte)ShapeshifterDuration);
		writer.Write((byte)ScientistCooldown);
		writer.Write((byte)GuardianAngelCooldown);
		writer.Write((byte)EngineerCooldown);
		writer.Write((byte)EngineerInVentMaxTime);
		writer.Write((byte)ScientistBatteryCharge);
		writer.Write((byte)ProtectionDurationSeconds);
		writer.Write(ImpostorsCanSeeProtect);
	}

	public static void Serialize(BinaryWriter writer, RoleOptionsData roleOptions)
	{
		writer.Write(roleOptions.roleRates.Count);
		foreach (KeyValuePair<RoleTypes, RoleRate> roleRate in roleOptions.roleRates)
		{
			writer.Write((ushort)roleRate.Key);
			writer.Write((byte)roleRate.Value.MaxCount);
			writer.Write((byte)roleRate.Value.Chance);
		}
		writer.Write(roleOptions.ShapeshifterLeaveSkin);
		writer.Write((byte)roleOptions.ShapeshifterCooldown);
		writer.Write((byte)roleOptions.ShapeshifterDuration);
		writer.Write((byte)roleOptions.ScientistCooldown);
		writer.Write((byte)roleOptions.GuardianAngelCooldown);
		writer.Write((byte)roleOptions.EngineerCooldown);
		writer.Write((byte)roleOptions.EngineerInVentMaxTime);
		writer.Write((byte)roleOptions.ScientistBatteryCharge);
		writer.Write((byte)roleOptions.ProtectionDurationSeconds);
		writer.Write(roleOptions.ImpostorsCanSeeProtect);
	}

	public int GetNumPerGame(RoleTypes role)
	{
		if (roleRates.TryGetValue(role, out var value))
		{
			return value.MaxCount;
		}
		return 0;
	}

	public int GetChancePerGame(RoleTypes role)
	{
		if (roleRates.TryGetValue(role, out var value))
		{
			return value.Chance;
		}
		return 0;
	}

	public void SetRoleRate(RoleTypes role, int maxCount, int chance)
	{
		roleRates[role] = new RoleRate
		{
			MaxCount = maxCount,
			Chance = chance
		};
	}

	public void SetRoleRecommended(RoleTypes roleTYpe)
	{
	}
}
