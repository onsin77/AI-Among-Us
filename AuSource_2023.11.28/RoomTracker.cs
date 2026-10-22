using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomTracker : MonoBehaviour
{
	public TextMeshPro text;

	public float SourceY = -2.5f;

	public float TargetY = -3.25f;

	private Collider2D playerCollider;

	private ContactFilter2D filter;

	private Collider2D[] buffer = new Collider2D[10];

	public PlainShipRoom LastRoom;

	private Coroutine slideInRoutine;

	public void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		filter = default;
		filter.layerMask = LayerMask.op_Implicit(Constants.PlayersOnlyMask);
		filter.useLayerMask = true;
		filter.useTriggers = false;
	}

	public void OnDisable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		LastRoom = null;
		Vector3 localPosition = text.transform.localPosition;
		localPosition.y = TargetY;
		text.transform.localPosition = localPosition;
	}

	public void FixedUpdate()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		PlainShipRoom[] array = null;
		if (Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
		{
			PlainShipRoom[] allRooms = LobbyBehaviour.Instance.AllRooms;
			array = allRooms;
		}
		if (Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			array = ShipStatus.Instance.AllRooms;
		}
		if (array == null)
		{
			return;
		}
		PlainShipRoom plainShipRoom = null;
		if (Object.op_Implicit((Object)(object)LastRoom))
		{
			int hitCount = LastRoom.roomArea.OverlapCollider(filter, buffer);
			if (CheckHitsForPlayer(buffer, hitCount))
			{
				plainShipRoom = LastRoom;
			}
		}
		if (!Object.op_Implicit((Object)(object)plainShipRoom))
		{
			foreach (PlainShipRoom plainShipRoom2 in array)
			{
				if (Object.op_Implicit((Object)(object)plainShipRoom2.roomArea))
				{
					int hitCount2 = plainShipRoom2.roomArea.OverlapCollider(filter, buffer);
					if (CheckHitsForPlayer(buffer, hitCount2))
					{
						plainShipRoom = plainShipRoom2;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)plainShipRoom))
		{
			if ((Object)(object)LastRoom != (Object)(object)plainShipRoom)
			{
				LastRoom = plainShipRoom;
				if (slideInRoutine != null)
				{
					((MonoBehaviour)this).StopCoroutine(slideInRoutine);
				}
				if (plainShipRoom.RoomId != SystemTypes.Hallway)
				{
					slideInRoutine = ((MonoBehaviour)this).StartCoroutine(CoSlideIn(plainShipRoom.RoomId));
				}
				else
				{
					slideInRoutine = ((MonoBehaviour)this).StartCoroutine(SlideOut());
				}
			}
		}
		else if (Object.op_Implicit((Object)(object)LastRoom))
		{
			LastRoom = null;
			if (slideInRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(slideInRoutine);
			}
			slideInRoutine = ((MonoBehaviour)this).StartCoroutine(SlideOut());
		}
	}

	private IEnumerator CoSlideIn(SystemTypes newRoom)
	{
		yield return SlideOut();
		Vector3 tempPos = text.transform.localPosition;
		Color tempColor = Color.white;
		((TMP_Text)text).text = DestroyableSingleton<TranslationController>.Instance.GetString(newRoom);
		float timer = 0f;
		while (timer < 0.25f)
		{
			timer = Mathf.Min(0.25f, timer + Time.deltaTime);
			float num = timer / 0.25f;
			tempPos.y = Mathf.SmoothStep(TargetY, SourceY, num);
			tempColor.a = Mathf.Lerp(0f, 1f, num);
			text.transform.localPosition = tempPos;
			((Graphic)text).color = tempColor;
			yield return null;
		}
	}

	private IEnumerator SlideOut()
	{
		Vector3 tempPos = text.transform.localPosition;
		Color tempColor = Color.white;
		float timer = FloatRange.ReverseLerp(tempPos.y, SourceY, TargetY) * 0.1f;
		while (timer < 0.1f)
		{
			timer = Mathf.Min(0.1f, timer + Time.deltaTime);
			float num = timer / 0.1f;
			tempPos.y = Mathf.SmoothStep(SourceY, TargetY, num);
			tempColor.a = Mathf.Lerp(1f, 0f, num);
			text.transform.localPosition = tempPos;
			((Graphic)text).color = tempColor;
			yield return null;
		}
	}

	private static bool CheckHitsForPlayer(Collider2D[] buffer, int hitCount)
	{
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			return false;
		}
		for (int i = 0; i < hitCount; i++)
		{
			if ((Object)(object)((Component)buffer[i]).gameObject == (Object)(object)((Component)PlayerControl.LocalPlayer).gameObject)
			{
				return true;
			}
		}
		return false;
	}
}
