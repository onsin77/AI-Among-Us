using System.Collections.Generic;
using Hazel;

namespace AmongUs.GameOptions;

public class RoleOptionsCollectionV07 : IRoleOptionsCollection
{
	private readonly Dictionary<RoleTypes, RoleDataV07> roles = new Dictionary<RoleTypes, RoleDataV07>();

	public static RoleOptionsCollectionV07 Deserialize(MessageReader reader)
	{
		RoleOptionsCollectionV07 roleOptionsCollectionV = new RoleOptionsCollectionV07();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			RoleTypes roleTypes = (RoleTypes)reader.ReadInt16();
			RoleRate rate = new RoleRate
			{
				MaxCount = reader.ReadByte(),
				Chance = reader.ReadByte()
			};
			RoleDataV07 roleDataV = new RoleDataV07(roleTypes);
			roleDataV.Rate = rate;
			MessageReader reader2 = reader.ReadMessage();
			roleDataV.RoleOptions?.Deserialize(reader2);
			roleOptionsCollectionV.roles[roleTypes] = roleDataV;
		}
		return roleOptionsCollectionV;
	}

	public static void Serialize(MessageWriter writer, RoleOptionsCollectionV07 options)
	{
		writer.WritePacked(options.roles.Count);
		foreach (KeyValuePair<RoleTypes, RoleDataV07> role in options.roles)
		{
			RoleDataV07 value = role.Value;
			writer.Write((ushort)role.Key);
			writer.Write((byte)value.Rate.MaxCount);
			writer.Write((byte)value.Rate.Chance);
			writer.StartMessage((byte)0);
			value.RoleOptions.Serialize(writer);
			writer.EndMessage();
		}
	}

	public int GetNumPerGame(RoleTypes role)
	{
		if (roles.TryGetValue(role, out var value))
		{
			return value.Rate.MaxCount;
		}
		return 0;
	}

	public int GetChancePerGame(RoleTypes role)
	{
		if (roles.TryGetValue(role, out var value))
		{
			return value.Rate.Chance;
		}
		return 0;
	}

	public void SetRoleRate(RoleTypes role, int maxCount, int chance)
	{
		if (!roles.TryGetValue(role, out var value))
		{
			value = new RoleDataV07(role);
		}
		value.Rate = new RoleRate
		{
			MaxCount = maxCount,
			Chance = chance
		};
	}

	public void SetRoleRecommended(RoleTypes role)
	{
		if (!roles.TryGetValue(role, out var value))
		{
			value = new RoleDataV07(role);
		}
		value.RoleOptions.SetRecommended();
	}

	public bool TryGetRoleOptions<T>(RoleTypes type, out T roleData) where T : class
	{
		roleData = null;
		if (!roles.TryGetValue(type, out var value))
		{
			return false;
		}
		roleData = value.RoleOptions as T;
		if (roleData == null)
		{
			return false;
		}
		return true;
	}

	public bool TryGetRoleRates(RoleTypes type, out RoleRate roleRates)
	{
		roleRates = default;
		if (!roles.TryGetValue(type, out var value))
		{
			return false;
		}
		roleRates = value.Rate;
		return true;
	}

	public void AddOrUpdateRole(RoleDataV07 role)
	{
		RoleTypes type = role.Type;
		RemoveRole(type);
		roles.Add(type, role);
	}

	public void RemoveRole(RoleTypes roleType)
	{
		roles.Remove(roleType);
	}
}
