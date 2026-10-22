using AmongUs.Data;
using InnerNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatModeCycle : MonoBehaviour, IChatModeDisplay
{
	public CycleButtonBehaviour chatMode;

	public TextMeshPro chatModeText;

	public SpriteRenderer backgroundSprite;

	public void Awake()
	{
		DestroyableSingleton<AccountManager>.Instance.SetChatModeButtonForUpdates(this);
	}

	public void OnEnable()
	{
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		chatMode.options = new StringNames[2]
		{
			StringNames.FreeOrQuickChat,
			StringNames.QuickChatOnly
		};
		if (!DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
		{
			DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.QuickChatOnly;
			chatMode.Rollover.OverColor = Color.grey;
			chatMode.Rollover.OutColor = Color.grey;
			((Graphic)chatMode.Text).color = Color.grey;
			((Graphic)chatModeText).color = Color.grey;
			if ((Object)(object)backgroundSprite != (Object)null)
			{
				backgroundSprite.color = Color.grey;
			}
		}
		else
		{
			chatMode.Rollover.OverColor = Color.green;
			chatMode.Rollover.OutColor = Color.white;
			((Graphic)chatMode.Text).color = Color.white;
			((Graphic)chatModeText).color = Color.white;
			if ((Object)(object)backgroundSprite != (Object)null)
			{
				backgroundSprite.color = Color.white;
			}
		}
		chatMode.UpdateText((int)(DataManager.Settings.Multiplayer.ChatMode - 1));
	}

	public void CycleChatMode()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
		{
			int num = (int)(DataManager.Settings.Multiplayer.ChatMode + 1);
			num %= 3;
			if (num == 0)
			{
				num++;
			}
			DataManager.Settings.Multiplayer.ChatMode = (QuickChatModes)num;
			DataManager.Settings.Save();
			chatMode.UpdateText((int)(DataManager.Settings.Multiplayer.ChatMode - 1));
		}
	}
}
