using System.Collections.Generic;
using UnityEngine;

public class SecurityLogBehaviour : MonoBehaviour
{
	public enum SecurityLogLocations
	{
		North,
		Southeast,
		Southwest
	}

	public struct SecurityLogEntry(byte playerId, SecurityLogLocations location)
	{
		public byte PlayerId = playerId;

		public SecurityLogLocations Location = location;
	}

	public const byte ConsoleMask = 240;

	public const byte PlayerMask = 15;

	public Color[] BarColors = new Color[3]
	{
		Color32.op_Implicit(new Color32((byte)33, (byte)77, (byte)173, (byte)128)),
		Color32.op_Implicit(new Color32((byte)173, (byte)81, (byte)16, (byte)128)),
		Color32.op_Implicit(new Color32((byte)16, (byte)97, (byte)8, (byte)128))
	};

	public readonly List<SecurityLogEntry> LogEntries = new List<SecurityLogEntry>();

	public bool HasNew;

	public void LogPlayer(PlayerControl player, SecurityLogLocations location)
	{
		HasNew = true;
		LogEntries.Add(new SecurityLogEntry(player.PlayerId, location));
		if (LogEntries.Count > 20)
		{
			LogEntries.RemoveAt(0);
		}
	}

	public SecurityLogBehaviour()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
	}
}
