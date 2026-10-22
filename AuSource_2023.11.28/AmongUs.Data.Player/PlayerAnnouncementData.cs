using System;
using System.Collections.Generic;
using Assets.InnerNet;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerAnnouncementData : AbstractSaveDataGroup
{
	[JsonProperty("announcementsRead")]
	private List<int> ids = new List<int>();

	[JsonProperty("latestAnnouncementNumber")]
	private int latestAnnouncementNumber;

	private List<Announcement> allAnnouncements = new List<Announcement>();

	public List<int> AnnouncementsRead
	{
		get
		{
			return ids;
		}
		set
		{
			SetValue(ref ids, value, OnReadAnnouncement);
		}
	}

	public int LatestAnnouncementNumber
	{
		get
		{
			return latestAnnouncementNumber;
		}
		set
		{
			SetValue(ref latestAnnouncementNumber, value, OnNewAnnouncement);
		}
	}

	public List<Announcement> AllAnnouncements => allAnnouncements;

	public event Action OnReadAnnouncement;

	public event Action OnNewAnnouncement;

	public event Action OnAddAnnouncement;

	public void AddAnnouncement(Announcement a)
	{
		allAnnouncements.Add(a);
		HandleChange();
		OnAddAnnouncement?.Invoke();
	}

	public void SetAnnouncements(Announcement[] aRange)
	{
		allAnnouncements = new List<Announcement>(aRange);
		HandleChange();
		OnAddAnnouncement?.Invoke();
	}
}
