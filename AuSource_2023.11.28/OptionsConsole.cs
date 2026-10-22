using UnityEngine;

public class OptionsConsole : MonoBehaviour, IUsable
{
	public Vector3 CustomPosition;

	public bool HostOnly;

	public GameObject MenuPrefab;

	public SpriteRenderer Outline;

	public ImageNames CustomUseIcon;

	public ImageNames UseIcon => CustomUseIcon;

	public float UsableDistance => 1f;

	public float PercentCool => 0f;

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		couldUse = playerControl.CanMove;
		if (HostOnly)
		{
			couldUse &= AmongUsClient.Instance.AmHost;
		}
		canUse = couldUse;
		if (canUse)
		{
			num = Vector2.Distance(playerControl.GetTruePosition(), Vector2.op_Implicit(((Component)this).transform.position));
			canUse &= num <= UsableDistance;
		}
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Outline))
		{
			((Renderer)Outline).material.SetFloat("_Outline", (float)(on ? 1 : 0));
			((Renderer)Outline).material.SetColor("_OutlineColor", Color.white);
			((Renderer)Outline).material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
		}
	}

	public void Use()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			PlayerControl.LocalPlayer.NetTransform.Halt();
			GameObject val = Object.Instantiate<GameObject>(MenuPrefab);
			val.transform.SetParent(((Component)Camera.main).transform, false);
			val.transform.localPosition = CustomPosition;
			DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(null, val.gameObject, null);
		}
	}
}
