using TMPro;
using UnityEngine;

public class ServerListButton : PoolableBehavior
{
	public TextMeshPro Text;

	public PassiveButton Button;

	public SpriteRenderer Background;

	public TextTranslatorTMP textTranslator;

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

	public void SetTextTranslationId(StringNames id, string defaultStr)
	{
		textTranslator.TargetText = id;
		textTranslator.defaultStr = defaultStr;
	}
}
