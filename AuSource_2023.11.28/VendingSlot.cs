using System.Collections;
using UnityEngine;

public class VendingSlot : MonoBehaviour
{
	public SpriteRenderer DrinkImage;

	public SpriteRenderer GlassImage;

	private const float SlideDuration = 0.75f;

	private const float SlideVibrateIntensity = 0.05f;

	private const float DrunkThunkVibrateIntensity = 0.4f;

	public IEnumerator CoBuy(AudioClip sliderOpen, AudioClip drinkShake, AudioClip drinkLand)
	{
		VibrationManager.Vibrate(0.05f, 0.05f, 0.75f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(sliderOpen, loop: false);
		}
		yield return new WaitForLerp(0.75f, (float v) =>
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			GlassImage.size = new Vector2(1f, Mathf.Lerp(1.7f, 0f, v));
			((Component)GlassImage).transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, 0.85f, v), -1f);
		});
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(drinkShake, loop: false);
		}
		yield return Effects.SwayX(((Component)DrinkImage).transform, 0.75f, 0.075f);
		Vector3 localPosition = ((Component)DrinkImage).transform.localPosition;
		localPosition.z = -5f;
		((Component)DrinkImage).transform.localPosition = localPosition;
		Vector3 val = localPosition;
		val.y = -8f - localPosition.y;
		yield return Effects.All(Effects.Slide2D(((Component)DrinkImage).transform, Vector2.op_Implicit(localPosition), Vector2.op_Implicit(val)), Effects.Rotate2D(((Component)DrinkImage).transform, 0f, 0f - FloatRange.Next(-45f, 45f)), Effects.Sequence(Effects.Wait(0.25f), PlayLand(drinkLand)));
		((Renderer)DrinkImage).enabled = false;
	}

	public IEnumerator CloseSlider(AudioClip sliderOpen)
	{
		VibrationManager.Vibrate(0.05f, 0.05f, 0.75f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(sliderOpen, loop: false);
		}
		yield return new WaitForLerp(0.75f, (float v) =>
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			GlassImage.size = new Vector2(1f, Mathf.Lerp(0f, 1.7f, v));
			((Component)GlassImage).transform.localPosition = new Vector3(0f, Mathf.Lerp(0.85f, 0f, v), -1f);
		});
	}

	private IEnumerator PlayLand(AudioClip drinkLand)
	{
		VibrationManager.Vibrate(0.4f, 0.4f, 0.5f, VibrationManager.VibrationFalloff.Linear);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(drinkLand, loop: false);
		}
		yield break;
	}
}
