using UnityEngine;

public class SensorDoor : MonoBehaviour
{
	public SpriteRenderer LeftSide;

	public SpriteRenderer RightSide;

	public float ActivationDistance = 2f;

	public bool Opening;

	public float OpenDuration;

	private float openTimer;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	private const float slideVibrationIntensity = 3f;

	public void OnEnable()
	{
		((MonoBehaviour)this).InvokeRepeating("CheckDoor", 0.1f, 0.1f);
		LeftSide.SetCooldownNormalizedUvs();
		RightSide.SetCooldownNormalizedUvs();
	}

	[ContextMenu("Set Right Uvs")]
	public void SetUvs()
	{
		RightSide.SetCooldownNormalizedUvs();
	}

	private void Update()
	{
		if (Opening && openTimer < OpenDuration)
		{
			openTimer += Time.deltaTime;
			float num = Mathf.SmoothStep(0f, 1f, openTimer / OpenDuration);
			((Renderer)LeftSide).material.SetFloat("_Percent", num);
			((Renderer)RightSide).material.SetFloat("_Percent", num);
		}
		else if (!Opening && openTimer > 0f)
		{
			openTimer -= Time.deltaTime;
			float num2 = Mathf.SmoothStep(0f, 1f, openTimer / OpenDuration);
			((Renderer)LeftSide).material.SetFloat("_Percent", num2);
			((Renderer)RightSide).material.SetFloat("_Percent", num2);
		}
	}

	private void CheckDoor()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		bool opening = Opening;
		Opening = PhysicsHelpers.CircleContains(Vector2.op_Implicit(((Component)this).transform.position), ActivationDistance, Constants.PlayersOnlyMask);
		if (opening && !Opening)
		{
			SoundManager.Instance.StopSound(OpenSound);
			if (Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), PlayerControl.LocalPlayer.GetTruePosition()) < 3f)
			{
				SoundManager.Instance.PlaySound(CloseSound, loop: false);
			}
			VibrationManager.Vibrate(3f, Vector2.op_Implicit(((Component)this).transform.position), 3f, OpenDuration, VibrationManager.VibrationFalloff.None, CloseSound);
		}
		else if (!opening && Opening)
		{
			SoundManager.Instance.StopSound(CloseSound);
			if (Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), PlayerControl.LocalPlayer.GetTruePosition()) < 3f)
			{
				SoundManager.Instance.PlaySound(OpenSound, loop: false);
			}
			VibrationManager.Vibrate(3f, Vector2.op_Implicit(((Component)this).transform.position), 3f, OpenDuration, VibrationManager.VibrationFalloff.None, OpenSound);
		}
	}
}
