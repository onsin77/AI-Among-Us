using System;
using System.Collections.Generic;
using AmongUs.Data;
using Assets.InnerNet;
using TMPro;
using UnityEngine;

public class AnnouncementPanel : MonoBehaviour
{
	public PassiveButton PassiveButton;

	[SerializeField]
	private TextMeshPro TitleText;

	[SerializeField]
	private TextMeshPro DateText;

	[SerializeField]
	private SpriteRenderer Background;

	[SerializeField]
	private SpriteRenderer RightBorder;

	[SerializeField]
	private SpriteRenderer NewIcon;

	private Announcement announcement;

	private Color32 ReadColor = new Color32((byte)62, (byte)79, (byte)79, (byte)122);

	private Color32 SelectedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)143);

	public int AnnouncementNumber => announcement.Number;

	public void SetUp(Announcement announcement)
	{
		this.announcement = announcement;
		((Renderer)Background).enabled = false;
		((TMP_Text)TitleText).text = announcement.ShortTitle;
		((TMP_Text)DateText).text = DateTime.Parse(announcement.Date).ToLocalTime().ToString(DestroyableSingleton<TranslationController>.Instance.dateFormats[DataManager.Settings.Language.CurrentLanguage]);
	}

	public void Select()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>(DataManager.Player.Announcements.AnnouncementsRead);
		if (!list.Contains(announcement.Number))
		{
			list.Add(announcement.Number);
		}
		DataManager.Player.Announcements.AnnouncementsRead = list;
		((Renderer)RightBorder).enabled = false;
		((Renderer)Background).enabled = true;
		Background.color = Color32.op_Implicit(SelectedColor);
		((Renderer)NewIcon).enabled = false;
	}

	public void UnSelect()
	{
		((Renderer)RightBorder).enabled = true;
		MarkAsRead();
	}

	public void MarkAsRead()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)NewIcon).enabled = false;
		Background.color = Color32.op_Implicit(ReadColor);
		((Renderer)Background).enabled = true;
	}

	public AnnouncementPanel()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
	}
}
