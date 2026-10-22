using System.Collections;
using UnityEngine;

public class SkeldExileController : ExileController
{
	protected override IEnumerator Animate()
	{
		float num = Camera.main.orthographicSize * Camera.main.aspect + 1f;
		Vector2 left = Vector2.left * num;
		Vector2 right = Vector2.right * num;
		((Component)Player).transform.localPosition = Vector2.op_Implicit(left);
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear);
		yield return (object)new WaitForSeconds(0.2f);
		if (exiled != null && Object.op_Implicit((Object)(object)EjectSound))
		{
			SoundManager.Instance.PlayDynamicSound("PlayerEjected", EjectSound, loop: true, SoundDynamics, SoundManager.Instance.SfxChannel);
		}
		yield return (object)new WaitForSeconds(0.8f);
		yield return Effects.All(PlayerSpin(left, right), HandleText(Duration * 0.3f, Duration * 0.5f));
		if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
		{
			((Component)ImpostorText).gameObject.SetActive(true);
		}
		yield return Effects.Bloop(0f, ImpostorText.transform);
		yield return (object)new WaitForSeconds(0.5f);
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);
		WrapUp();
	}

	private IEnumerator PlayerSpin(Vector2 left, Vector2 right)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		for (float t = 0f; t <= Duration; t += Time.deltaTime)
		{
			float num = t / Duration;
			((Component)Player).transform.localPosition = Vector2.op_Implicit(Vector2.Lerp(left, right, LerpCurve.Evaluate(num)));
			float num2 = (t + 0.75f) * 25f / Mathf.Exp(t * 0.75f + 1f);
			((Component)Player).transform.Rotate(new Vector3(0f, 0f, num2 * Time.deltaTime * 30f));
			yield return null;
		}
	}

	private void SoundDynamics(AudioSource source, float dt)
	{
		source.volume = 0.8f;
		source.loop = false;
		source.panStereo = -1f;
	}
}
