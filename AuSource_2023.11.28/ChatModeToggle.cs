using AmongUs.Data;
using InnerNet;
using TMPro;
using UnityEngine;

public class ChatModeToggle : MonoBehaviour, IChatModeDisplay
{
	public SpriteRenderer quickChatBackgroundSprite;

	public SpriteRenderer freeChatBackgroundSprite;

	public TextMeshPro infoText;

	public TextMeshPro warningText;

	private float warningTimer;

	public void Awake()
	{
		DestroyableSingleton<AccountManager>.Instance.SetChatModeButtonForUpdates(this);
	}

	public void OnEnable()
	{
		UpdateDisplay();
	}

	private void Update()
	{
		if (warningTimer > 0f)
		{
			warningTimer -= Time.deltaTime;
			((TMP_Text)warningText).alpha = warningTimer;
			if (warningTimer <= 0f)
			{
				((Component)warningText).gameObject.SetActive(false);
			}
		}
	}

	public void ClickQuickChat()
	{
		DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.QuickChatOnly;
		DataManager.Settings.Save();
		((Component)warningText).gameObject.SetActive(false);
		UpdateDisplay();
	}

	public void ClickFreeChat()
	{
		if (!DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
		{
			((Component)warningText).gameObject.SetActive(true);
			((TMP_Text)warningText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FreeChatWarning);
			warningTimer = 10f;
		}
		else
		{
			((Component)warningText).gameObject.SetActive(false);
			DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
			DataManager.Settings.Save();
			UpdateDisplay();
		}
	}

	public void UpdateDisplay()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		switch (DataManager.Settings.Multiplayer.ChatMode)
		{
		case QuickChatModes.QuickChatOnly:
			if ((Object)(object)quickChatBackgroundSprite != (Object)null)
			{
				((Renderer)quickChatBackgroundSprite).enabled = true;
			}
			if ((Object)(object)freeChatBackgroundSprite != (Object)null)
			{
				((Renderer)freeChatBackgroundSprite).enabled = false;
			}
			((TMP_Text)infoText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QCOnlyInfo);
			break;
		case QuickChatModes.FreeChatOrQuickChat:
			if ((Object)(object)quickChatBackgroundSprite != (Object)null)
			{
				((Renderer)quickChatBackgroundSprite).enabled = false;
			}
			if ((Object)(object)freeChatBackgroundSprite != (Object)null)
			{
				((Renderer)freeChatBackgroundSprite).enabled = true;
				freeChatBackgroundSprite.color = Color.green;
			}
			((TMP_Text)infoText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FreeChatInfo);
			((Component)warningText).gameObject.SetActive(false);
			break;
		}
		if (!DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed() && (Object)(object)freeChatBackgroundSprite != (Object)null)
		{
			((Renderer)freeChatBackgroundSprite).enabled = true;
			freeChatBackgroundSprite.color = Color.grey;
		}
	}
}
