using UnityEngine;

public interface IVirtualJoystick
{
	Vector2 DeltaL { get; }

	Vector2 DeltaR { get; }

	void ToggleVisuals(bool on);
}
