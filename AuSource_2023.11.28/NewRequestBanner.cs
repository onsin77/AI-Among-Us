using TMPro;
using UnityEngine;

public class NewRequestBanner : MonoBehaviour
{
	[SerializeField]
	private GameObject activeBanner;

	[SerializeField]
	private GameObject inactiveBanner;

	[SerializeField]
	private SpriteRenderer parentRenderer;

	[SerializeField]
	private SpriteRenderer ourSprite;

	[SerializeField]
	private TextMeshPro[] notifText;

	private void Update()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		int totalCount = DestroyableSingleton<FriendsListManager>.Instance.ReceivedLobbyInvites.Count + DestroyableSingleton<FriendsListManager>.Instance.ReceivedRequests.Count;
		activeBanner.SetActive(totalCount > 0);
		inactiveBanner.SetActive(totalCount > 0);
		notifText.ForEach((TextMeshPro t) =>
		{
			((TMP_Text)t).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NewRequests, totalCount);
		});
		((Component)this).transform.position = new Vector3(((Component)parentRenderer).transform.position.x - ourSprite.size.x / 2f * ((Component)ourSprite).transform.localScale.x - parentRenderer.size.x * 0.4f * ((Component)parentRenderer).transform.localScale.x, ((Component)this).transform.position.y, ((Component)this).transform.position.z);
		notifText.ForEach((TextMeshPro t) =>
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			t.transform.position = new Vector3(((Component)ourSprite).transform.position.x, t.transform.position.y, t.transform.position.z);
		});
	}
}
