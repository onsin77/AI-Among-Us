using Newtonsoft.Json;

namespace Assets.InnerNet;

public struct AnnouncementList
{
	[JsonProperty("announcements")]
	public Announcement[] Announcements;

	[JsonProperty("page")]
	public uint Page;

	[JsonProperty("pages")]
	public uint Pages;
}
