using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeshifterMinigame : Minigame
{
	public ShapeshifterPanel PanelPrefab;

	public float XStart = -0.8f;

	public float YStart = 2.15f;

	public float XOffset = 1.95f;

	public float YOffset = -0.65f;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	private List<ShapeshifterPanel> potentialVictims;

	public override void Begin(PlayerTask task)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		List<byte> bodies = new List<byte>();
		Object.FindObjectsOfType<DeadBody>().ForEach((DeadBody body) =>
		{
			bodies.Add(body.ParentId);
		});
		List<PlayerControl> list = PlayerControl.AllPlayerControls.FindAll((PlayerControl p) => (Object)(object)p != (Object)(object)PlayerControl.LocalPlayer && (!p.Data.IsDead || (p.Data.IsDead && bodies.Contains(p.PlayerId))));
		potentialVictims = new List<ShapeshifterPanel>();
		List<UiElement> list2 = new List<UiElement>();
		for (int num = 0; num < list.Count; num++)
		{
			PlayerControl player = list[num];
			int num2 = num % 3;
			int num3 = num / 3;
			bool flag = PlayerControl.LocalPlayer.Data.Role.NameColor == player.Data.Role.NameColor;
			ShapeshifterPanel shapeshifterPanel = Object.Instantiate<ShapeshifterPanel>(PanelPrefab, ((Component)this).transform);
			((Component)shapeshifterPanel).transform.localPosition = new Vector3(XStart + (float)num2 * XOffset, YStart + (float)num3 * YOffset, -1f);
			shapeshifterPanel.SetPlayer(num, player.Data, () =>
			{
				Shapeshift(player);
			});
			((Graphic)shapeshifterPanel.NameText).color = (flag ? player.Data.Role.NameColor : Color.white);
			potentialVictims.Add(shapeshifterPanel);
			list2.Add(shapeshifterPanel.Button);
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, list2);
	}

	private void Shapeshift(PlayerControl target)
	{
		if (PlayerControl.LocalPlayer.inVent)
		{
			Close();
			return;
		}
		if ((Object)(object)target != (Object)null)
		{
			PlayerControl.LocalPlayer.CmdCheckShapeshift(target, shouldAnimate: true);
		}
		else
		{
			Logger.GlobalInstance.Warning($"Player attempted to shapeshift into a null player {target?.PlayerId}. Maybe due to disconnect?");
		}
		Close();
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}
}
