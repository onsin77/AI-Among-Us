using UnityEngine;

public class AmbientSoundPlayer : MonoBehaviour
{
	public AudioClip AmbientSound;

	public Collider2D[] HitAreas;

	public float MaxVolume = 1f;

	public float DistanceFallOff = -1f;

	public float FallOffRate = 1f;

	[SerializeField]
	private bool useMuffled;

	public void Start()
	{
		if (useMuffled)
		{
			SoundManager.Instance.PlayDynamicSound(((Object)this).name + ((Object)this).GetInstanceID(), AmbientSound, loop: true, Dynamics, SoundManager.Instance.AmbienceHighPassChannel);
		}
		else
		{
			SoundManager.Instance.PlayDynamicSound(((Object)this).name + ((Object)this).GetInstanceID(), AmbientSound, loop: true, Dynamics, SoundManager.Instance.AmbienceChannel);
		}
	}

	private void Dynamics(AudioSource source, float dt)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			source.volume = 0f;
			return;
		}
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		bool flag = false;
		for (int i = 0; i < HitAreas.Length; i++)
		{
			if (HitAreas[i].OverlapPoint(truePosition))
			{
				flag = true;
				break;
			}
		}
		float num = 0f;
		if (flag)
		{
			num = 1f;
		}
		else if (DistanceFallOff >= 0f)
		{
			float num2 = Vector2.Distance(truePosition, Vector2.op_Implicit(((Component)this).transform.position));
			num = 1f - Mathf.Clamp(num2 / DistanceFallOff, 0f, 1f);
		}
		source.volume = Mathf.Lerp(source.volume, num * MaxVolume, dt * FallOffRate);
	}

	public void OnDestroy()
	{
		SoundManager.Instance.StopNamedSound(((Object)this).name + ((Object)this).GetInstanceID());
	}

	private void OnDrawGizmosSelected()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (DistanceFallOff > 0f)
		{
			Color color = Gizmos.color;
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(((Component)this).transform.position, DistanceFallOff);
			Gizmos.color = color;
		}
	}
}
