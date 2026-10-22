using System.Collections;
using Hazel;
using UnityEngine;

public class ManualDoor : SomeKindaDoor
{
	public bool Opening;

	public BoxCollider2D myCollider;

	public Collider2D shadowCollider;

	public SpriteRenderer image;

	private float size;

	public float OpenDuration = 0.3f;

	private float openTimer;

	private float openValue;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = myCollider.size;
		size = ((val.x > val.y) ? val.y : val.x);
		image.SetCooldownNormalizedUvs();
		Opening = ((Collider2D)myCollider).isTrigger;
	}

	private void Update()
	{
		if (Opening && openTimer < OpenDuration)
		{
			openTimer += Time.deltaTime;
			openValue = Mathf.SmoothStep(0f, 1f, openTimer / OpenDuration);
			((Renderer)image).material.SetFloat("_PercentY", openValue);
		}
		else if (!Opening && openTimer > 0f)
		{
			openTimer -= Time.deltaTime;
			openValue = Mathf.SmoothStep(0f, 1f, openTimer / OpenDuration);
			((Renderer)image).material.SetFloat("_PercentY", openValue);
		}
		if ((Object)(object)shadowCollider != (Object)null)
		{
			bool flag = openValue <= 0.01f;
			if (((Behaviour)shadowCollider).enabled != flag)
			{
				((Behaviour)shadowCollider).enabled = flag;
			}
		}
	}

	public override void SetDoorway(bool open)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (Opening == open)
		{
			return;
		}
		Opening = open;
		((Collider2D)myCollider).isTrigger = open;
		((MonoBehaviour)this).StopAllCoroutines();
		if (!open)
		{
			Vector2 val = myCollider.size;
			((MonoBehaviour)this).StartCoroutine(CoCloseDoorway(val.x > val.y));
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlayDynamicSound(((Object)this).name, CloseSound, loop: false, DoorDynamics, SoundManager.Instance.SfxChannel);
			}
			VibrationManager.Vibrate(2.5f, Vector2.op_Implicit(((Component)this).transform.position), 3f, 0f, VibrationManager.VibrationFalloff.None, CloseSound);
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlayDynamicSound(((Object)this).name, OpenSound, loop: false, DoorDynamics, SoundManager.Instance.SfxChannel);
			}
			VibrationManager.Vibrate(2.5f, Vector2.op_Implicit(((Component)this).transform.position), 3f, 0f, VibrationManager.VibrationFalloff.None, OpenSound);
		}
	}

	private void DoorDynamics(AudioSource source, float dt)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			source.volume = 0f;
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(val, truePosition);
		if (num > 4f)
		{
			source.volume = 0f;
			return;
		}
		float num2 = 1f - num / 4f;
		source.volume = Mathf.Lerp(source.volume, num2, dt);
	}

	private IEnumerator CoCloseDoorway(bool isHort)
	{
		Vector2 s = myCollider.size;
		float i = 0f;
		if (isHort)
		{
			while (i < 0.1f)
			{
				i += Time.deltaTime;
				s.y = Mathf.Lerp(0.0001f, size, i / 0.1f);
				myCollider.size = s;
				yield return null;
			}
		}
		else
		{
			while (i < 0.1f)
			{
				i += Time.deltaTime;
				s.x = Mathf.Lerp(0.0001f, size, i / 0.1f);
				myCollider.size = s;
				yield return null;
			}
		}
	}

	public virtual void Serialize(MessageWriter writer)
	{
		writer.Write(Opening);
	}

	public virtual void Deserialize(MessageReader reader)
	{
		SetDoorway(reader.ReadBoolean());
	}
}
