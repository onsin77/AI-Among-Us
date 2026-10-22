using System;

namespace AmongUs.GameOptions;

public interface IGameOptions
{
	byte Version { get; }

	GameModes GameMode { get; }

	int MaxPlayers { get; }

	GameKeywords Keywords { get; }

	byte MapId { get; }

	int NumImpostors { get; }

	int TotalTaskCount { get; }

	bool IsDefaults { get; }

	IRoleOptionsCollection RoleOptions { get; }

	bool AreInvalid(int maxExpectedPlayers);

	void SetRecommendations(int numPlayers, bool isOnlineGame);

	void SetByte(ByteOptionNames optionName, byte value);

	void SetFloat(FloatOptionNames optionName, float value);

	void SetBool(BoolOptionNames optionName, bool value);

	void SetInt(Int32OptionNames optionName, int value);

	void SetUInt(UInt32OptionNames optionName, uint value);

	[Obsolete("Prefer usage of TryGetByte")]
	byte GetByte(ByteOptionNames optionName);

	[Obsolete("Prefer usage of TryGetFloat")]
	float GetFloat(FloatOptionNames optionName);

	[Obsolete("Prefer usage of TryGetBool")]
	bool GetBool(BoolOptionNames optionName);

	[Obsolete("Prefer usage of TryGetInt")]
	int GetInt(Int32OptionNames optionName);

	[Obsolete("Prefer usage of TryGetIntArray")]
	int[] GetIntArray(Int32ArrayOptionNames optionName);

	[Obsolete("Prefer usage of TryGetFloatArray")]
	float[] GetFloatArray(FloatArrayOptionNames optionName);

	bool TryGetByte(ByteOptionNames optionName, out byte value);

	bool TryGetFloat(FloatOptionNames optionName, out float value);

	bool TryGetBool(BoolOptionNames optionName, out bool value);

	bool TryGetInt(Int32OptionNames optionName, out int value);

	bool TryGetIntArray(Int32ArrayOptionNames optionName, out int[] value);

	bool TryGetFloatArray(FloatArrayOptionNames optionName, out float[] value);
}
