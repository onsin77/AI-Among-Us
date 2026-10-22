using System.Linq;
using AmongUs.GameOptions;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatContext
{
	private static readonly StringNames[] TASKS_TO_IGNORE = new StringNames[2]
	{
		StringNames.None,
		StringNames.ImpostorTask
	};

	private static readonly RoleTypes[] ROLES_TO_IGNORE = new RoleTypes[2]
	{
		RoleTypes.CrewmateGhost,
		RoleTypes.ImpostorGhost
	};

	private QuickChatData data;

	private PlayerControl localPlayer;

	private PlayerControl[] players;

	private PlayerControl[] deadPlayers;

	private StringNames[] locations;

	private StringNames[] roles;

	private StringNames[] systems;

	private StringNames[] tasks;

	private QuickChatContextPhase phase;

	private GameModes gameMode;

	private MapNames map;

	private int meetingCount;

	public QuickChatData Data => data;

	public PlayerControl[] Players => players;

	public PlayerControl[] DeadPlayers => deadPlayers;

	public StringNames[] Locations => locations;

	public StringNames[] Roles => roles;

	public StringNames[] Systems => systems;

	public StringNames[] Tasks => tasks;

	public QuickChatContextPhase Phase => phase;

	public GameModes GameMode => gameMode;

	public MapNames Map => map;

	public int MeetingCount => meetingCount;

	public static QuickChatContext Current(QuickChatData data)
	{
		QuickChatContext quickChatContext = new QuickChatContext(data);
		quickChatContext.UpdateToCurrent();
		return quickChatContext;
	}

	public QuickChatContext(QuickChatData data)
	{
		this.data = data;
	}

	private void UpdateToCurrent()
	{
		gameMode = GameModes.None;
		phase = QuickChatContextPhase.None;
		map = (MapNames)(-1);
		meetingCount = 0;
		localPlayer = PlayerControl.LocalPlayer;
		players = PlayerControl.AllPlayerControls.OrderBy((PlayerControl x) => (!((Object)(object)x == (Object)(object)localPlayer)) ? 1 : 0).ToArray();
		deadPlayers = players.Where((PlayerControl p) => p.Data.IsDead).ToArray();
		if ((Object)(object)GameManager.Instance != (Object)null)
		{
			gameMode = ((!GameManager.Instance.IsHideAndSeek()) ? GameModes.Normal : GameModes.HideNSeek);
		}
		if (Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
		{
			UpdateWithCurrentLobby();
		}
		else if (Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			UpdateWithCurrentMap();
		}
	}

	private void UpdateWithCurrentLobby()
	{
		phase = QuickChatContextPhase.Lobby;
		locations = new StringNames[6]
		{
			StringNames.QCLocationLaptop,
			StringNames.MapNameSkeld,
			StringNames.MapNameMira,
			StringNames.MapNamePolus,
			StringNames.MapNameAirship,
			StringNames.MapNameFungle
		};
		roles = (from role in DestroyableSingleton<RoleManager>.Instance.AllRoles
			where !ROLES_TO_IGNORE.Contains(role.Role)
			select role.StringName).Distinct().ToArray();
		systems = new StringNames[2]
		{
			StringNames.QCSystemsStart,
			StringNames.QCSystemsKick
		};
		tasks = new StringNames[0];
	}

	private void UpdateWithCurrentMap()
	{
		ShipStatus instance = ShipStatus.Instance;
		phase = QuickChatContextPhase.Game;
		map = GetCurrentMapID(instance);
		meetingCount = TempData.MeetingCount;
		locations = instance.AllRooms.Select((PlainShipRoom room) => DestroyableSingleton<TranslationController>.Instance.GetSystemName(room.RoomId)).Distinct().ToArray();
		roles = (from role in DestroyableSingleton<RoleManager>.Instance.AllRoles
			where !ROLES_TO_IGNORE.Contains(role.Role)
			select role.StringName).Concat(new StringNames[1] { StringNames.Ghost }).Distinct().ToArray();
		systems = instance.SystemNames;
		tasks = (from stringName in instance.ExtraTaskNames.Concat(from task in instance.GetAllTasks()
				select DestroyableSingleton<TranslationController>.Instance.GetTaskName(task.TaskType))
			where !TASKS_TO_IGNORE.Contains(stringName)
			select stringName).Distinct().ToArray();
	}

	private MapNames GetCurrentMapID(ShipStatus ship)
	{
		if (!(ship is SkeldShipStatus))
		{
			if (!(ship is PolusShipStatus))
			{
				if (!(ship is MiraShipStatus))
				{
					if (!(ship is AirshipStatus))
					{
						if (!(ship is FungleShipStatus))
						{
							QuickChatMenu.Logger.Error(string.Format("[{0}] {1}() :: {2} of type {3} has not been configured.", "QuickChatContext", "GetCurrentMapID", "ShipStatus", ((object)ship).GetType()));
							return (MapNames)(-1);
						}
						return MapNames.Fungle;
					}
					return MapNames.Airship;
				}
				return MapNames.Mira;
			}
			return MapNames.Polus;
		}
		return MapNames.Skeld;
	}

	public bool HasPlayersWithRole(RoleTypes role)
	{
		PlayerControl[] array = players;
		foreach (PlayerControl playerControl in array)
		{
			if (playerControl.Data.RoleWhenAlive.HasValue && playerControl.Data.RoleWhenAlive.Value == role)
			{
				return true;
			}
		}
		return false;
	}
}
