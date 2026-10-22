using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using AmongUs.Data;
using AmongUs.GameOptions;
using AmongUs.HTTP;
using InnerNet;
using Newtonsoft.Json;
using UnityEngine;

public class HttpMatchmakerManager : DestroyableSingleton<HttpMatchmakerManager>
{
	public class MatchmakerResponse
	{
		public MatchmakerError[] Errors;
	}

	public class MatchmakerError
	{
		public DisconnectReasons Reason;

		public SanctionReasons SanctionReason;

		public DateTimeOffset EndsAt;
	}

	public class UserTokenRequestData
	{
		public string Puid { get; set; }

		public string Username { get; set; }

		public int ClientVersion { get; set; }

		public SupportedLangs Language { get; set; }
	}

	public class HostServer
	{
		public readonly string Ip;

		public readonly ushort Port;

		public HostServer(long ip, ushort port)
		{
			Ip = new IPAddress(ip).ToString();
			Port = port;
		}
	}

	public class MatchmakerFailure
	{
		public DisconnectReasons Reason { get; set; }

		public string CustomDisconnect { get; set; }

		public MatchmakerError MatchmakerError { get; set; }

		public bool ShouldGoOffline { get; set; }
	}

	private const string CONTEXT_AUTHENTICATE = "authenticate";

	private const string CONTEXT_REQUEST_GAME_CODE = "request gamecode server";

	private const string CONTEXT_FIND_HOST_SERVER = "find host server";

	private const string CONTEXT_REQUEST_GAMES_LIST = "request games list";

	private const string CONTEXT_GET_FILTER_TAGS = "get filter tags";

	private readonly Logger logger = new Logger("HttpMatchmakerManager", Logger.Level.Info, Logger.Category.Network);

	private static readonly Dictionary<string, StringNames> ContextMessages = new Dictionary<string, StringNames>
	{
		{
			"authenticate",
			StringNames.HttpErrorContextAuthenticate
		},
		{
			"request gamecode server",
			StringNames.HttpErrorContextRequestGameCode
		},
		{
			"find host server",
			StringNames.HttpErrorContextFindHostServer
		},
		{
			"request games list",
			StringNames.HttpErrorContextRequestGamesList
		}
	};

	private static readonly Dictionary<long, StringNames> ErrorMessages = new Dictionary<long, StringNames>
	{
		{
			0L,
			StringNames.HttpErrorUnknown
		},
		{
			400L,
			StringNames.HttpErrorBadRequest
		},
		{
			401L,
			StringNames.HttpErrorUnauthorized
		},
		{
			403L,
			StringNames.HttpErrorForbidden
		},
		{
			404L,
			StringNames.HttpErrorNotFound
		},
		{
			405L,
			StringNames.HttpErrorMethodNotAllowed
		},
		{
			408L,
			StringNames.HttpErrorRequestTimeout
		},
		{
			429L,
			StringNames.HttpErrorTooManyRequests
		},
		{
			500L,
			StringNames.HttpErrorInternalServerError
		},
		{
			502L,
			StringNames.HttpErrorBadGateway
		},
		{
			503L,
			StringNames.HttpErrorServiceUnavailable
		},
		{
			504L,
			StringNames.HttpErrorGatewayTimeout
		}
	};

	private MatchmakerToken token;

	private string base64Token;

	public IEnumerator CoGetOrRefreshToken(Action<string> onGetToken)
	{
		if (TryReadCachedToken(out var matchmakerToken))
		{
			onGetToken(matchmakerToken);
		}
		else
		{
			yield return CoRefreshTokenInternal(onGetToken);
		}
	}

	private bool TryReadCachedToken(out string matchmakerToken)
	{
		if (token != null && token.IsValid)
		{
			matchmakerToken = base64Token;
			return true;
		}
		base64Token = null;
		token = null;
		matchmakerToken = null;
		return false;
	}

	private IEnumerator CoRefreshTokenInternal(Action<string> onGetToken)
	{
		if (string.IsNullOrEmpty(DestroyableSingleton<EOSManager>.Instance.UserIDToken))
		{
			AmongUsClient.Instance.LastCustomDisconnect = "User is not logged in - can not authenticate";
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.Custom;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
			yield break;
		}
		string url = DestroyableSingleton<ServerManager>.Instance.CurrentUdpServer.HttpUrl + "api/user";
		string body = JsonConvert.SerializeObject((object)new UserTokenRequestData
		{
			Puid = DestroyableSingleton<EOSManager>.Instance.ProductUserId,
			Username = DataManager.Player.Customization.Name,
			ClientVersion = Constants.GetBroadcastVersion(),
			Language = DataManager.Settings.Language.CurrentLanguage
		});
		RetryableWebRequest retryableWebRequest = RetryableWebRequest.Post(url, body);
		retryableWebRequest.SetOrReplaceRequestHeader("Content-Type", "application/json");
		retryableWebRequest.SetOrReplaceRequestHeader("Accept", "text/plain");
		retryableWebRequest.SetOrReplaceRequestHeader("Authorization", "Bearer " + DestroyableSingleton<EOSManager>.Instance.UserIDToken);
		retryableWebRequest.SetOrReplaceSuccessCallback((string encodedToken) =>
		{
			if (!MatchmakerToken.TryParse(encodedToken, out var matchmakerToken))
			{
				logger.Warning("Hmm, the server sent a corrupt token: " + encodedToken);
			}
			else
			{
				base64Token = encodedToken;
				token = matchmakerToken;
				onGetToken(encodedToken);
			}
		});
		logger.Info("Sent UserIDToken Request");
		yield return CoSendRequest(retryableWebRequest, "authenticate");
	}

	public IEnumerator CoFindGameCode(int gameId, Action<HostServer, string> onGameHost)
	{
		string uri = DestroyableSingleton<ServerManager>.Instance.CurrentUdpServer.HttpUrl + "api/games?gameId=" + gameId;
		string matchmakerToken = string.Empty;
		yield return CoGetOrRefreshToken((string token) =>
		{
			matchmakerToken = token;
		});
		if (string.IsNullOrEmpty(matchmakerToken))
		{
			yield break;
		}
		RetryableWebRequest retryableWebRequest = RetryableWebRequest.Post(uri, "");
		retryableWebRequest.SetOrReplaceRequestHeader("Accept", "application/json");
		retryableWebRequest.SetOrReplaceRequestHeader("Authorization", "Bearer " + matchmakerToken);
		retryableWebRequest.SetOrReplaceSuccessCallback((string server) =>
		{
			//IL_001a: Expected Obj, but got Unknown
			try
			{
				onGameHost(JsonConvert.DeserializeObject<HostServer>(server), matchmakerToken);
			}
			catch (JsonException ex)
			{
				JsonException ex2 = ex;
				logger.Warning("CoFindGameCode failed to deserialize response: " + ((Exception)(object)ex2).Message);
				logger.Warning("\t" + server);
			}
		});
		logger.Info("Sent FindGameCode Request");
		yield return CoSendRequest(retryableWebRequest, "request gamecode server");
	}

	public IEnumerator CoFindHostServer(Action<HostServer, string> onGameHost)
	{
		string uri = DestroyableSingleton<ServerManager>.Instance.CurrentUdpServer.HttpUrl + "api/games";
		if (!DestroyableSingleton<ServerManager>.Instance.TargetServer.IsNullOrWhiteSpace())
		{
			uri = uri + "?targetServer=" + GameCode.GameNameToInt("QQ" + DestroyableSingleton<ServerManager>.Instance.TargetServer);
		}
		string matchmakerToken = string.Empty;
		yield return CoGetOrRefreshToken((string token) =>
		{
			matchmakerToken = token;
		});
		if (string.IsNullOrEmpty(matchmakerToken))
		{
			yield break;
		}
		RetryableWebRequest retryableWebRequest = RetryableWebRequest.Put(uri, new byte[1]);
		retryableWebRequest.SetOrReplaceRequestHeader("Accept", "application/json");
		retryableWebRequest.SetOrReplaceRequestHeader("Authorization", "Bearer " + matchmakerToken);
		retryableWebRequest.SetOrReplaceSuccessCallback((string server) =>
		{
			//IL_001a: Expected Obj, but got Unknown
			try
			{
				onGameHost(JsonConvert.DeserializeObject<HostServer>(server), matchmakerToken);
			}
			catch (JsonException ex)
			{
				JsonException ex2 = ex;
				logger.Warning("CoFindHostServer failed to deserialize response: " + ((Exception)(object)ex2).Message);
				logger.Warning("\t" + server);
			}
		});
		logger.Info("Sent FindHost Request");
		yield return CoSendRequest(retryableWebRequest, "find host server");
	}

	public IEnumerator CoRequestGameList(IGameOptions gameSearchOptions, GameFilterOptions filterOpts, Action<InnerNetClient.TotalGameData, List<GameListing>> onGameList)
	{
		Dictionary<string, object> uriParams = new Dictionary<string, object>
		{
			{ "mapId", gameSearchOptions.MapId },
			{ "lang", gameSearchOptions.Keywords },
			{
				"quickChat",
				DataManager.Settings.Multiplayer.ChatMode
			},
			{
				"platformFlags",
				CrossplayMode.GetCrossplayFlags()
			},
			{ "numImpostors", gameSearchOptions.NumImpostors },
			{
				"gameMode",
				(byte)gameSearchOptions.GameMode
			},
			{
				"filterTags",
				string.Join(",", filterOpts.FilterTags)
			}
		};
		string uri = DestroyableSingleton<ServerManager>.Instance.CurrentUdpServer.HttpUrl + "api/games?" + HttpUtils.ToUriQuery(uriParams);
		string matchmakerToken = string.Empty;
		yield return CoGetOrRefreshToken((string token) =>
		{
			matchmakerToken = token;
		});
		if (string.IsNullOrEmpty(matchmakerToken))
		{
			yield break;
		}
		RetryableWebRequest retryableWebRequest = RetryableWebRequest.Get(uri);
		retryableWebRequest.SetOrReplaceRequestHeader("Accept", "application/json");
		retryableWebRequest.SetOrReplaceRequestHeader("Authorization", "Bearer " + matchmakerToken);
		retryableWebRequest.SetOrReplaceSuccessCallback((string games) =>
		{
			//IL_001b: Expected Obj, but got Unknown
			InnerNetClient.TotalGameData arg = new InnerNetClient.TotalGameData();
			try
			{
				onGameList(arg, JsonConvert.DeserializeObject<List<GameListing>>(games));
			}
			catch (JsonException ex)
			{
				JsonException ex2 = ex;
				logger.Warning("CoRequestGameList failed to deserialize response: " + ((Exception)(object)ex2).Message);
				logger.Warning("\t" + games);
			}
		});
		logger.Info("Sent GetGamesRequest");
		yield return CoSendRequest(retryableWebRequest, "request games list");
	}

	public IEnumerator CoRefreshTags(Action<HashSet<string>> onTags)
	{
		string matchmakerToken = string.Empty;
		yield return CoGetOrRefreshToken((string token) =>
		{
			matchmakerToken = token;
		});
		if (string.IsNullOrEmpty(matchmakerToken))
		{
			yield break;
		}
		Dictionary<string, object> uriParams = new Dictionary<string, object> { 
		{
			"lang",
			(uint)DataManager.Settings.Language.CurrentLanguage
		} };
		RetryableWebRequest retryableWebRequest = RetryableWebRequest.Get(DestroyableSingleton<ServerManager>.Instance.CurrentUdpServer.HttpUrl + "api/filtertags?" + HttpUtils.ToUriQuery(uriParams));
		retryableWebRequest.SetOrReplaceRequestHeader("Accept", "application/json");
		retryableWebRequest.SetOrReplaceRequestHeader("Authorization", "Bearer " + matchmakerToken);
		retryableWebRequest.SetOrReplaceSuccessCallback((string tags) =>
		{
			//IL_002e: Expected Obj, but got Unknown
			if (string.IsNullOrEmpty(tags))
			{
				onTags(new HashSet<string>());
				return;
			}
			try
			{
				onTags(JsonConvert.DeserializeObject<HashSet<string>>(tags));
			}
			catch (JsonException ex)
			{
				JsonException ex2 = ex;
				logger.Warning("CoRefreshTags failed to deserialize response: " + ((Exception)(object)ex2).Message);
				logger.Warning("\t" + tags);
			}
		});
		logger.Info("Sent GetFilterTags");
		yield return CoSendRequest(retryableWebRequest, "get filter tags", 2, (MatchmakerFailure failure) =>
		{
			logger.Warning("CoRefreshTags failed");
		});
	}

	private IEnumerator CoSendRequest(RetryableWebRequest request, string context)
	{
		return CoSendRequest(request, context, 2, SetDisconnectInfoAndShowPopup);
	}

	private IEnumerator CoSendRequest(RetryableWebRequest request, string context, int maxRetries, Action<MatchmakerFailure> onFailure)
	{
		request.SetOrReplaceErrorCallback((RetryableWebRequest r) =>
		{
			string message = string.Join(Environment.NewLine, "CoSendRequest failed:", $"\tStatus Code: {r.ResponseCode}", "\tContext: " + context, "\tRequest: " + r.Method + " at " + r.Url, "\tResponse: " + (r.ResponseText ?? "null"), "\tError: " + (r.Error ?? "null"));
			logger.Info(message);
		});
		yield return request.CoSend(logger);
		if (request.IsSuccess)
		{
			yield break;
		}
		if (request.IsAuthError && context == "authenticate")
		{
			onFailure?.Invoke(BundleFailureInfo(request, context));
			yield break;
		}
		if (request.IsAuthError)
		{
			logger.Info("Authorization failure. Attempting to refresh token.");
			bool didRefreshToken = false;
			yield return CoRefreshTokenInternal((string token) =>
			{
				request.SetOrReplaceRequestHeader("Authorization", "Bearer " + token);
				didRefreshToken = true;
			});
			if (!didRefreshToken)
			{
				logger.Info("Failed to refresh matchmaker token.");
				onFailure?.Invoke(BundleFailureInfo(request, context));
				yield break;
			}
		}
		if (request.IsAuthError || request.IsTransientError)
		{
			float retryIntervalSeconds = 0.2f;
			int retryBackoffFactor = 2;
			for (int retries = 1; retries <= maxRetries; retries++)
			{
				float num = (float)((double)retryIntervalSeconds * Math.Pow(retryBackoffFactor, retries));
				yield return (object)new WaitForSeconds(num);
				yield return request.CoSend(logger);
				if (request.IsSuccess)
				{
					yield break;
				}
				if (!request.IsTransientError)
				{
					break;
				}
			}
		}
		onFailure?.Invoke(BundleFailureInfo(request, context));
	}

	private MatchmakerFailure BundleFailureInfo(RetryableWebRequest request, string context)
	{
		//IL_01a9: Expected Obj, but got Unknown
		if (!ContextMessages.TryGetValue(context, out var value))
		{
			value = StringNames.HttpErrorContextNone;
		}
		if (!ErrorMessages.TryGetValue(request.ResponseCode, out var value2))
		{
			value2 = StringNames.HttpErrorUnknown;
		}
		bool flag = request.ResponseCode == 0;
		string customDisconnect = (flag ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorGenericOnlineDisconnect) : (DestroyableSingleton<TranslationController>.Instance.GetString(value) + ".\n" + DestroyableSingleton<TranslationController>.Instance.GetString(value2)));
		MatchmakerFailure matchmakerFailure = new MatchmakerFailure
		{
			Reason = DisconnectReasons.Custom,
			CustomDisconnect = customDisconnect,
			MatchmakerError = null,
			ShouldGoOffline = flag
		};
		if (string.IsNullOrEmpty(request.ResponseText))
		{
			logger.Warning(string.Format("HTTP-MM request failed internally\nStatus Code: {0}\nContext: {1}\nRequest: {2} at {3}\nResponse: {4}\nError: {5}", request.ResponseCode, context, request.Method, request.Url, request.ResponseText ?? "null", request.Error ?? "null"));
			return matchmakerFailure;
		}
		try
		{
			MatchmakerResponse matchmakerResponse = JsonConvert.DeserializeObject<MatchmakerResponse>(request.ResponseText);
			if (matchmakerResponse.Errors.Length != 0)
			{
				MatchmakerError matchmakerError = matchmakerResponse.Errors[0];
				matchmakerFailure.Reason = matchmakerError.Reason;
				matchmakerFailure.MatchmakerError = matchmakerError;
			}
			logger.Warning($"HTTP-MM request failed with reason\nStatus Code: {request.ResponseCode}\nContext: {context}\nRequest: {request.Method} at {request.Url}\nReason: {matchmakerResponse.Errors[0].Reason}");
		}
		catch (JsonException ex)
		{
			JsonException ex2 = ex;
			logger.Warning(string.Format("HTTP-MM request failed but could not parse response\nStatus Code: {0}\nContext: {1}\nRequest: {2} at {3}\nResponse: {4}\nError: {5}\nStacktrace = {6}", request.ResponseCode, context, request.Method, request.Url, request.ResponseText ?? "null", ((Exception)(object)ex2).Message, ((Exception)(object)ex2).StackTrace));
		}
		return matchmakerFailure;
	}

	private void SetDisconnectInfoAndShowPopup(MatchmakerFailure failure)
	{
		AmongUsClient.Instance.LastDisconnectReason = failure.Reason;
		AmongUsClient.Instance.LastCustomDisconnect = failure.CustomDisconnect;
		if (failure.MatchmakerError != null)
		{
			AmongUsClient.Instance.LastMatchmakerError = failure.MatchmakerError;
		}
		if (failure.ShouldGoOffline)
		{
			DestroyableSingleton<EOSManager>.Instance.ContinueInOfflineMode();
		}
		DestroyableSingleton<DisconnectPopup>.Instance.Show();
	}
}
