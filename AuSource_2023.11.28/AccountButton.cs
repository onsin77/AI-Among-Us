using TMPro;
using UnityEngine;

public class AccountButton : PoolableBehavior
{
	public TextMeshPro Text;

	public PassiveButton Button;

	public SpriteRenderer Background;

	public void SetSelected(bool selected)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ButtonRolloverHandler component = ((Component)this).GetComponent<ButtonRolloverHandler>();
		Color outColor = (Background.color = (selected ? Color.white : Color.black));
		component.OutColor = outColor;
	}

	public override void Reset()
	{
		base.Reset();
	}
}
