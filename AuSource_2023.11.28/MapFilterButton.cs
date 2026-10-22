using UnityEngine;

public class MapFilterButton : MonoBehaviour
{
	public MapNames MapId;

	public PassiveButton Button;

	[SerializeField]
	private SpriteRenderer Icon;

	[SerializeField]
	private SpriteRenderer ButtonCheck;

	[SerializeField]
	private SpriteRenderer ButtonOutline;

	[SerializeField]
	private SpriteRenderer ButtonImage;

	public void Show()
	{
		((Component)Icon).gameObject.SetActive(true);
		((Component)Button).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)Icon).gameObject.SetActive(false);
		((Component)Button).gameObject.SetActive(false);
	}

	public void SetFlipped(bool flipped)
	{
		Icon.flipX = flipped;
		ButtonImage.flipX = flipped;
	}

	public void SetSelected(bool selected)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Color val = (selected ? Color.white : Palette.DisabledGrey);
		Icon.color = val;
		ButtonImage.color = val;
		ButtonOutline.color = val;
		((Component)ButtonOutline).GetComponent<ButtonRolloverHandler>().OutColor = val;
		((Renderer)ButtonCheck).enabled = selected;
	}
}
