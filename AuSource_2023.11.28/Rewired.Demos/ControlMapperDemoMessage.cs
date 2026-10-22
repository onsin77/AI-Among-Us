using System.Collections;
using Rewired.UI.ControlMapper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class ControlMapperDemoMessage : MonoBehaviour
{
	public ControlMapper controlMapper;

	public Selectable defaultSelectable;

	private void Awake()
	{
		if ((Object)(object)controlMapper != (Object)null)
		{
			controlMapper.ScreenClosedEvent += OnControlMapperClosed;
			controlMapper.ScreenOpenedEvent += OnControlMapperOpened;
		}
	}

	private void Start()
	{
		SelectDefault();
	}

	private void OnControlMapperClosed()
	{
		((Component)this).gameObject.SetActive(true);
		((MonoBehaviour)this).StartCoroutine(SelectDefaultDeferred());
	}

	private void OnControlMapperOpened()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void SelectDefault()
	{
		if (!((Object)(object)EventSystem.current == (Object)null) && (Object)(object)defaultSelectable != (Object)null)
		{
			EventSystem.current.SetSelectedGameObject(((Component)defaultSelectable).gameObject);
		}
	}

	private IEnumerator SelectDefaultDeferred()
	{
		yield return null;
		SelectDefault();
	}
}
