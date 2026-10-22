using System;
using UnityEngine;

public class DefaultPool : IObjectPool
{
	private static DefaultPool _instance;

	private static object _lock = new object();

	public override int InUse => 0;

	public override int NotInUse => 0;

	public static bool InstanceExists => Object.op_Implicit((Object)(object)_instance);

	public static DefaultPool Instance
	{
		get
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			lock (_lock)
			{
				if ((Object)(object)_instance == (Object)null)
				{
					_instance = Object.FindObjectOfType<DefaultPool>();
					if (Object.FindObjectsOfType<DefaultPool>().Length > 1)
					{
						Debug.LogError((object)"[Singleton] Something went really wrong  - there should never be more than 1 singleton! Reopening the scene might fix it.");
						return _instance;
					}
					if ((Object)(object)_instance == (Object)null)
					{
						GameObject val = new GameObject();
						_instance = val.AddComponent<DefaultPool>();
						((Object)val).name = "(singleton) DefaultPool";
					}
				}
				return _instance;
			}
		}
	}

	public void OnDestroy()
	{
		lock (_lock)
		{
			_instance = null;
		}
	}

	public override T Get<T>()
	{
		throw new NotImplementedException();
	}

	public override void Reclaim(PoolableBehavior obj)
	{
		Debug.Log((object)"Default Pool: Destroying this thing.");
		Object.Destroy((Object)(object)((Component)obj).gameObject);
	}
}
