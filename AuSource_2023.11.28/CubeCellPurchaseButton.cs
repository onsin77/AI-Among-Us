using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CubeCellPurchaseButton : MonoBehaviour
{
	public PassiveButton button;

	public TextMeshPro text;

	public SpriteRenderer podIcon;

	public SpriteRenderer buttonBackground;

	public void SetUseable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)button).enabled = true;
		((Graphic)text).color = Color.white;
		buttonBackground.color = Color.white;
	}

	public void SetInactive()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)button).enabled = false;
		((Graphic)text).color = Color.red;
		buttonBackground.color = Palette.DisabledGrey;
	}
}
