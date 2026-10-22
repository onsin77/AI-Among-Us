using InnerNet;
using PowerTools;
using TMPro;
using UnityEngine;

public class MatchMakerGameButton : PoolableBehavior, IConnectButton
{
	public TextMeshPro NameText;

	public TextMeshPro SmallNameText;

	public TextMeshPro PlayerCountText;

	public TextMeshPro ImpostorCountText;

	public SpriteRenderer MapIcon;

	public PlatformIdentifier PlatformIdentifier;

	public TextMeshPro LanguageText;

	public Sprite[] MapIcons;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public GameListing myListing;

	public void OnClick()
	{
		if (DestroyableSingleton<MatchMaker>.Instance.Connecting(this) && myListing.IP != 0)
		{
			Debug.Log((object)("Connecting to: " + InnerNetClient.AddressToString(myListing.IP) + " for " + GameCode.IntToGameName(myListing.GameId)));
			((MonoBehaviour)this).StartCoroutine(AmongUsClient.Instance.CoJoinOnlinePublicGame(myListing.GameId, myListing.IPString, myListing.Port));
		}
	}

	public void StartIcon()
	{
		connectIcon.Play(connectClip);
	}

	public void StopIcon()
	{
		connectIcon.Stop();
		((Component)connectIcon).GetComponent<SpriteRenderer>().sprite = null;
	}

	public void SetGame(GameListing gameListing)
	{
		myListing = gameListing;
		if (Constants.GetPlatformType() == gameListing.Platform && PlatformIdentifier.SupportsPlatformUsername(gameListing.Platform))
		{
			((Component)SmallNameText).gameObject.SetActive(true);
			((Component)NameText).gameObject.SetActive(false);
			((TMP_Text)SmallNameText).text = myListing.HostName;
		}
		else
		{
			((Component)SmallNameText).gameObject.SetActive(false);
			((Component)NameText).gameObject.SetActive(true);
			((TMP_Text)NameText).text = myListing.HostName;
		}
		if (myListing.Language == 1)
		{
			((TMP_Text)LanguageText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OtherLanguage);
		}
		else
		{
			((TMP_Text)LanguageText).text = ChatLanguageSet.Instance.GetString(myListing.Language);
		}
		((Component)PlatformIdentifier).gameObject.SetActive(false);
		((TMP_Text)ImpostorCountText).text = myListing.NumImpostors.ToString();
		((TMP_Text)PlayerCountText).text = $"{myListing.PlayerCount}/{myListing.MaxPlayers}";
		MapIcon.sprite = MapIcons[Mathf.Clamp((int)myListing.MapId, 0, MapIcons.Length - 1)];
	}
}
