using UnityEngine;

public class DestroyableSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public bool DontDestroy;

	public static bool InstanceExists => Object.op_Implicit((Object)(object)_instance);

	public static T Instance
	{
		get
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)_instance))
			{
				_instance = Object.FindObjectOfType<T>();
				if (!Object.op_Implicit((Object)(object)_instance))
				{
					_instance = new GameObject().AddComponent<T>();
				}
			}
			return _instance;
		}
	}

	public virtual void Awake()
	{
		if (!Object.op_Implicit((Object)(object)_instance))
		{
			_instance = (T)(object)((this is T) ? this : null);
			if (DontDestroy)
			{
				Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			}
		}
		else if ((Object)(object)_instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public virtual void OnDestroy()
	{
		if (!DontDestroy && (Object)(object)_instance == (Object)(object)this)
		{
			_instance = default;
		}
	}
}
