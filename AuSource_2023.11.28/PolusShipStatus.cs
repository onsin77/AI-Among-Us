using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolusShipStatus : ShipStatus
{
	protected override void OnEnable()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (Systems == null)
		{
			Systems = new Dictionary<SystemTypes, ISystemType>(SystemTypeComparer.Instance)
			{
				{
					SystemTypes.Electrical,
					new SwitchSystem()
				},
				{
					SystemTypes.MedBay,
					new MedScanSystem()
				}
			};
			Camera main = Camera.main;
			main.backgroundColor = CameraColor;
			FollowerCamera component = ((Component)main).GetComponent<FollowerCamera>();
			((Renderer)DestroyableSingleton<HudManager>.Instance.ShadowQuad).material.SetInt("_Mask", 7);
			Systems.Add(SystemTypes.Doors, new DoorsSystemType());
			Systems.Add(SystemTypes.Comms, new HudOverrideSystemType());
			Systems.Add(SystemTypes.Security, new SecurityCameraSystemType());
			Systems.Add(SystemTypes.Ventilation, new VentilationSystem());
			Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory));
			if (Object.op_Implicit((Object)(object)component))
			{
				component.shakeAmount = 0f;
				component.shakePeriod = 0f;
			}
			Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(Systems.Values.Where((ISystemType i) => i is IActivatable).Cast<IActivatable>().ToArray()));
		}
	}

	public override void SpawnPlayer(PlayerControl player, int numPlayers, bool initialSpawn)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (initialSpawn)
		{
			base.SpawnPlayer(player, numPlayers, initialSpawn);
			return;
		}
		int num = Mathf.FloorToInt((float)numPlayers / 2f);
		int num2 = player.PlayerId % 15;
		Vector2 position = ((num2 >= num) ? (MeetingSpawnCenter2 + Vector2.right * (float)(num2 - num) * 0.6f) : (MeetingSpawnCenter + Vector2.right * (float)num2 * 0.6f));
		player.NetTransform.SnapTo(position);
	}
}
