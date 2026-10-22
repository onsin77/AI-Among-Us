using System.Collections;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

public class LogicPingsHnS : GameLogicComponent
{
	private LogicOptionsHnS options;

	private LogicGameFlowHnS flow;

	private ObjectPoolBehavior pingPool;

	private Coroutine seekerPingCoroutine;

	public LogicPingsHnS(GameManager manager, ObjectPoolBehavior pool)
		: base(manager)
	{
		options = manager.LogicOptions as LogicOptionsHnS;
		flow = manager.LogicFlow as LogicGameFlowHnS;
		pingPool = pool;
	}

	public override void OnGameStart()
	{
	}

	public override void OnGameEnd()
	{
		DestroyPingCoroutine();
	}

	private void DestroyPingCoroutine()
	{
		if (seekerPingCoroutine != null)
		{
			((MonoBehaviour)Manager).StopCoroutine(seekerPingCoroutine);
			seekerPingCoroutine = null;
		}
		SetPingsInactive();
	}

	private void SetPingsInactive()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		foreach (ArrowBehaviour activeChild in pingPool.activeChildren)
		{
			if (!((Object)(object)activeChild == (Object)null))
			{
				activeChild.target = Vector3.zero;
				((Component)activeChild).gameObject.SetActive(false);
			}
		}
	}

	public override void FixedUpdate()
	{
		if (Manager.GameHasStarted && flow.IsFinalCountdown && seekerPingCoroutine == null && options.GetSeekerPings())
		{
			seekerPingCoroutine = ((MonoBehaviour)Manager).StartCoroutine(SeekerPing());
		}
	}

	private IEnumerator SeekerPing()
	{
		while (Manager.GameHasStarted)
		{
			for (int i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
			{
				if (PlayerControl.AllPlayerControls[i].Data.Role.TeamType == RoleTeamTypes.Crewmate && !PlayerControl.AllPlayerControls[i].Data.IsDead && (PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Impostor || (Object)(object)PlayerControl.AllPlayerControls[i] == (Object)(object)PlayerControl.LocalPlayer))
				{
					PingBehaviour pingBehaviour = pingPool.Get<PingBehaviour>();
					pingBehaviour.target = Vector2.op_Implicit(PlayerControl.AllPlayerControls[i].GetTruePosition());
					pingBehaviour.AmSeeker = PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Impostor;
					pingBehaviour.UpdatePosition();
					((Component)pingBehaviour).gameObject.SetActive(true);
					pingBehaviour.SetImageEnabled(isOn: true);
				}
			}
			yield return (object)new WaitForSeconds(options.GetShowPingTime());
			foreach (ArrowBehaviour activeChild in pingPool.activeChildren)
			{
				activeChild.target = Vector3.zero;
				activeChild.SetImageEnabled(isOn: false);
				((Component)activeChild).gameObject.SetActive(false);
			}
			float maxPingTime = options.GetMaxPingTime();
			yield return (object)new WaitForSeconds(maxPingTime);
		}
	}

	public override void OnDestroy()
	{
		DestroyPingCoroutine();
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
