using InnerNet;
using TMPro;
using UnityEngine;

public class ReportReasonScreen : MonoBehaviour
{
	public BanMenu Parent;

	public ButtonRolloverHandler[] Buttons;

	public TextMeshPro NameText;

	public SpriteRenderer PlayerIcon;

	private ReportReasons? currentReason;

	public ReportNotification ConfirmScreen;

	private string playerName;

	private int colorId;

	public void Show(string playerName, int colorId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = ((Component)DestroyableSingleton<HudManager>.Instance.UICamera).transform.TransformPoint(Vector3.zero);
		position.x = val.x;
		position.y = val.y;
		((Component)this).transform.position = position;
		this.playerName = playerName;
		this.colorId = colorId;
		((TMP_Text)NameText).text = playerName;
		PlayerMaterial.SetColors(colorId, (Renderer)(object)PlayerIcon);
		currentReason = null;
		((Component)this).gameObject.SetActive(true);
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].ChangeOutColor(Color.white);
		}
	}

	public void SelectReason(ReportReason reason)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (reason.Reason == ReportReasons.None)
		{
			currentReason = null;
		}
		else
		{
			currentReason = reason.Reason;
		}
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].ChangeOutColor(((Object)(object)Buttons[i] == (Object)(object)reason.button) ? Color.green : Color.white);
		}
	}

	public void Submit()
	{
		if (currentReason.HasValue)
		{
			Parent.ReportPlayer(currentReason.Value);
			Hide();
			Object.Instantiate<ReportNotification>(ConfirmScreen, ((Component)DestroyableSingleton<HudManager>.Instance).gameObject.transform);
		}
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
