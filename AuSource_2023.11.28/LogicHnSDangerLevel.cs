using System.Collections.Generic;
using Hazel;
using UnityEngine;

public class LogicHnSDangerLevel : GameLogicComponent
{
	private DangerMeter dangerMeter;

	private List<PlayerControl> impostors;

	private float scaryMusicDistance;

	private float veryScaryMusicDistance;

	private float dangerLevel1;

	private float dangerLevel2;

	private HideAndSeekManager hnsManager;

	private bool firstMusicActivation;

	private float firstCrossfadeCountdown;

	public LogicHnSDangerLevel(HideAndSeekManager manager)
		: base(manager)
	{
		hnsManager = manager;
	}

	public override void OnGameStart()
	{
		firstMusicActivation = true;
		if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			dangerMeter = DestroyableSingleton<HudManager>.Instance.DangerMeter;
			((Component)dangerMeter).gameObject.SetActive(true);
		}
		impostors = new List<PlayerControl>();
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			if ((Object)(object)allPlayerControl.Data?.Role != (Object)null && allPlayerControl.Data.Role.IsImpostor)
			{
				impostors.Add(allPlayerControl);
			}
		}
		scaryMusicDistance = hnsManager.LogicOptionsHnS.GetScaryMusicDistance() * hnsManager.LogicOptionsHnS.PlayerSpeedBase;
		veryScaryMusicDistance = hnsManager.LogicOptionsHnS.GetVeryScaryMusicDistance() * hnsManager.LogicOptionsHnS.PlayerSpeedBase;
		if (scaryMusicDistance < veryScaryMusicDistance)
		{
			float num = veryScaryMusicDistance;
			float num2 = scaryMusicDistance;
			scaryMusicDistance = num;
			veryScaryMusicDistance = num2;
		}
	}

	public override void OnPlayerDisconnect(PlayerControl pc)
	{
		if (pc.Data.Role.IsImpostor)
		{
			impostors?.Remove(pc);
		}
	}

	public override void OnGameEnd()
	{
		impostors = null;
	}

	public override void FixedUpdate()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (impostors == null || (Object)(object)localPlayer == (Object)null || impostors.Count <= 0)
		{
			return;
		}
		float num = float.MaxValue;
		foreach (PlayerControl impostor in impostors)
		{
			if (!((Object)(object)impostor == (Object)null))
			{
				Vector3 val = ((Component)impostor).transform.position - ((Component)localPlayer).transform.position;
				float sqrMagnitude = val.sqrMagnitude;
				if (sqrMagnitude < scaryMusicDistance && num > sqrMagnitude)
				{
					num = sqrMagnitude;
				}
			}
		}
		if (ShipStatus.Instance.HideCountdown > 0f)
		{
			dangerLevel1 = 0f;
			dangerLevel2 = 0f;
		}
		else
		{
			if (firstMusicActivation)
			{
				firstMusicActivation = false;
				firstCrossfadeCountdown = 3f;
				hnsManager.LogicMusic.SetMusicCrossfadeSpeed(0.6f);
			}
			if (firstCrossfadeCountdown > 0f)
			{
				firstCrossfadeCountdown -= Time.deltaTime;
				if (firstCrossfadeCountdown <= 0f)
				{
					hnsManager.LogicMusic.SetMusicCrossfadeSpeed(5f);
				}
			}
			dangerLevel1 = Mathf.Clamp01((scaryMusicDistance - num) / (scaryMusicDistance - veryScaryMusicDistance));
			dangerLevel2 = Mathf.Clamp01((veryScaryMusicDistance - num) / veryScaryMusicDistance);
		}
		UpdateDangerMeter();
		UpdateDangerMusic();
	}

	private void UpdateDangerMusic()
	{
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			hnsManager.LogicMusic.SetTaskState(isDoingTask: false);
			hnsManager.LogicMusic.ResetMusic();
		}
		else
		{
			hnsManager.LogicMusic.SetMusicValues(dangerLevel1, dangerLevel2);
		}
	}

	private void UpdateDangerMeter()
	{
		if (!((Object)(object)dangerMeter == (Object)null))
		{
			dangerMeter.SetDangerValue(dangerLevel1, dangerLevel2);
		}
	}

	public override void OnDestroy()
	{
		impostors = null;
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
