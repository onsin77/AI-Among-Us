using UnityEngine;
using UnityEngine.EventSystems;

namespace Rewired.Integration.UnityUI;

[AddComponentMenu("Rewired/Rewired Event System")]
public class RewiredEventSystem : EventSystem
{
	[SerializeField]
	[Tooltip("If enabled, the Event System will be updated every frame even if other Event Systems are enabled. Otherwise, only EventSystem.current will be updated.")]
	private bool _alwaysUpdate;

	public bool alwaysUpdate
	{
		get
		{
			return _alwaysUpdate;
		}
		set
		{
			_alwaysUpdate = value;
		}
	}

	protected override void Update()
	{
		if (alwaysUpdate)
		{
			EventSystem current = EventSystem.current;
			if ((Object)(object)current != (Object)(object)this)
			{
				EventSystem.current = (EventSystem)(object)this;
			}
			try
			{
				((EventSystem)this).Update();
				return;
			}
			finally
			{
				if ((Object)(object)current != (Object)(object)this)
				{
					EventSystem.current = current;
				}
			}
		}
		((EventSystem)this).Update();
	}
}
