using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AirshipStatus : ShipStatus
{
	public SpawnInMinigame SpawnInGame;

	public MovingPlatformBehaviour GapPlatform;

	public ParticleSystem ShowerParticles;

	public LightAffector[] LightAffectors;

	public GameObject outOfOrderPlat;

	protected override void OnEnable()
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (Systems == null)
		{
			ElectricalDoors componentInChildren = ((Component)this).GetComponentInChildren<ElectricalDoors>();
			if (AmongUsClient.Instance.AmHost)
			{
				componentInChildren.Initialize();
			}
			Systems = new Dictionary<SystemTypes, ISystemType>(SystemTypeComparer.Instance)
			{
				{
					SystemTypes.Electrical,
					new SwitchSystem()
				},
				{
					SystemTypes.MedBay,
					new MedScanSystem()
				},
				{
					SystemTypes.Doors,
					new DoorsSystemType()
				},
				{
					SystemTypes.Comms,
					new HudOverrideSystemType()
				},
				{
					SystemTypes.GapRoom,
					((Component)this).GetComponentInChildren<MovingPlatformBehaviour>()
				},
				{
					SystemTypes.HeliSabotage,
					((Component)this).GetComponentInChildren<HeliSabotageSystem>()
				},
				{
					SystemTypes.Decontamination,
					componentInChildren
				},
				{
					SystemTypes.Decontamination2,
					new AutoDoorsSystemType()
				},
				{
					SystemTypes.Security,
					new SecurityCameraSystemType()
				},
				{
					SystemTypes.Ventilation,
					new VentilationSystem()
				}
			};
			Camera main = Camera.main;
			main.backgroundColor = CameraColor;
			FollowerCamera component = ((Component)main).GetComponent<FollowerCamera>();
			component.shakeAmount = 0f;
			component.shakePeriod = 0f;
			Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(Systems.Values.Where((ISystemType i) => i is IActivatable).Cast<IActivatable>().ToArray()));
			if (GameManager.Instance.IsHideAndSeek())
			{
				outOfOrderPlat.SetActive(true);
			}
		}
	}

	public override void RepairCriticalSabotages()
	{
		(ShipStatus.Instance.Systems[SystemTypes.HeliSabotage] as HeliSabotageSystem).ClearSabotage();
	}

	public override float CalculateLightRadius(GameData.PlayerInfo player)
	{
		float num = base.CalculateLightRadius(player);
		if (player.Role.AffectedByLightAffectors)
		{
			LightAffector[] lightAffectors = LightAffectors;
			foreach (LightAffector lightAffector in lightAffectors)
			{
				if (Object.op_Implicit((Object)(object)player.Object) && player.Object.Collider.IsTouching(lightAffector.Hitbox))
				{
					num *= lightAffector.Multiplier;
				}
			}
		}
		return num;
	}

	public override void OnMeetingCalled()
	{
		GapPlatform.MeetingCalled();
	}

	public override void SpawnPlayer(PlayerControl player, int numPlayers, bool initialSpawn)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			player.NetTransform.SnapTo(new Vector2(-0.66f, -0.5f));
		}
		else if (initialSpawn)
		{
			player.NetTransform.SnapTo(InitialSpawnCenter);
		}
		else
		{
			player.NetTransform.Halt();
		}
	}

	public override IEnumerator PrespawnStep()
	{
		SpawnInMinigame spawnInMinigame = Object.Instantiate<SpawnInMinigame>(SpawnInGame);
		((Component)spawnInMinigame).transform.SetParent(((Component)Camera.main).transform, false);
		((Component)spawnInMinigame).transform.localPosition = new Vector3(0f, 0f, -600f);
		spawnInMinigame.Begin(null);
		yield return spawnInMinigame.WaitForFinish();
	}
}
