using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FriendListNotification : MonoBehaviour
{
	public enum IconType
	{
		Friend,
		Block,
		LobbyInvite,
		Failed,
		Sent
	}

	[Serializable]
	public class FriendNotificationIcon
	{
		public IconType type;

		public Sprite icon;

		public float scale;

		public Vector2 offset;
	}

	public List<FriendNotificationIcon> IconTypes;

	public SpriteRenderer Icon;

	public TextMeshPro Text;

	private float timeOnScreen;

	public void SetUp(string text, IconType type)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			timeOnScreen = 5f;
			((Component)this).gameObject.SetActive(true);
			((TMP_Text)Text).text = text;
			FriendNotificationIcon friendNotificationIcon = IconTypes.Find((FriendNotificationIcon i) => i.type == type);
			Icon.sprite = friendNotificationIcon.icon;
			((Component)Icon).transform.localScale = Vector2.op_Implicit(Vector2.one * friendNotificationIcon.scale);
			((Component)Icon).transform.localPosition = new Vector3(friendNotificationIcon.offset.x, friendNotificationIcon.offset.y, -1f);
		}
	}

	private void Update()
	{
		timeOnScreen -= Time.deltaTime;
		if (timeOnScreen <= 0f)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
