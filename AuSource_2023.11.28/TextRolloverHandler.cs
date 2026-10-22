using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextRolloverHandler : MonoBehaviour
{
	public TextMeshPro Target;

	public Color OverColor = Color.green;

	public Color OverOutlineColor = Color.white;

	public Color OutColor = Color.white;

	public Color OutOutlineColor = Color.white;

	public void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		PassiveButton component = ((Component)this).GetComponent<PassiveButton>();
		component.OnMouseOver.AddListener((UnityAction)DoMouseOver);
		component.OnMouseOut.AddListener((UnityAction)DoMouseOut);
	}

	public void DoMouseOver()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)Target).color = OverColor;
		((TMP_Text)Target).outlineColor = Color32.op_Implicit(OverOutlineColor);
	}

	public void DoMouseOut()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)Target).color = OutColor;
		((TMP_Text)Target).outlineColor = Color32.op_Implicit(OutOutlineColor);
	}

	public TextRolloverHandler()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
	}
}
