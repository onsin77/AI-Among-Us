using UnityEngine;

public class SortableSample : MonoBehaviour
{
	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private bool isMushroom;

	[SerializeField]
	private float distanceAboveJar;

	[SerializeField]
	[Header("Audio")]
	private AudioClip pickUpSfx;

	[SerializeField]
	private AudioClip placeCorrectSfx;

	private Vector3 originalPosition;

	public Collider2D Collider => coll;

	public bool IsMushroom => isMushroom;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		originalPosition = ((Component)this).transform.localPosition;
	}

	public void StartPickUp()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(pickUpSfx, loop: false);
		}
	}

	public void SnapBackToOriginalPosition()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)coll).enabled = false;
		((MonoBehaviour)this).StartCoroutine(Effects.Sequence(Effects.Slide2D(((Component)this).transform, Vector2.op_Implicit(((Component)this).transform.localPosition), Vector2.op_Implicit(originalPosition), 0.25f), Effects.Action(() =>
		{
			((Behaviour)coll).enabled = true;
		})));
	}

	public void DropIntoContainer(Transform dropSpot)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)coll).enabled = false;
		Vector3 localPosition = ((Component)dropSpot).transform.localPosition;
		localPosition.x += Random.Range(-0.5f, 0.5f);
		Vector3 val = localPosition + new Vector3(0f, distanceAboveJar, 0f);
		((MonoBehaviour)this).StartCoroutine(Effects.Sequence(Effects.Slide2D(((Component)this).transform, Vector2.op_Implicit(((Component)this).transform.localPosition), Vector2.op_Implicit(val), 0.4f), Effects.Action(() =>
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = ((Component)this).transform.position;
			position.z = dropSpot.position.z;
			((Component)this).transform.position = position;
		}), Effects.Slide2D(((Component)this).transform, Vector2.op_Implicit(val), Vector2.op_Implicit(localPosition), 1f), Effects.Action(() =>
		{
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(placeCorrectSfx, loop: false);
			}
		})));
	}

	public void UpdatePosition(Vector3 pos)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		pos.y -= ((Component)coll).transform.localPosition.y / 2f;
		((Component)this).transform.localPosition = pos;
	}
}
