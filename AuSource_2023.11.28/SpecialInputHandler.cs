using UnityEngine;

public class SpecialInputHandler : MonoBehaviour
{
	public static int count;

	public static int disableVirtualCursorCount;

	public static int suppressInGamePlayerListCount;

	[SerializeField]
	private bool _disableVirtualCursor;

	[SerializeField]
	private bool _suppressInGamePlayerList;

	public bool disableVirtualCursor
	{
		get
		{
			return _disableVirtualCursor;
		}
		set
		{
			if (_disableVirtualCursor == value)
			{
				return;
			}
			_disableVirtualCursor = value;
			if (((Behaviour)this).isActiveAndEnabled)
			{
				if (_disableVirtualCursor)
				{
					disableVirtualCursorCount++;
				}
				else
				{
					disableVirtualCursorCount--;
				}
			}
		}
	}

	public bool suppressInGamePlayerList
	{
		get
		{
			return _suppressInGamePlayerList;
		}
		set
		{
			if (_suppressInGamePlayerList == value)
			{
				return;
			}
			_suppressInGamePlayerList = value;
			if (((Behaviour)this).isActiveAndEnabled)
			{
				if (_suppressInGamePlayerList)
				{
					suppressInGamePlayerListCount++;
				}
				else
				{
					suppressInGamePlayerListCount--;
				}
			}
		}
	}

	private void OnEnable()
	{
		count++;
		if (disableVirtualCursor)
		{
			disableVirtualCursorCount++;
		}
		if (suppressInGamePlayerList)
		{
			suppressInGamePlayerListCount++;
		}
	}

	private void OnDisable()
	{
		count--;
		if (disableVirtualCursor)
		{
			disableVirtualCursorCount--;
		}
		if (suppressInGamePlayerList)
		{
			suppressInGamePlayerListCount--;
		}
	}
}
