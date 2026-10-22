using System.Collections.Generic;
using Hazel;
using UnityEngine;

public class MushroomMixupSabotageSystem : MonoBehaviour, ISystemType, IActivatable
{
	public enum Operation
	{
		None,
		TriggerSabotage
	}

	private enum State
	{
		Inactive,
		JustTriggered,
		IdleButMixedUp
	}

	private struct CondensedOutfit
	{
		public byte HatIndex;

		public byte VisorIndex;

		public byte SkinIndex;

		public byte PetIndex;

		public byte ColorPlayerId;

		public void Serialize(MessageWriter writer)
		{
			writer.Write(ColorPlayerId);
			writer.Write(HatIndex);
			writer.Write(VisorIndex);
			writer.Write(SkinIndex);
			writer.Write(PetIndex);
		}

		public void Deserialize(MessageReader reader)
		{
			ColorPlayerId = reader.ReadByte();
			HatIndex = reader.ReadByte();
			VisorIndex = reader.ReadByte();
			SkinIndex = reader.ReadByte();
			PetIndex = reader.ReadByte();
		}
	}

	private const float SYNC_RATE = 2f;

	private const float VIBRATION_INTENSITY = 0.3f;

	[SerializeField]
	[Header("Cosmetics")]
	[Range(0f, 1f)]
	private float skinEmptyChance;

	private List<string> skinIds;

	[Range(0f, 1f)]
	[SerializeField]
	private float hatEmptyChance;

	[SerializeField]
	private string[] hatIds;

	[SerializeField]
	[Range(0f, 1f)]
	private float visorEmptyChance;

	[SerializeField]
	private string[] visorIds;

	[SerializeField]
	[Range(0f, 1f)]
	private float petEmptyChance;

	[SerializeField]
	private string[] petIds;

	[SerializeField]
	[Header("Healing")]
	private float secondsForAutoHeal;

	[SerializeField]
	[Header("Animations")]
	private MushroomMixupScreenTint screenTint;

	[SerializeField]
	private MushroomMixupPlayerAnimation playerAnimationPrefab;

	[SerializeField]
	private AudioClip activateSfx;

	[SerializeField]
	private AudioClip deactivateSfx;

	private readonly Dictionary<byte, CondensedOutfit> currentMixups = new Dictionary<byte, CondensedOutfit>();

	private readonly Dictionary<byte, GameData.PlayerOutfit> cachedOutfitsByPlayerId = new Dictionary<byte, GameData.PlayerOutfit>();

	private readonly Dictionary<byte, GameData.PlayerOutfit> deadAndDcPlayerOutfitsByPlayerId = new Dictionary<byte, GameData.PlayerOutfit>();

	private State currentState;

	private float currentSecondsUntilHeal;

	private float syncTimer;

	public bool IsDirty { get; private set; }

	public bool IsActive => currentSecondsUntilHeal > 0f;

	public float CurrentSecondsUntilHeal => currentSecondsUntilHeal;

	private void Start()
	{
		skinIds = new List<string>();
		skinIds.Add("skin_None");
		PopulateSkinsFromPlayers();
		skinIds.Sort();
	}

	private void PopulateSkinsFromPlayers()
	{
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			if (!((Object)(object)allPlayerControl == (Object)null) && allPlayerControl.CurrentOutfit != null && !((Object)(object)((Component)allPlayerControl).gameObject == (Object)null))
			{
				GameData.PlayerOutfit currentOutfit = allPlayerControl.CurrentOutfit;
				if (!skinIds.Contains(currentOutfit.SkinId))
				{
					skinIds.Add(currentOutfit.SkinId);
				}
			}
		}
	}

	public void Deteriorate(float deltaTime)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			List<byte> list = new List<byte>();
			foreach (KeyValuePair<byte, CondensedOutfit> currentMixup in currentMixups)
			{
				byte key = currentMixup.Key;
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(key);
				if (playerById == null || playerById.Disconnected || playerById.IsDead)
				{
					list.Add(key);
				}
			}
			if (list.Count > 0)
			{
				foreach (byte item in list)
				{
					currentMixups.Remove(item);
					deadAndDcPlayerOutfitsByPlayerId[item] = cachedOutfitsByPlayerId[item];
					cachedOutfitsByPlayerId.Remove(item);
				}
				IsDirty = true;
			}
		}
		if (!IsActive)
		{
			return;
		}
		currentSecondsUntilHeal -= deltaTime;
		syncTimer += deltaTime;
		if (syncTimer > 2f)
		{
			syncTimer = 0f;
			IsDirty = true;
		}
		if (!(currentSecondsUntilHeal <= 0f))
		{
			return;
		}
		foreach (KeyValuePair<byte, CondensedOutfit> playerIdToOutfit in currentMixups)
		{
			PlayerControl player = PlayerControl.AllPlayerControls.Find((PlayerControl p) => p.PlayerId == playerIdToOutfit.Key);
			MushroomMixupPlayerAnimation mushroomMixupPlayerAnimation = Object.Instantiate<MushroomMixupPlayerAnimation>(playerAnimationPrefab, ((Component)player).transform, false);
			mushroomMixupPlayerAnimation.SetPlayer(player);
			mushroomMixupPlayerAnimation.StartAnimation(null, () =>
			{
				player.FixMixedUpOutfit();
			});
		}
		screenTint.Deactivate();
		if (!Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			SoundManager.Instance.PlaySound(deactivateSfx, loop: false);
			VibrationManager.Vibrate(0.3f, 0.3f, 1f, VibrationManager.VibrationFalloff.Linear);
		}
		currentState = State.Inactive;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Operation operation = (Operation)msgReader.ReadByte();
		if (operation == Operation.TriggerSabotage)
		{
			Host_GenerateRandomOutfits();
			MushroomMixUp();
			currentState = State.JustTriggered;
			currentSecondsUntilHeal = secondsForAutoHeal;
			IsDirty = true;
		}
		else
		{
			Logger.GlobalInstance.Error(string.Format("Unexpected operation {0} to {1}", operation, "MushroomMixupSabotageSystem"));
		}
	}

	private void MushroomMixUp()
	{
		if (!PlayerControl.LocalPlayer.Data.IsDead && !PlayerTask.TryGetTaskOfType<MushroomMixupSabotageTask>(PlayerControl.LocalPlayer, out var _))
		{
			PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.MushroomMixupSabotage);
		}
		UpdatePlayerOutfits();
		if (!PlayerControl.LocalPlayer.Data.IsDead)
		{
			screenTint.Activate();
		}
		VibrationManager.Vibrate(0.3f, 0.3f, 1f, VibrationManager.VibrationFalloff.Linear);
		SoundManager.Instance.PlaySound(activateSfx, loop: false);
	}

	private void Host_GenerateRandomOutfits()
	{
		currentMixups.Clear();
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			if (!((Object)(object)allPlayerControl == (Object)null) && !allPlayerControl.Data.IsDead)
			{
				CondensedOutfit value = GenerateRandomOutfit(allPlayerControl.PlayerId);
				currentMixups.Add(allPlayerControl.PlayerId, value);
			}
		}
	}

	private void UpdatePlayerOutfits()
	{
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			if (!((Object)(object)allPlayerControl == (Object)null) && !allPlayerControl.Data.IsDead && currentMixups.ContainsKey(allPlayerControl.PlayerId))
			{
				CondensedOutfit condensedOutfit = currentMixups[allPlayerControl.PlayerId];
				GameData.PlayerOutfit playerOutfit = ConvertToPlayerOutfit(condensedOutfit);
				PlayerControl player = allPlayerControl;
				playerOutfit.NamePlateId = player.Data.DefaultOutfit.NamePlateId;
				MushroomMixupPlayerAnimation mushroomMixupPlayerAnimation = Object.Instantiate<MushroomMixupPlayerAnimation>(playerAnimationPrefab, ((Component)player).transform, false);
				mushroomMixupPlayerAnimation.SetPlayer(player);
				mushroomMixupPlayerAnimation.StartAnimation(ShipStatus.Instance.CosmeticsCache.CoPopulateFromOutfit(playerOutfit), () =>
				{
					player.MixUpOutfit(playerOutfit);
				});
			}
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		if (cachedOutfitsByPlayerId.Count == 0)
		{
			foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
			{
				cachedOutfitsByPlayerId[allPlayerControl.PlayerId] = allPlayerControl.Data.DefaultOutfit;
			}
		}
		writer.Write((byte)currentState);
		writer.Write(currentSecondsUntilHeal);
		writer.WritePacked(currentMixups.Count);
		foreach (KeyValuePair<byte, CondensedOutfit> currentMixup in currentMixups)
		{
			writer.Write(currentMixup.Key);
			currentMixup.Value.Serialize(writer);
		}
		currentState = ((currentState == State.JustTriggered) ? State.IdleButMixedUp : currentState);
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		if (cachedOutfitsByPlayerId.Count == 0)
		{
			foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
			{
				cachedOutfitsByPlayerId[allPlayerControl.PlayerId] = allPlayerControl.Data.DefaultOutfit;
			}
		}
		State state = currentState;
		currentState = (State)reader.ReadByte();
		currentSecondsUntilHeal = reader.ReadSingle();
		currentMixups.Clear();
		int num = reader.ReadByte();
		for (int i = 0; i < num; i++)
		{
			byte key = reader.ReadByte();
			CondensedOutfit value = default;
			value.Deserialize(reader);
			currentMixups[key] = value;
		}
		if (state != currentState && currentState == State.JustTriggered)
		{
			MushroomMixUp();
			currentState = State.IdleButMixedUp;
		}
	}

	private CondensedOutfit GenerateRandomOutfit(int playerId)
	{
		List<byte> list = new List<byte>(cachedOutfitsByPlayerId.Keys);
		list.Remove((byte)playerId);
		return new CondensedOutfit
		{
			ColorPlayerId = list.Random(),
			HatIndex = ((Random.Range(0f, 1f) < hatEmptyChance) ? byte.MaxValue : ((byte)hatIds.RandomIdx())),
			VisorIndex = ((Random.Range(0f, 1f) < visorEmptyChance) ? byte.MaxValue : ((byte)visorIds.RandomIdx())),
			SkinIndex = ((Random.Range(0f, 1f) < skinEmptyChance) ? byte.MaxValue : ((byte)skinIds.RandomIdx())),
			PetIndex = ((Random.Range(0f, 1f) < petEmptyChance) ? byte.MaxValue : ((byte)petIds.RandomIdx()))
		};
	}

	private GameData.PlayerOutfit ConvertToPlayerOutfit(CondensedOutfit condensedOutfit)
	{
		return new GameData.PlayerOutfit
		{
			ColorId = (cachedOutfitsByPlayerId.ContainsKey(condensedOutfit.ColorPlayerId) ? cachedOutfitsByPlayerId[condensedOutfit.ColorPlayerId].ColorId : deadAndDcPlayerOutfitsByPlayerId[condensedOutfit.ColorPlayerId].ColorId),
			HatId = ((condensedOutfit.HatIndex >= hatIds.Length) ? "hat_NoHat" : hatIds[condensedOutfit.HatIndex]),
			SkinId = ((condensedOutfit.SkinIndex >= skinIds.Count) ? "skin_None" : skinIds[condensedOutfit.SkinIndex]),
			VisorId = ((condensedOutfit.VisorIndex >= visorIds.Length) ? "visor_EmptyVisor" : visorIds[condensedOutfit.VisorIndex]),
			PetId = ((condensedOutfit.PetIndex >= petIds.Length) ? "pet_EmptyPet" : petIds[condensedOutfit.PetIndex])
		};
	}
}
