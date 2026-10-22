using Rewired.Dev;

namespace RewiredConsts;

public static class Action
{
	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Pause the game")]
	public const int Pause = 1;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MoveHorizontal")]
	public const int MoveHorizontal = 2;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MoveHorizontalR")]
	public const int MoveHorizontalR = 54;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MoveVertical")]
	public const int MoveVertical = 3;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MoveVerticalR")]
	public const int MoveVerticalR = 55;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Map Toggle Sabotage")]
	public const int ToggleMap = 4;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Task Toggle")]
	public const int ToggleTasks = 5;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Use Vent as Impostor")]
	public const int UseVent = 50;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Interact Pet (Hold)")]
	public const int ActionPrimary = 6;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Kill")]
	public const int ActionSecondary = 8;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Report")]
	public const int ActionTertiary = 7;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Role Ability")]
	public const int ActionQuaternary = 49;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuHorizontal")]
	public const int MenuHorizontal = 9;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuVertical")]
	public const int MenuVertical = 10;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuConfirm")]
	public const int MenuConfirm = 11;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuCancel")]
	public const int MenuCancel = 12;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "TaskLHorizontal")]
	public const int TaskLHorizontal = 13;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "TaskLVertical")]
	public const int TaskLVertical = 14;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "TaskRHorizontal")]
	public const int TaskRHorizontal = 16;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "TaskRVertical")]
	public const int TaskRVertical = 17;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "ButtonChat")]
	public const int ButtonChat = 18;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "ButtonStart")]
	public const int ButtonStart = 19;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Task Left Trigger")]
	public const int TaskLT = 24;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Task Left Bumper")]
	public const int TaskLB = 20;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Task Right Trigger")]
	public const int TaskRT = 21;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Task Right Bumper")]
	public const int TaskRB = 22;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuUp")]
	public const int MenuUp = 25;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuDown")]
	public const int MenuDown = 26;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuRight")]
	public const int MenuRight = 27;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuLeft")]
	public const int MenuLeft = 28;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Other button used for tasks that isn t A (X on xbox controller)")]
	public const int TaskConfirmAlt = 29;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Secondary navigation using right stick")]
	public const int MenuHorizontalAlt = 30;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Secondary navigation using right stick")]
	public const int MenuVerticalAlt = 31;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Because we ve hit the point where we need all 4 face buttons when in a task")]
	public const int TaskConfirmTertiary = 32;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Open virtual keyboard")]
	public const int ButtonKeyboard = 33;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuRT")]
	public const int MenuRT = 34;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuLT")]
	public const int MenuLT = 35;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "chat ban button")]
	public const int ButtonBan = 36;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "AccountPicker")]
	public const int AccountPicker = 37;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "toggle in-game player list")]
	public const int TogglePlayerList = 38;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Move Left")]
	public const int KBLeft = 39;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Move Right")]
	public const int KBRight = 40;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Move Down")]
	public const int KBDown = 42;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Move Up")]
	public const int KBUp = 44;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Clicking in the right stick")]
	public const int TaskR3Click = 51;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuLB")]
	public const int MenuLB = 52;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MenuRB")]
	public const int MenuRB = 53;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolRB")]
	public const int DebugToolRB = 56;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolLB")]
	public const int DebugToolLB = 57;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolRT")]
	public const int DebugToolRT = 58;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolLT")]
	public const int DebugToolLT = 59;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolStart")]
	public const int DebugToolStart = 60;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolNavLeft")]
	public const int DebugToolNavLeft = 61;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolNavRight")]
	public const int DebugToolNavRight = 62;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolVertical")]
	public const int DebugToolVertical = 63;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolConfirm")]
	public const int DebugToolConfirm = 65;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolCancel")]
	public const int DebugToolCancel = 66;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolCopy")]
	public const int DebugToolCopy = 67;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "DebugToolClear")]
	public const int DebugToolClear = 68;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Quick Chat Favorite")]
	public const int QuickChatFavorite = 69;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Quick Chat Clear")]
	public const int QuickChatClear = 70;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Quick Chat Open")]
	public const int QuickChatOpen = 72;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Chat Send")]
	public const int ChatSend = 71;

	[ActionIdFieldInfo(categoryName = "System", friendlyName = "Restricted")]
	public const int Restricted = 47;

	[ActionIdFieldInfo(categoryName = "System", friendlyName = "Restricted2")]
	public const int Restricted2 = 48;
}
