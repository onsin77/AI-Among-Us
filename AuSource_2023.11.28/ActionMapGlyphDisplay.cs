using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ActionMapGlyphDisplay : MonoBehaviour
{
	public RewiredConstsEnum.Action actionToDisplayMappedGlyphFor;

	private SpriteRenderer sr;

	private void Awake()
	{
		sr = ((Component)this).GetComponent<SpriteRenderer>();
		UpdateGlyphDisplay();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(UpdateGlyphDisplay));
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(UpdateGlyphDisplay));
	}

	public void UpdateGlyphDisplay()
	{
		if (Object.op_Implicit((Object)(object)sr))
		{
			((Component)sr).gameObject.SetActive(false);
		}
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick && Object.op_Implicit((Object)(object)sr))
		{
			((Component)sr).gameObject.SetActive(true);
			Sprite sprite = GlyphCollection.FindGlyph((int)actionToDisplayMappedGlyphFor, out var error);
			if (error != GlyphCollection.ErrorCode.NoController)
			{
				sr.sprite = sprite;
			}
			else
			{
				((Component)sr).gameObject.SetActive(false);
			}
		}
	}

	public void SetColor(Color color)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)sr))
		{
			sr.color = color;
		}
	}

	public void SetSpriteVisibility(bool visible)
	{
		if ((Object)(object)sr != (Object)null)
		{
			((Renderer)sr).enabled = visible;
		}
	}
}
