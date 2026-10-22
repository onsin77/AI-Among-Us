using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatLanguageButton : PoolableBehavior
{
	public TextMeshPro Text;

	public PassiveButton Button;

	[SerializeField]
	private SpriteRenderer Background;

	[SerializeField]
	private SpriteRenderer Check;

	public void SetSelected(bool selected)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Color val = (selected ? Color.white : Palette.DisabledGrey);
		((Component)this).GetComponent<ButtonRolloverHandler>().OutColor = val;
		Background.color = val;
		((Graphic)Text).color = val;
		if (Object.op_Implicit((Object)(object)Check))
		{
			((Renderer)Check).enabled = selected;
		}
	}

	public override void Reset()
	{
		base.Reset();
	}
}
