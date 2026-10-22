using TMPro;
using UnityEngine;

public class TaskPanelBehaviour : MonoBehaviour
{
	[SerializeField]
	private Vector3 openPosition;

	[SerializeField]
	private Vector3 closedPosition;

	[SerializeField]
	private SpriteRenderer background;

	[SerializeField]
	private SpriteRenderer tab;

	[SerializeField]
	private TextMeshPro taskText;

	[SerializeField]
	private float animationTimeSeconds;

	private bool open = true;

	private float timer;

	public void ToggleOpen()
	{
		open = !open;
	}

	private void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)background).transform;
		Bounds val = ((TMP_Text)taskText).textBounds;
		Vector3 localScale;
		if (!(val.size.x > 0f))
		{
			localScale = Vector3.zero;
		}
		else
		{
			val = ((TMP_Text)taskText).textBounds;
			float num = val.size.x + 0.2f;
			val = ((TMP_Text)taskText).textBounds;
			localScale = new Vector3(num, val.size.y + 0.2f, 1f);
		}
		transform.localScale = localScale;
		val = background.sprite.bounds;
		Vector3 extents = val.extents;
		extents.y = 0f - extents.y;
		extents = extents.Mul(((Component)background).transform.localScale);
		((Component)background).transform.localPosition = extents;
		val = tab.sprite.bounds;
		Vector3 extents2 = val.extents;
		extents2 = extents2.Mul(((Component)tab).transform.localScale);
		extents2.y = 0f - extents2.y;
		extents2.x += extents.x * 2f;
		((Component)tab).transform.localPosition = extents2;
		if (!((Object)(object)GameManager.Instance == (Object)null))
		{
			if (GameManager.Instance.IsHideAndSeek())
			{
				closedPosition.y = (openPosition.y = 1.6f);
			}
			else
			{
				closedPosition.y = (openPosition.y = 0.6f);
			}
			ref Vector3 reference = ref closedPosition;
			val = background.sprite.bounds;
			reference.x = (0f - val.size.x) * ((Component)background).transform.localScale.x;
			if (open)
			{
				timer = Mathf.Min(1f, timer + Time.deltaTime / animationTimeSeconds);
			}
			else
			{
				timer = Mathf.Max(0f, timer - Time.deltaTime / animationTimeSeconds);
			}
			Vector3 relativePos = new Vector3(Mathf.SmoothStep(closedPosition.x, openPosition.x, timer), Mathf.SmoothStep(closedPosition.y, openPosition.y, timer), openPosition.z);
			((Component)this).transform.localPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop, relativePos);
		}
	}

	public void SetTaskText(string str)
	{
		((TMP_Text)taskText).text = str;
	}
}
