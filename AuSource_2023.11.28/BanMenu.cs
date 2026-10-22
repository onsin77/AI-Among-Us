using System.Collections.Generic;
using System.Linq;
using InnerNet;
using TMPro;
using UnityEngine;

public class BanMenu : MonoBehaviour
{
	public BanButton BanButtonPrefab;

	public SpriteRenderer Background;

	public PassiveButton MenuButton;

	public SpriteRenderer BanButton;

	public SpriteRenderer KickButton;

	public SpriteRenderer ReportButton;

	public GameObject ContentParent;

	public ReportReasonScreen ReportReason;

	public int selectedClientId = -1;

	public List<BanButton> allButtons = new List<BanButton>();

	private List<ClientData> recentClients = new List<ClientData>();

	[Header("Alignment Settings")]
	[SerializeField]
	private float backgroundTailWidth = 0.4f;

	[SerializeField]
	private float backgroundBaseWidth = 3.2f;

	[SerializeField]
	private float backgroundBaseHeight = 1.2f;

	[SerializeField]
	private float borderPadding = 0.15f;

	[SerializeField]
	private float heightPerButton = 0.4f;

	[SerializeField]
	private float widthPerCol = 1.4f;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public void SetVisible(bool show)
	{
		bool flag = Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) && !(PlayerControl.LocalPlayer.Data?.IsDead ?? true);
		show &= Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.Data != null;
		((Component)BanButton).gameObject.SetActive(AmongUsClient.Instance.CanBan() & flag);
		((Component)KickButton).gameObject.SetActive(AmongUsClient.Instance.CanKick() & flag);
		((Component)MenuButton).gameObject.SetActive(show);
	}

	private void Update()
	{
		if (!Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			return;
		}
		AmongUsClient.Instance.GetRecentClients(recentClients);
		foreach (ClientData client in recentClients)
		{
			if (client.Id == AmongUsClient.Instance.ClientId)
			{
				continue;
			}
			try
			{
				if (VoteBanSystem.Instance.HasMyVote(client.Id) && VoteBanSystem.Instance.Votes.TryGetValue(client.Id, out var value))
				{
					int num = value.Count((int c) => c != 0);
					BanButton banButton = allButtons.FirstOrDefault((BanButton b) => b.TargetClientId == client.Id);
					if (Object.op_Implicit((Object)(object)banButton) && banButton.numVotes != num)
					{
						banButton.SetVotes(num);
					}
				}
			}
			catch
			{
				break;
			}
		}
	}

	public void Show()
	{
		if (ContentParent.activeSelf)
		{
			Hide();
			return;
		}
		selectedClientId = -1;
		((Component)KickButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
		((Component)BanButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
		((Component)ReportButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
		ContentParent.SetActive(true);
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			List<ClientData> list = new List<ClientData>();
			AmongUsClient.Instance.GetRecentClients(list);
			foreach (ClientData item in list)
			{
				if (item.Id != AmongUsClient.Instance.ClientId)
				{
					string text = item.PlayerName ?? "???";
					if (AmongUsClient.Instance.GetClient(item.Id) == null)
					{
						text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.UserLeftGame, text);
					}
					BanButton banButton = Object.Instantiate<BanButton>(BanButtonPrefab, ((Component)Background).transform);
					banButton.Parent = this;
					((TMP_Text)banButton.NameText).text = text;
					banButton.TargetClientId = item.Id;
					banButton.Unselect();
					allButtons.Add(banButton);
					ControllerSelectable.AddUnique(((Component)banButton).GetComponent<UiElement>());
				}
			}
		}
		AlignAllButtons();
		ConsoleJoystick.SetMode_QuickChat();
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
	}

	[ContextMenu("AlignAllButtons")]
	private void AlignAllButtons()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.CeilToInt(6.6666665f);
		int num2 = Mathf.Max(1, Mathf.CeilToInt((float)allButtons.Count / (float)num));
		int num3 = Mathf.CeilToInt((float)allButtons.Count / (float)num2) + 1;
		float num4 = widthPerCol * (float)(num2 - 1);
		float num5 = backgroundBaseWidth + num4 + backgroundTailWidth;
		float num6 = (float)num3 * heightPerButton;
		float num7 = borderPadding * 2f + (float)num3 * heightPerButton;
		Background.size = new Vector2(num5, num7);
		((Component)Background).GetComponent<BoxCollider2D>().size = new Vector2(num5, num7);
		((Component)Background).transform.localPosition = new Vector3(0f - backgroundTailWidth - num4 / 2f, backgroundBaseHeight + num6 / 2f, 0.1f);
		for (int i = 0; i < allButtons.Count; i++)
		{
			int i2 = i % num2;
			int num8 = i / num2;
			float num9 = FloatRange.SpreadEvenly((0f - num5) / 2f - borderPadding * 4f, num5 / 2f + borderPadding * 4f, i2, num2) - backgroundTailWidth / 2f;
			float num10 = num7 / 2f - borderPadding - heightPerButton * (float)num8 - 0.1f;
			((Component)allButtons[i]).transform.localPosition = new Vector3(num9, num10, -1f);
		}
		float num11 = 0f - num7 / 2f + heightPerButton / 2f;
		((Component)KickButton).transform.localPosition = new Vector3(num5 / 2f - backgroundTailWidth - 2.6f, num11, -1f);
		((Component)BanButton).transform.localPosition = new Vector3(num5 / 2f - backgroundTailWidth - 1.6f, num11, -1f);
		((Component)ReportButton).transform.localPosition = new Vector3(num5 / 2f - backgroundTailWidth - 0.6f, num11, -1f);
	}

	public void Hide()
	{
		selectedClientId = -1;
		ContentParent.SetActive(false);
		for (int i = 0; i < allButtons.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)allButtons[i]).gameObject);
		}
		allButtons.Clear();
		ConsoleJoystick.SetMode_QuickChat();
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		ReportReason.Hide();
	}

	public bool CanSelect(int clientId)
	{
		if (VoteBanSystem.Instance.HasMyVote(clientId))
		{
			return false;
		}
		return true;
	}

	public void Select(int clientId)
	{
		if (!CanSelect(clientId))
		{
			return;
		}
		selectedClientId = clientId;
		ClientData recentClient = AmongUsClient.Instance.GetRecentClient(clientId);
		if (recentClient == null)
		{
			((Component)KickButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
			((Component)BanButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
			((Component)ReportButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
		}
		else
		{
			if (Object.op_Implicit((Object)(object)recentClient.Character))
			{
				((Component)KickButton).GetComponent<ButtonRolloverHandler>().SetEnabledColors();
				((Component)BanButton).GetComponent<ButtonRolloverHandler>().SetEnabledColors();
			}
			else
			{
				((Component)KickButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
				((Component)BanButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
			}
			if (!recentClient.HasBeenReported)
			{
				((Component)ReportButton).GetComponent<ButtonRolloverHandler>().SetEnabledColors();
			}
			else
			{
				((Component)ReportButton).GetComponent<ButtonRolloverHandler>().SetDisabledColors();
			}
		}
		for (int i = 0; i < allButtons.Count; i++)
		{
			BanButton banButton = allButtons[i];
			if (banButton.TargetClientId != clientId)
			{
				banButton.Unselect();
			}
		}
	}

	public void Kick(bool ban)
	{
		if (selectedClientId >= 0)
		{
			if (AmongUsClient.Instance.CanBan())
			{
				AmongUsClient.Instance.KickPlayer(selectedClientId, ban);
				Hide();
			}
			else
			{
				VoteBanSystem.Instance.CmdAddVote(selectedClientId);
			}
			Select(-1);
		}
	}

	public void PickReportReason()
	{
		ClientData recentClient = AmongUsClient.Instance.GetRecentClient(selectedClientId);
		if (recentClient != null && !recentClient.HasBeenReported)
		{
			ReportReason.Show(recentClient.PlayerName ?? "???", recentClient.ColorId);
		}
	}

	public void ReportPlayer(ReportReasons reason)
	{
		ClientData recentClient = AmongUsClient.Instance.GetRecentClient(selectedClientId);
		if (recentClient != null && !recentClient.HasBeenReported)
		{
			AmongUsClient.Instance.ReportPlayer(selectedClientId, reason);
			Hide();
		}
		Select(-1);
	}
}
