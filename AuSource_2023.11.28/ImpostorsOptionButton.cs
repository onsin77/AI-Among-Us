using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImpostorsOptionButton : MonoBehaviour
{
	public SpriteRenderer SpriteRenderer { get; private set; }

	public TextMeshPro TextMesh { get; private set; }

	public BoxCollider2D BoxCollider { get; private set; }

	public PassiveButton PassiveButton { get; private set; }

	public ButtonRolloverHandler ButtonRollover { get; private set; }

	private void Awake()
	{
		SpriteRenderer = ((Component)this).gameObject.GetComponent<SpriteRenderer>();
		TextMesh = ((Component)this).gameObject.GetComponentInChildren<TextMeshPro>();
		BoxCollider = ((Component)this).gameObject.GetComponent<BoxCollider2D>();
		PassiveButton = ((Component)this).gameObject.GetComponent<PassiveButton>();
		ButtonRollover = ((Component)this).gameObject.GetComponent<ButtonRolloverHandler>();
	}

	public void SetOptionEnabled(bool b)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)TextMesh).color = (b ? Color.white : Palette.DisabledGrey);
		SpriteRenderer.color = (b ? Color.white : Palette.DisabledGrey);
		((Behaviour)BoxCollider).enabled = b;
		((Behaviour)PassiveButton).enabled = b;
		((Behaviour)ButtonRollover).enabled = b;
	}
}
