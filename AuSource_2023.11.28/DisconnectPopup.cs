using System;
using System.Collections.Generic;
using AmongUs.Data;
using InnerNet;
using TMPro;
using UnityEngine;

public class DisconnectPopup : DestroyableSingleton<DisconnectPopup>
{
	[SerializeField]
	private TextMeshPro _textArea;

	private static readonly Dictionary<DisconnectReasons, StringNames> ErrorMessages = new Dictionary<DisconnectReasons, StringNames>
	{
		{
			DisconnectReasons.NotAuthorized,
			StringNames.ErrorNotAuthenticated
		},
		{
			DisconnectReasons.ServerFull,
			StringNames.ErrorServerOverload
		},
		{
			DisconnectReasons.Hacking,
			StringNames.ErrorKickedNoCode
		},
		{
			DisconnectReasons.IncorrectVersion,
			StringNames.ErrorIncorrectVersion
		},
		{
			DisconnectReasons.GameNotFound,
			StringNames.ErrorNotFoundGame
		},
		{
			DisconnectReasons.GameFull,
			StringNames.ErrorFullGame
		},
		{
			DisconnectReasons.GameStarted,
			StringNames.ErrorStartedGame
		},
		{
			DisconnectReasons.FocusLost,
			StringNames.ErrorFocusLost
		},
		{
			DisconnectReasons.PlatformLock,
			StringNames.ErrorPlatformLock
		},
		{
			DisconnectReasons.LobbyInactivity,
			StringNames.ErrorLobbyInactivity
		},
		{
			DisconnectReasons.MatchmakerInactivity,
			StringNames.ErrorMatchmakerInactivity
		},
		{
			DisconnectReasons.InvalidGameOptions,
			StringNames.ErrorInvalidGameOptions
		},
		{
			DisconnectReasons.NoServersAvailable,
			StringNames.ErrorNoServersAvailable
		},
		{
			DisconnectReasons.QuickmatchDisabled,
			StringNames.ErrorQuickmatchDisabled
		},
		{
			DisconnectReasons.TooManyGames,
			StringNames.ErrorTooManyGames
		},
		{
			DisconnectReasons.PlatformParentalControlsBlock,
			StringNames.ErrorPlatformParentalControlsBlock
		},
		{
			DisconnectReasons.PlatformUserBlock,
			StringNames.ErrorLobbyUsersBlocked
		},
		{
			DisconnectReasons.TooManyRequests,
			StringNames.ErrorTooManyRequests
		}
	};

	private static readonly Dictionary<SanctionReasons, StringNames> SanctionMessages = new Dictionary<SanctionReasons, StringNames>
	{
		{
			SanctionReasons.SexualMisconduct,
			StringNames.SanctionSexualMisconduct
		},
		{
			SanctionReasons.Doxing,
			StringNames.SanctionDoxing
		},
		{
			SanctionReasons.IllegalActivity,
			StringNames.SanctionIllegalActivity
		},
		{
			SanctionReasons.Harassment,
			StringNames.SanctionHarassment
		},
		{
			SanctionReasons.SelfHarmPromotion,
			StringNames.SanctionSelfHarmPromotion
		},
		{
			SanctionReasons.CheatingHacking,
			StringNames.SanctionCheatingHacking
		},
		{
			SanctionReasons.Spamming,
			StringNames.SanctionSpamming
		},
		{
			SanctionReasons.UnsportsmanlikeConduct,
			StringNames.SanctionUnsportsmanlikeConduct
		},
		{
			SanctionReasons.InappropriateChatDating,
			StringNames.SanctionInappropriateChatDating
		},
		{
			SanctionReasons.Bullying,
			StringNames.SanctionBullying
		},
		{
			SanctionReasons.InappropriateChatPersonalInfo,
			StringNames.SanctionInappropriateChatPersonalInfo
		},
		{
			SanctionReasons.ImpersonationDevelopers,
			StringNames.SanctionImpersonationDevelopers
		},
		{
			SanctionReasons.ImpersonationCeleb,
			StringNames.SanctionImpersonationCeleb
		},
		{
			SanctionReasons.WeaponizingRules,
			StringNames.SanctionWeaponizingRules
		},
		{
			SanctionReasons.InappropriateNameDerogatory,
			StringNames.SanctionInappropriateNameDerogatory
		},
		{
			SanctionReasons.InappropriateNameNsfw,
			StringNames.SanctionInappropriateNameNsfw
		},
		{
			SanctionReasons.InappropriateNameUnsportsmanlike,
			StringNames.SanctionInappropriateNameUnsportsmanlike
		},
		{
			SanctionReasons.RepeatOffender3,
			StringNames.SanctionRepeatOffender3
		},
		{
			SanctionReasons.RepeatOffender10,
			StringNames.SanctionRepeatOffender10
		}
	};

	private StringNames[] QuickChatNames = new StringNames[3]
	{
		StringNames.FreeChatOnly,
		StringNames.FreeOrQuickChat,
		StringNames.QuickChatOnly
	};

	public UiElement BackButton;

	private List<SelectableHyperLink> selectableHyperLinks = new List<SelectableHyperLink>();

	public void Start()
	{
		if ((Object)(object)DestroyableSingleton<DisconnectPopup>.Instance == (Object)(object)this)
		{
			Show();
		}
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
		DoShow();
	}

	private void DoShow()
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		if (!Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton);
		SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorUnknown));
		string text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
		switch (AmongUsClient.Instance.LastDisconnectReason)
		{
		case DisconnectReasons.IntentionalLeaving:
			SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorIntentionalLeaving, StatsManager.Instance.BanMinutesLeft));
			break;
		case DisconnectReasons.Banned:
			if (text != null)
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorBanned, text));
			}
			else
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorBannedNoCode));
			}
			break;
		case DisconnectReasons.Sanctions:
		{
			HttpMatchmakerManager.MatchmakerError lastMatchmakerError = AmongUsClient.Instance.LastMatchmakerError;
			if (lastMatchmakerError != null)
			{
				StringNames id = (SanctionMessages.ContainsKey(lastMatchmakerError.SanctionReason) ? SanctionMessages[lastMatchmakerError.SanctionReason] : StringNames.SanctionUnknown);
				bool flag = lastMatchmakerError.EndsAt == DateTimeOffset.MaxValue;
				string text2 = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorSanction, DestroyableSingleton<TranslationController>.Instance.GetString(id), (!flag) ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SanctionDuration, lastMatchmakerError.EndsAt.ToString("d")) : DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SanctionPermanent));
				if (!flag)
				{
					text2 += DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SanctionConduct);
				}
				SetText(text2);
			}
			else
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorSanction, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SanctionUnknown), DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SanctionDuration, "YYYY-MM-DD")));
			}
			break;
		}
		case DisconnectReasons.Kicked:
			if (text != null)
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorKicked, text));
			}
			else
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorKickedNoCode));
			}
			break;
		case DisconnectReasons.Error:
			if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorGenericOnlineDisconnect));
			}
			else
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorGenericLocalDisconnect));
			}
			break;
		case DisconnectReasons.InvalidName:
			SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorInvalidName, DataManager.Player.Customization.Name));
			break;
		case DisconnectReasons.ClientTimeout:
			SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorClientTimeout));
			break;
		default:
		{
			if (ErrorMessages.TryGetValue(AmongUsClient.Instance.LastDisconnectReason, out var value))
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(value));
				break;
			}
			SetText(string.IsNullOrWhiteSpace(AmongUsClient.Instance.LastCustomDisconnect) ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorUnknown, (int)AmongUsClient.Instance.LastDisconnectReason) : AmongUsClient.Instance.LastCustomDisconnect);
			break;
		}
		case DisconnectReasons.ExitGame:
		case DisconnectReasons.Destroy:
		case DisconnectReasons.Unknown:
			((Component)this).gameObject.SetActive(false);
			break;
		case DisconnectReasons.QuickchatLock:
			SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorQuickChatMode, DestroyableSingleton<TranslationController>.Instance.GetString(QuickChatNames[AmongUsClient.Instance.LastServerChatMode]), DestroyableSingleton<TranslationController>.Instance.GetString(QuickChatNames[(int)DataManager.Settings.Multiplayer.ChatMode])));
			break;
		case DisconnectReasons.ServerRequest:
		case DisconnectReasons.InternalPlayerMissing:
		case DisconnectReasons.InternalNonceFailure:
		case DisconnectReasons.InternalConnectionToken:
			SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorInternalServer, (int)AmongUsClient.Instance.LastDisconnectReason));
			break;
		case DisconnectReasons.DuplicateConnectionDetected:
			SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorDuplicateConnection));
			break;
		case DisconnectReasons.SelfPlatformLock:
			if (AmongUsClient.Instance.CrossplayPrivilegeError == CrossplayPrivilegeErrorType.Crossplay)
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorCrossPlat));
			}
			else if (AmongUsClient.Instance.CrossplayPrivilegeError == CrossplayPrivilegeErrorType.CrossplayCommunication)
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorCrossPlatformCommunication));
			}
			else
			{
				SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorCommunications));
			}
			break;
		}
	}

	private void SetText(string text)
	{
		((TMP_Text)_textArea).text = text;
		SelectableHyperLinkHelper.AddSelectableUiForHyperlinks(selectableHyperLinks, ((Object)this).name);
		OnTextChanged();
	}

	public void ShowCustom(string message)
	{
		((Component)this).gameObject.SetActive(true);
		SetText(message);
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnTextChanged()
	{
		((TMP_Text)_textArea).text = SelectableHyperLinkHelper.DecomposeAnnouncementText(_textArea, selectableHyperLinks, ((Object)this).name, ((TMP_Text)_textArea).text);
		SelectableHyperLinkHelper.UpdateHyperlinkPositions(_textArea, selectableHyperLinks, ((Object)this).name);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}
}
