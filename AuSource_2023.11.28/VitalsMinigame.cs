using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VitalsMinigame : Minigame
{
	public TextMeshPro BatteryText;

	public VitalsPanel PanelPrefab;

	public TextMeshPro SabText;

	public float XStart = -0.8f;

	public float YStart = 2.15f;

	public float XOffset = 1.95f;

	public float YOffset = -0.65f;

	private VitalsPanel[] vitals;

	public override void Begin(PlayerTask task)
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		DeadBody[] source = Object.FindObjectsOfType<DeadBody>();
		List<GameData.PlayerInfo> list = new List<GameData.PlayerInfo>();
		list.AddRange(GameData.Instance.AllPlayers);
		Dictionary<byte, bool> vitalDisconnected = new Dictionary<byte, bool>();
		foreach (GameData.PlayerInfo player in list)
		{
			bool flag = !source.Any((DeadBody b) => b.ParentId == player.PlayerId);
			vitalDisconnected[player.PlayerId] = flag && (player.Disconnected || player.IsDead);
		}
		list.Sort((GameData.PlayerInfo a, GameData.PlayerInfo b) =>
		{
			if (vitalDisconnected[a.PlayerId] && !vitalDisconnected[b.PlayerId])
			{
				return 1;
			}
			return (!vitalDisconnected[a.PlayerId] && vitalDisconnected[b.PlayerId]) ? (-1) : 0;
		});
		vitals = new VitalsPanel[list.Count];
		for (int num = 0; num < list.Count; num++)
		{
			GameData.PlayerInfo playerInfo = list[num];
			int num2 = num % 3;
			int num3 = num / 3;
			VitalsPanel vitalsPanel = Object.Instantiate<VitalsPanel>(PanelPrefab, ((Component)this).transform);
			((Component)vitalsPanel).transform.localPosition = new Vector3(XStart + (float)num2 * XOffset, YStart + (float)num3 * YOffset, -1f);
			vitalsPanel.SetPlayer(num, playerInfo);
			if (vitalDisconnected[playerInfo.PlayerId])
			{
				vitalsPanel.SetDisconnected();
			}
			else if (playerInfo.IsDead)
			{
				vitalsPanel.SetDead();
			}
			else
			{
				vitalsPanel.SetAlive();
			}
			vitals[num] = vitalsPanel;
		}
		SetupInput(disableCursor: true);
	}

	private void Update()
	{
		if (((Behaviour)SabText).isActiveAndEnabled && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			((Component)SabText).gameObject.SetActive(false);
			for (int i = 0; i < vitals.Length; i++)
			{
				((Component)vitals[i]).gameObject.SetActive(true);
			}
		}
		else if (!((Behaviour)SabText).isActiveAndEnabled && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			((Component)SabText).gameObject.SetActive(true);
			for (int j = 0; j < vitals.Length; j++)
			{
				((Component)vitals[j]).gameObject.SetActive(false);
			}
		}
		for (int k = 0; k < vitals.Length; k++)
		{
			VitalsPanel vitalsPanel = vitals[k];
			if (!vitalsPanel.PlayerInfo.IsDead && vitalsPanel.PlayerInfo.Disconnected && !vitalsPanel.IsDiscon)
			{
				vitalsPanel.SetDisconnected();
			}
			else if (vitalsPanel.PlayerInfo.IsDead && !vitalsPanel.IsDead && !vitalsPanel.IsDiscon)
			{
				vitalsPanel.SetDead();
			}
		}
	}
}
