using System;

[Serializable]
public struct ControllerNavigation
{
	public enum Mode
	{
		Automatic,
		Explicit
	}

	public Mode mode;

	public UiElement selectOnUp;

	public UiElement selectOnDown;

	public UiElement selectOnLeft;

	public UiElement selectOnRight;

	public static ControllerNavigation defaultNavigation { get; }
}
