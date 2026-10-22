namespace AmongUsServer;

public enum ExtensionFailureReasons : byte
{
	Invalid,
	WrongGameState,
	OutOfExtensionThreshold,
	NoExtensionAvailable,
	WrongExtensionId,
	ExtensionAlreadyRequested
}
