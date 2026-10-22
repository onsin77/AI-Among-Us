using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

public class HeliSabotageSystem : MonoBehaviour, ISystemType, ICriticalSabotage, IActivatable
{
	private struct ActiveConsoleData(byte playerId, byte consoleId)
	{
		public readonly byte PlayerId = playerId;

		public readonly byte ConsoleId = consoleId;
	}

	public enum Tags
	{
		DamageBit = 128,
		ActiveBit = 64,
		DeactiveBit = 32,
		FixBit = 16
	}

	public const float CountdownStopped = 10000f;

	public const byte TagMask = 240;

	public const byte IdMask = 15;

	public SpriteRenderer Helicopter;

	public AnimationCurve ScaleCurve;

	private const float CharlesDuration = 90f;

	private const float CodeActiveDuration = 10f;

	private const float SyncRate = 1f;

	private HashSet<ActiveConsoleData> ActiveConsoles = new HashSet<ActiveConsoleData>();

	private HashSet<byte> CompletedConsoles = new HashSet<byte>();

	private float codeResetTimer;

	private float syncTimer;

	private bool wasActive;

	public float Countdown { get; private set; } = 10000f;

	public bool IsActive => CompletedConsoles.Count < 2;

	public float PercentActive => codeResetTimer / 10f;

	public int TargetCode { get; private set; }

	public bool IsDirty { get; private set; }

	public int UserCount => 0;

	public HeliSabotageSystem()
	{
		ClearSabotage();
	}

	public void ClearSabotage()
	{
		Countdown = 10000f;
		CompletedConsoles.Add(0);
		CompletedConsoles.Add(1);
		IsDirty = true;
	}

	public void Deteriorate(float deltaTime)
	{
		if (IsActive)
		{
			if (!wasActive)
			{
				codeResetTimer = 0f;
			}
			wasActive = true;
			UpdateHeliSize();
			Countdown -= deltaTime;
			codeResetTimer -= deltaTime;
			if (codeResetTimer <= 0f)
			{
				TargetCode = IntRange.Next(0, 99999);
				codeResetTimer = 10f;
				CompletedConsoles.Clear();
			}
			if (!PlayerTask.PlayerHasTaskOfType<HeliCharlesTask>(PlayerControl.LocalPlayer))
			{
				PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.HeliSabotage);
			}
			syncTimer -= deltaTime;
			if (syncTimer < 0f)
			{
				syncTimer = 1f;
				IsDirty = true;
			}
		}
		else
		{
			wasActive = false;
			((Component)Helicopter).gameObject.SetActive(false);
			DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
		}
	}

	private void UpdateHeliSize()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		float num = ScaleCurve.Evaluate(1f - Countdown / 90f);
		((Component)Helicopter).gameObject.SetActive(true);
		if (num > 0.8f)
		{
			num -= 0.8f;
			float num2 = Mathf.Lerp(0.05f, 1.2f, num);
			((Component)Helicopter).transform.localScale = new Vector3(num2, num2, num2);
			Vector3 localPosition = ((Component)Helicopter).transform.localPosition;
			localPosition.y = Mathf.Lerp(2.17f, 1.5f, num);
			((Component)Helicopter).transform.localPosition = localPosition;
		}
		else
		{
			((Component)Helicopter).transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			Vector3 localPosition2 = ((Component)Helicopter).transform.localPosition;
			localPosition2.y = 2.17f;
			((Component)Helicopter).transform.localPosition = localPosition2;
		}
	}

	internal bool IsConsoleActive(int consoleId)
	{
		return ActiveConsoles.Any((ActiveConsoleData s) => s.ConsoleId == (byte)consoleId);
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
			codeResetTimer = -1f;
			Countdown = 90f;
			CompletedConsoles.Clear();
			ActiveConsoles.Clear();
			break;
		case Tags.ActiveBit:
			ActiveConsoles.Add(new ActiveConsoleData(player.PlayerId, b2));
			break;
		case Tags.DeactiveBit:
			ActiveConsoles.Remove(new ActiveConsoleData(player.PlayerId, b2));
			break;
		case Tags.FixBit:
			codeResetTimer = 10f;
			CompletedConsoles.Add(b2);
			break;
		}
		IsDirty = true;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(Countdown);
		writer.Write(codeResetTimer);
		writer.WritePacked(ActiveConsoles.Count);
		foreach (ActiveConsoleData activeConsole in ActiveConsoles)
		{
			writer.Write(activeConsole.PlayerId);
			writer.Write(activeConsole.ConsoleId);
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
		Countdown = reader.ReadSingle();
		codeResetTimer = reader.ReadSingle();
		int num = reader.ReadPackedInt32();
		ActiveConsoles.Clear();
		for (int i = 0; i < num; i++)
		{
			ActiveConsoles.Add(new ActiveConsoleData(reader.ReadByte(), reader.ReadByte()));
		}
		int num2 = reader.ReadPackedInt32();
		CompletedConsoles.Clear();
		for (int j = 0; j < num2; j++)
		{
			CompletedConsoles.Add(reader.ReadByte());
		}
	}
}
