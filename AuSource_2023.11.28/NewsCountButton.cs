using AmongUs.Data;
using TMPro;
using UnityEngine;

public class NewsCountButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro countText;

	[SerializeField]
	private GameObject notifIcon;

	private void Start()
	{
		UpdateCountText();
		DataManager.Player.Announcements.OnReadAnnouncement += UpdateCountText;
	}

	private void OnDestroy()
	{
		DataManager.Player.Announcements.OnReadAnnouncement -= UpdateCountText;
	}

	public void UpdateCountText()
	{
		int num = DataManager.Player.Announcements.AllAnnouncements.Count - DataManager.Player.Announcements.AnnouncementsRead.Count;
		notifIcon.SetActive(num > 0);
		if (num > 9)
		{
			((TMP_Text)countText).text = "9+";
		}
		else
		{
			((TMP_Text)countText).text = num.ToString();
		}
	}
}
