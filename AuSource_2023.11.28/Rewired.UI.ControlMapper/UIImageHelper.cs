using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[RequireComponent(typeof(Image))]
[AddComponentMenu("")]
public class UIImageHelper : MonoBehaviour
{
	[Serializable]
	private class State
	{
		[SerializeField]
		public Color color;

		public void Set(Image image)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)image == (Object)null))
			{
				((Graphic)image).color = color;
			}
		}
	}

	[SerializeField]
	private State enabledState;

	[SerializeField]
	private State disabledState;

	private bool currentState;

	public void SetEnabledState(bool newState)
	{
		currentState = newState;
		State state = (newState ? enabledState : disabledState);
		if (state != null)
		{
			Image component = ((Component)this).gameObject.GetComponent<Image>();
			if ((Object)(object)component == (Object)null)
			{
				Debug.LogError((object)"Image is missing!");
			}
			else
			{
				state.Set(component);
			}
		}
	}

	public void SetEnabledStateColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		enabledState.color = color;
	}

	public void SetDisabledStateColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		disabledState.color = color;
	}

	public void Refresh()
	{
		State state = (currentState ? enabledState : disabledState);
		Image component = ((Component)this).gameObject.GetComponent<Image>();
		if (!((Object)(object)component == (Object)null))
		{
			state.Set(component);
		}
	}
}
