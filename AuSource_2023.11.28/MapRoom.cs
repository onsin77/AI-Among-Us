using UnityEngine;

public class MapRoom : MonoBehaviour
{
	public SystemTypes room;

	public SpriteRenderer door;

	public SpriteRenderer special;

	public InfectedOverlay Parent { get; set; }

	public void Start()
	{
		if (Object.op_Implicit((Object)(object)door))
		{
			door.SetCooldownNormalizedUvs();
		}
		if (Object.op_Implicit((Object)(object)special))
		{
			special.SetCooldownNormalizedUvs();
		}
	}

	public void DoorsUpdate()
	{
		if (Object.op_Implicit((Object)(object)door) && Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			float timer = ((RunTimer)ShipStatus.Instance.Systems[SystemTypes.Doors]).GetTimer(room);
			float num = (Parent.CanUseDoors ? timer : 1f);
			((Renderer)door).material.SetFloat("_Percent", num);
		}
	}

	internal void SetSpecialActive(float perc)
	{
		if (Object.op_Implicit((Object)(object)special))
		{
			((Renderer)special).material.SetFloat("_Percent", perc);
		}
	}

	public void SabotageReactor()
	{
		if (Parent.CanUseSabotage)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)3);
			DestroyableSingleton<AchievementManager>.Instance.SabotageCalledLocally();
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageStart(SystemTypes.Reactor);
		}
	}

	public void SabotageHeli()
	{
		if (Parent.CanUseSabotage)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)58);
			DestroyableSingleton<AchievementManager>.Instance.SabotageCalledLocally();
		}
	}

	public void SabotageComms()
	{
		if (Parent.CanUseSabotage)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)14);
			DestroyableSingleton<AchievementManager>.Instance.SabotageCalledLocally();
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageStart(SystemTypes.Comms);
		}
	}

	public void SabotageOxygen()
	{
		if (Parent.CanUseSabotage)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)8);
			DestroyableSingleton<AchievementManager>.Instance.SabotageCalledLocally();
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageStart(SystemTypes.LifeSupp);
		}
	}

	public void SabotageLights()
	{
		if (Parent.CanUseSabotage)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)7);
			DestroyableSingleton<AchievementManager>.Instance.SabotageCalledLocally();
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageStart(SystemTypes.Electrical);
		}
	}

	public void SabotageSeismic()
	{
		if (Parent.CanUseSabotage)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)21);
			DestroyableSingleton<AchievementManager>.Instance.SabotageCalledLocally();
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageStart(SystemTypes.Laboratory);
		}
	}

	public void SabotageMushroomMixup()
	{
		if (Parent.CanUseSabotage)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)57);
			DestroyableSingleton<AchievementManager>.Instance.SabotageCalledLocally();
		}
	}

	public void SabotageDoors()
	{
		if (Parent.CanUseDoors && !(((RunTimer)ShipStatus.Instance.Systems[SystemTypes.Doors]).GetTimer(room) > 0f))
		{
			ShipStatus.Instance.RpcCloseDoorsOfType(room);
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageStart(SystemTypes.Doors);
		}
	}
}
