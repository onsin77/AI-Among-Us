using System.Collections;
using PowerTools;
using UnityEngine;

public class PbExileController : ExileController
{
	public SpriteRenderer HandSlot;

	public Sprite BadHand;

	public Sprite GoodHand;

	public AudioClip SplashSound;

	public SpriteAnim Sploosher;

	public AnimationClip Sploosh;

	protected override IEnumerator Animate()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear);
		yield return Effects.Wait(0.75f);
		yield return Effects.All(PlayerFall(), PlayerSpin(), HandleText(Duration * 0.5f, Duration * 0.5f));
		if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
		{
			((Component)ImpostorText).gameObject.SetActive(true);
		}
		yield return Effects.Bloop(0f, ImpostorText.transform);
		yield return (object)new WaitForSeconds(0.5f);
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);
		WrapUp();
	}

	private IEnumerator PlayerFall()
	{
		float num = Camera.main.orthographicSize + 1f;
		Vector2 top = Vector2.up * num;
		Vector2 bottom = Vector2.down * 2.81f;
		bool started = exiled == null;
		float d1 = Duration / 1.8f;
		for (float t = 0f; t <= d1; t += Time.deltaTime)
		{
			float num2 = t / d1;
			float num3 = LerpCurve.Evaluate(num2);
			Vector2 val = Vector2.Lerp(top, bottom, num3);
			((Component)Player).transform.localPosition = Vector2.op_Implicit(val);
			if (!started && val.y < -1.7f)
			{
				started = true;
				Sploosher.Play(Sploosh);
				SoundManager.Instance.PlaySound(SplashSound, loop: false);
				VibrationManager.Vibrate(0.2f, 0.2f, 0.35f, VibrationManager.VibrationFalloff.Linear);
			}
			yield return null;
		}
	}

	private IEnumerator PlayerSpin()
	{
		float num = Camera.main.orthographicSize + 1f;
		Vector2 top = Vector2.up * num;
		Vector2 bottom = Vector2.down * 2.81f;
		float d1 = Duration / 1.8f;
		for (float t = 0f; t <= d1; t += Time.deltaTime)
		{
			_ = t / d1;
			float num2 = (t + 0.75f) * 25f / Mathf.Exp(t * 0.75f + 1f);
			((Component)Player).transform.Rotate(new Vector3(0f, 0f, num2 * Time.deltaTime * 5f));
			yield return null;
		}
		if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
		{
			if (exiled != null)
			{
				HandSlot.sprite = (exiled.Role.IsImpostor ? GoodHand : BadHand);
				PlayerMaterial.SetColors(exiled.DefaultOutfit.ColorId, (Renderer)(object)HandSlot);
			}
			((Component)Player).transform.eulerAngles = new Vector3(0f, 0f, -10f);
			float duration = Duration / 4f;
			top.y = -1.78f;
			yield return Effects.Overlerp(duration, (float p) =>
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				((Component)Player).transform.localPosition = Vector2.op_Implicit(Vector2.LerpUnclamped(bottom, top, p));
			});
			float d3 = Duration / 2f;
			for (float t = 0f; t <= d3; t += Time.deltaTime)
			{
				float num3 = t / d3;
				Vector2 val = Vector2.Lerp(top, bottom, num3);
				val += Random.insideUnitCircle * 0.025f;
				((Component)Player).transform.localPosition = Vector2.op_Implicit(val);
				((Component)Player).transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(-10f, 17f, num3));
				yield return null;
			}
		}
	}
}
