using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AmongUs.Data;
using ImaginationOverflow.UniversalDeepLinking;
using Innersloth.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Twitch;

public class TwitchManager : DestroyableSingleton<TwitchManager>
{
	private enum UpdateState
	{
		NotStarted,
		Started,
		Success,
		Failure
	}

	private const string RedirectUri = "AmongUs://callback";

	private const string ClientId = "yioca4gf70qx0v05qodt6tnwlkerr3";

	private static readonly string[] Scopes = new string[0];

	private string verify;

	public GenericPopup TwitchPopup;

	private UpdateState running;

	private List<string> newItems = new List<string>();

	public string Token { get; set; }

	private void Start()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		LinkProviderFactory.DeferredExePath = Path.Combine(Application.dataPath.Replace('/', '\\'), "Resources", "AmongUsHelper.exe");
		DeepLinkManager.Instance.LinkActivated += Instance_LinkActivated;
	}

	private void Instance_LinkActivated(LinkActivation s)
	{
		Debug.Log((object)("Twitch link activated " + s.Uri));
		if (!s.Uri.StartsWith("amongus://callback", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		Debug.Log((object)"Twitch link is correct format");
		string text = s.RawQueryString;
		int num = text.IndexOf('#');
		if (num != -1)
		{
			text = text.Substring(num + 1);
		}
		string path = Path.Combine(PlatformPaths.persistentDataPath, "twitch_verify");
		if (!FileIO.Exists(path))
		{
			return;
		}
		verify = FileIO.ReadAllText(path);
		string[] source = text.Split('&');
		string text2 = source.First((string a) => a.StartsWith("state"));
		string text3 = text2.Substring(text2.LastIndexOf('=') + 1);
		if (!(verify != text3))
		{
			string text4 = source.First((string a) => a.StartsWith("access_token"));
			string text5 = text4.Substring(text4.LastIndexOf('=') + 1);
			FileIO.WriteAllText(Path.Combine(PlatformPaths.persistentDataPath, "twitch"), text5);
			Token = text5;
			LaunchImplicitAuthAsync();
		}
	}

	public override void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		DeepLinkManager.Instance.LinkActivated -= Instance_LinkActivated;
		base.OnDestroy();
	}

	public void LaunchImplicitAuth(Transform target)
	{
		if (running != UpdateState.Started)
		{
			running = UpdateState.Started;
			((MonoBehaviour)this).StartCoroutine(ShakeGlitch(target));
			LaunchImplicitAuthAsync();
		}
	}

	private IEnumerator ShakeGlitch(Transform target)
	{
		while (running == UpdateState.Started)
		{
			yield return Effects.Bounce(((Component)this).transform, 1f, 0.2f);
			yield return Effects.Wait(0.3f);
		}
		if (running == UpdateState.Success)
		{
			if (newItems.Count > 0)
			{
				yield return DestroyableSingleton<InventoryManager>.Instance.CoValidateTrustedReceipt(newItems.ToArray(), CompletionToken<PurchaseStates>.Default);
			}
			TwitchPopup.Show("Success!");
		}
	}

	private async void LaunchImplicitAuthAsync()
	{
		_ = 4;
		try
		{
			string path = Path.Combine(PlatformPaths.persistentDataPath, "twitch");
			if (!FileIO.Exists(path))
			{
				await FetchNewToken();
				await FetchEntitlements(Token);
			}
			else
			{
				Token = FileIO.ReadAllText(path);
				if (await FetchEntitlements(Token) == HttpStatusCode.Unauthorized)
				{
					await FetchNewToken();
					await FetchEntitlements(Token);
				}
			}
			running = UpdateState.Success;
		}
		catch (Exception ex)
		{
			TwitchPopup.Show(ex.Message);
			running = UpdateState.Failure;
		}
	}

	private async Task FetchNewToken()
	{
		Token = null;
		Random random = new Random();
		verify = random.Next(0, int.MaxValue).ToString() + random.Next(0, int.MaxValue);
		FileIO.WriteAllText(Path.Combine(PlatformPaths.persistentDataPath, "twitch_verify"), verify);
		string tokenCache = Path.Combine(PlatformPaths.persistentDataPath, "twitch");
		FileIO.Delete(tokenCache);
		Debug.Log((object)"Starting async..");
		string text = string.Join(" ", Scopes);
		Application.OpenURL("https://id.twitch.tv/oauth2/authorize?client_id=yioca4gf70qx0v05qodt6tnwlkerr3&redirect_uri=AmongUs://callback&response_type=token&scope=" + text + "&state=" + verify);
		while (!FileIO.Exists(tokenCache) || Token != null)
		{
			await Task.Delay(100);
		}
		if (Token == null)
		{
			Token = FileIO.ReadAllText(tokenCache);
		}
		Debug.Log((object)("Got Token: " + Token));
		try
		{
			FileIO.WriteAllText(tokenCache, Token);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	public async Task<HttpStatusCode> FetchEntitlements(string token)
	{
		newItems.Clear();
		using (HttpClient http = new HttpClient())
		{
			using HttpRequestMessage msg = new HttpRequestMessage();
			msg.Method = HttpMethod.Get;
			msg.RequestUri = new Uri("https://api.twitch.tv/helix/entitlements/drops");
			msg.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
			msg.Headers.TryAddWithoutValidation("Client-Id", "yioca4gf70qx0v05qodt6tnwlkerr3");
			using HttpResponseMessage res = await http.SendAsync(msg);
			_ = 1;
			try
			{
				if (res.Content == null)
				{
					Debug.Log((object)("Server returned no data: " + res.StatusCode));
					return HttpStatusCode.ExpectationFailed;
				}
				if (res.StatusCode != HttpStatusCode.OK)
				{
					return res.StatusCode;
				}
				string text = await res.Content.ReadAsStringAsync();
				JToken val = JObject.Parse(text)["data"];
				if (!val.HasValues)
				{
					Debug.Log((object)("Server returned no drops: " + text));
					return HttpStatusCode.ExpectationFailed;
				}
				foreach (JToken item in (IEnumerable<JToken>)val)
				{
					string text2 = item.Value<string>((object)"benefit_id");
					Debug.Log((object)("Drop unlocked: " + text2));
					DataManager.Player.Purchases.SetPurchased(text2);
					newItems.Add(text2);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		return HttpStatusCode.OK;
	}
}
