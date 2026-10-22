using System;
using System.Linq;
using UnityEngine;

public class MapBehaviour : MonoBehaviour
{
	public static MapBehaviour Instance;

	public AlphaPulse ColorControl;

	public SpriteRenderer HerePoint;

	public MapCountOverlay countOverlay;

	public InfectedOverlay infectedOverlay;

	public MapTaskOverlay taskOverlay;

	[SerializeField]
	private GameObject fadedBackground;

	private SpecialInputHandler specialInputHandler;

	private bool countOverlayAllowsMovement;

	private Vector3? preMeetingPosition;

	public bool IsOpen => ((Behaviour)this).isActiveAndEnabled;

	public bool IsOpenStopped
	{
		get
		{
			if (IsOpen && ((Behaviour)countOverlay).isActiveAndEnabled)
			{
				return !countOverlayAllowsMovement;
			}
			return false;
		}
	}

	public void SetPreMeetingPosition(Vector3 preMeetingPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.preMeetingPosition = preMeetingPosition;
	}

	public void Show(MapOptions opts)
	{
		if (IsOpen)
		{
			Close();
		}
		else if (PlayerControl.LocalPlayer.CanMove || Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			Logger.GlobalInstance.Debug($"GAMEPLAY::MapBehaviour::Showing map in mode {opts.Mode}");
			switch (opts.Mode)
			{
			case MapOptions.Modes.Normal:
				ShowNormalMap();
				break;
			case MapOptions.Modes.CountOverlay:
				ShowCountOverlay(opts.AllowMovementWhileMapOpen, opts.ShowLivePlayerPosition, opts.IncludeDeadBodies);
				break;
			case MapOptions.Modes.Sabotage:
				ShowSabotageMap();
				break;
			default:
				throw new ArgumentOutOfRangeException($"Unexpected map mode {opts.Mode}");
			}
		}
	}

	public void Close()
	{
		Logger.GlobalInstance.Debug("GAMEPLAY::MapBehaviour::Closing map");
		((Component)this).gameObject.SetActive(false);
		((Component)countOverlay).gameObject.SetActive(false);
		((Component)infectedOverlay).gameObject.SetActive(false);
		taskOverlay.Hide();
		((Renderer)HerePoint).enabled = true;
		if (!Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			DestroyableSingleton<HudManager>.Instance.SetHudActive(isActive: true);
		}
	}

	private void Validate()
	{
		countOverlay.CountAreas.ForEach((CounterArea area) =>
		{
			if ((Object)(object)area == (Object)null)
			{
				Debug.LogError((object)"One of the count areas in the count overlay is null");
			}
			else if (ShipStatus.Instance.AllRooms.All((PlainShipRoom r) => r.RoomId != area.RoomType))
			{
				Debug.LogError((object)$"There was a count area for a room that isn't in the current ship: {area.RoomType}");
			}
		});
		infectedOverlay.rooms.ForEach((MapRoom area) =>
		{
			if ((Object)(object)area == (Object)null)
			{
				Debug.LogError((object)"One of the sabotage areas in the sabotage overlay is null");
			}
		});
	}

	private void ShowSabotageMap()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			Close();
		}
		else if (PlayerControl.LocalPlayer.CanMove)
		{
			if ((Object)(object)specialInputHandler != (Object)null)
			{
				specialInputHandler.disableVirtualCursor = true;
			}
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)HerePoint);
			GenericShow();
			((Component)infectedOverlay).gameObject.SetActive(true);
			ColorControl.SetColor(Palette.ImpostorRed);
			taskOverlay.Show();
			DestroyableSingleton<HudManager>.Instance.SetHudActive(isActive: false);
			ConsoleJoystick.SetMode_Sabotage();
		}
	}

	private void ShowNormalMap()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			Close();
		}
		else if (PlayerControl.LocalPlayer.CanMove || Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)HerePoint);
			GenericShow();
			taskOverlay.Show();
			ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
			DestroyableSingleton<HudManager>.Instance.SetHudActive(isActive: false);
		}
	}

	private void ShowCountOverlay(bool allowedToMove, bool showLivePlayerPosition, bool includeDeadBodies)
	{
		GenericShow();
		((Component)countOverlay).gameObject.SetActive(true);
		countOverlay.SetOptions(showLivePlayerPosition, includeDeadBodies);
		countOverlayAllowsMovement = allowedToMove;
		taskOverlay.Hide();
		((Renderer)HerePoint).enabled = !showLivePlayerPosition;
		if (((Renderer)HerePoint).enabled)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)HerePoint);
		}
		DestroyableSingleton<HudManager>.Instance.SetHudActive(isActive: false);
	}

	private void FixedUpdate()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			Vector3 val = ((Component)PlayerControl.LocalPlayer).transform.position;
			if (Object.op_Implicit((Object)(object)MeetingHud.Instance) && preMeetingPosition.HasValue)
			{
				val = preMeetingPosition.Value;
			}
			else if (preMeetingPosition.HasValue)
			{
				preMeetingPosition = null;
			}
			val /= ShipStatus.Instance.MapScale;
			val.x *= Mathf.Sign(((Component)ShipStatus.Instance).transform.localScale.x);
			val.z = -1f;
			((Component)HerePoint).transform.localPosition = val;
		}
	}

	private void Awake()
	{
		Instance = this;
		specialInputHandler = ((Component)this).GetComponent<SpecialInputHandler>();
	}

	private void GenericShow()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = Vector3.one;
		((Component)this).transform.localPosition = new Vector3(0f, 0f, -25f);
		Vector3 localScale = ((Component)taskOverlay).transform.localScale;
		if (Mathf.Sign(localScale.x) != Mathf.Sign(((Component)ShipStatus.Instance).transform.localScale.x))
		{
			localScale.x *= -1f;
		}
		((Component)taskOverlay).transform.localScale = localScale;
		fadedBackground.SetActive(Object.op_Implicit((Object)(object)MeetingHud.Instance));
		((Renderer)HerePoint).material.SetInt(PlayerMaterial.MaskLayer, 255);
		((Component)this).gameObject.SetActive(true);
	}
}
