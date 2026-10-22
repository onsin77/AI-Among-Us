using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using UnityEngine;

public abstract class RoleBehaviour : MonoBehaviour
{
	public RoleTypes Role;

	public StringNames StringName = StringNames.Crewmate;

	public StringNames BlurbName = StringNames.CrewmateBlurb;

	public StringNames BlurbNameMed = StringNames.CrewmateBlurb;

	public StringNames BlurbNameLong = StringNames.CrewmateBlurb;

	public Color NameColor = Color.white;

	public bool TasksCountTowardProgress = true;

	public bool CanUseKillButton;

	public bool CanBeKilled = true;

	public bool CanVent;

	public bool AffectedByLightAffectors = true;

	public int MaxCount = 15;

	public RoleTeamTypes TeamType;

	public AbilityButtonSettings Ability;

	public RoleTypes DefaultGhostRole;

	public AudioClip UseSound;

	public AudioClip IntroSound;

	protected PlayerControl Player;

	protected AbilityButton buttonManager;

	private static List<PlayerControl> tempPlayerList = new List<PlayerControl>();

	public bool IsImpostor => TeamType == RoleTeamTypes.Impostor;

	public abstract bool IsDead { get; }

	public string NiceName => DestroyableSingleton<TranslationController>.Instance.GetString(StringName);

	public string Blurb => DestroyableSingleton<TranslationController>.Instance.GetString(BlurbName);

	public string BlurbMed => DestroyableSingleton<TranslationController>.Instance.GetString(BlurbNameMed);

	public string BlurbLong => DestroyableSingleton<TranslationController>.Instance.GetString(BlurbNameLong);

	public bool IsSimpleRole
	{
		get
		{
			if (Role != RoleTypes.Crewmate)
			{
				return Role == RoleTypes.Impostor;
			}
			return true;
		}
	}

	public Color TeamColor
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			if (!IsImpostor)
			{
				return Palette.CrewmateBlue;
			}
			return Palette.ImpostorRed;
		}
	}

	public virtual bool IsAffectedByComms
	{
		get
		{
			if (CommsSabotaged)
			{
				return !Player.Data.Role.IsImpostor;
			}
			return false;
		}
	}

	protected bool CommsSabotaged
	{
		get
		{
			if ((Object)(object)Player != (Object)null)
			{
				return PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(Player);
			}
			return false;
		}
	}

	protected static void AppendTaskHint(RoleBehaviour role, StringBuilder taskStringBuilder)
	{
		taskStringBuilder.AppendFormat("\n{0} {1}\n{2}", role.NiceName, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoleHint), role.BlurbMed);
	}

	public virtual bool CanUse(IUsable console)
	{
		return false;
	}

	public virtual bool DidWin(GameOverReason gameOverReason)
	{
		return false;
	}

	public virtual void Deinitialize(PlayerControl targetPlayer)
	{
	}

	public virtual void SpawnTaskHeader(PlayerControl playerControl)
	{
	}

	public virtual void UseAbility()
	{
	}

	public virtual void OnVotingComplete()
	{
	}

	public virtual void Initialize(PlayerControl player)
	{
		Player = player;
		if (player.AmOwner)
		{
			DestroyableSingleton<HudManager>.Instance.SetHudActive(player, this, isActive: true);
			PlayerNameColor.SetForRoleDirectly(player, this);
			InitializeAbilityButton();
		}
	}

	public virtual void SetUsableTarget(IUsable target)
	{
	}

	public virtual void SetPlayerTarget(PlayerControl target)
	{
	}

	public virtual void SetCooldown()
	{
	}

	private void InitializeAbilityButton()
	{
		buttonManager = DestroyableSingleton<HudManager>.Instance.AbilityButton;
		if (Object.op_Implicit((Object)(object)Ability))
		{
			buttonManager.SetFromSettings(Ability);
			buttonManager.SetEnabled();
			((Component)buttonManager).gameObject.SetActive(true);
			buttonManager.SetCoolDown(0f, 1f);
		}
		else
		{
			((Component)buttonManager).gameObject.SetActive(false);
		}
	}

	private void SetAbilityUsesRemaining(int num)
	{
		buttonManager.SetUsesRemaining(num);
	}

	protected virtual bool IsValidTarget(GameData.PlayerInfo target)
	{
		if (target == null || target.Disconnected || target.IsDead || target.PlayerId == Player.PlayerId || (Object)(object)target.Role == (Object)null || (Object)(object)target.Object == (Object)null || target.Object.inVent || target.Object.inMovingPlat)
		{
			return false;
		}
		return true;
	}

	public virtual PlayerControl FindClosestTarget()
	{
		return null;
	}

	protected virtual float GetAbilityDistance()
	{
		return GameManager.Instance.LogicOptions.GetKillDistance();
	}

	protected static List<PlayerControl> GetTempPlayerList()
	{
		return tempPlayerList;
	}

	public List<PlayerControl> GetPlayersInAbilityRangeSorted(List<PlayerControl> outputList)
	{
		return GetPlayersInAbilityRangeSorted(outputList, ignoreColliders: false);
	}

	protected List<PlayerControl> GetPlayersInAbilityRangeSorted(List<PlayerControl> outputList, bool ignoreColliders)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		outputList.Clear();
		float abilityDistance = GetAbilityDistance();
		Vector2 myPos = Player.GetTruePosition();
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			GameData.PlayerInfo playerInfo = allPlayers[i];
			if (!IsValidTarget(playerInfo))
			{
				continue;
			}
			PlayerControl playerControl = playerInfo.Object;
			if (Object.op_Implicit((Object)(object)playerControl) && ((Behaviour)playerControl.Collider).enabled)
			{
				Vector2 val = playerControl.GetTruePosition() - myPos;
				float magnitude = val.magnitude;
				if (magnitude <= abilityDistance && (ignoreColliders || !PhysicsHelpers.AnyNonTriggersBetween(myPos, val.normalized, magnitude, Constants.ShipAndObjectsMask)))
				{
					outputList.Add(playerControl);
				}
			}
		}
		outputList.Sort((PlayerControl a, PlayerControl b) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val2 = a.GetTruePosition() - myPos;
			float magnitude2 = val2.magnitude;
			val2 = b.GetTruePosition() - myPos;
			float magnitude3 = val2.magnitude;
			if (magnitude2 > magnitude3)
			{
				return 1;
			}
			return (magnitude2 < magnitude3) ? (-1) : 0;
		});
		return outputList;
	}

	public virtual void AdjustTasks(PlayerControl player)
	{
	}

	public virtual void AppendTaskHint(StringBuilder taskStringBuilder)
	{
		AppendTaskHint(this, taskStringBuilder);
	}

	protected RoleBehaviour()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
	}
}
