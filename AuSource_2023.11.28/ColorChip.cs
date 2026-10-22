using UnityEngine;

public class ColorChip : MonoBehaviour
{
	public HatParent Inner;

	public GameObject PlayerEquippedForeground;

	public GameObject InUseForeground;

	public PassiveButton Button;

	public SpriteRenderer SelectionHighlight;

	public object Tag;

	public string ProductId { get; set; }

	public void ShowSelected()
	{
		((Renderer)SelectionHighlight).enabled = true;
	}

	public void Deselect()
	{
		((Renderer)SelectionHighlight).enabled = false;
	}
}
