using System.Collections;
using TMPro;
using UnityEngine;

public class AirshipExileController : ExileController
{
	public SpriteRenderer ForegroundCloud;

	public SpriteRenderer BackgroundCloud;

	public SpriteRenderer Cloud1;

	public SpriteRenderer Cloud2;

	public SpriteRenderer Cloud3;

	public SpriteRenderer HandSlot;

	public AudioClip Stinger;

	private float CloudSlowMo = 1f;

	private float PlayerSlowMo = 1f;

	protected override IEnumerator Animate()
	{
		if (exiled != null)
		{
			PlayerMaterial.SetColors(exiled.DefaultOutfit.ColorId, (Renderer)(object)HandSlot);
		}
		float duration = 0.7f + Duration + 0.5f + 2f;
		((MonoBehaviour)this).StartCoroutine(Effects.All(SlowMoSlide2D(((Component)ForegroundCloud).transform, new Vector2(-1.4f, -2.41f), new Vector2(-0.4f, -2.41f), duration), SlowMoSlide2D(((Component)BackgroundCloud).transform, new Vector2(-0.97f, -1.043f), new Vector2(0.25f, -1.043f), duration), SlowMoSlide2D(((Component)Cloud1).transform, new Vector2(3f, 0.25f), new Vector2(6.5f, 0.25f), duration), SlowMoSlide2D(((Component)Cloud2).transform, new Vector2(-6f, 3f), new Vector2(5f, 3f), duration), SlowMoSlide2D(((Component)Cloud3).transform, new Vector2(-4f, -2.2f), new Vector2(4f, -2.2f), duration)));
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear);
		yield return Effects.Wait(0.5f);
		yield return Effects.All(PlayerFall(), HandleText(1.75f, Duration * 0.5f));
		yield return (object)new WaitForSeconds(0.5f);
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);
		yield return WrapUpAndSpawn();
	}

	private IEnumerator SlowMoSlide2D(Transform target, Vector2 source, Vector2 dest, float duration)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 temp = new Vector3
		{
			z = target.localPosition.z
		};
		for (float time = 0f; time < duration; time += Time.deltaTime * CloudSlowMo)
		{
			float num = time / duration;
			temp.x = Mathf.SmoothStep(source.x, dest.x, num);
			temp.y = Mathf.SmoothStep(source.y, dest.y, num);
			target.localPosition = temp;
			yield return null;
		}
		temp.x = dest.x;
		temp.y = dest.y;
		target.localPosition = temp;
	}

	protected IEnumerator WrapUpAndSpawn()
	{
		if (exiled != null)
		{
			PlayerControl playerControl = exiled.Object;
			if (Object.op_Implicit((Object)(object)playerControl))
			{
				playerControl.Exiled();
			}
			exiled.IsDead = true;
		}
		if (DestroyableSingleton<TutorialManager>.InstanceExists || !GameManager.Instance.LogicFlow.IsGameOverDueToDeath())
		{
			yield return ShipStatus.Instance.PrespawnStep();
			ReEnableGameplay();
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	protected override IEnumerator HandleText(float firstWaitTime, float animDuration)
	{
		yield return Effects.Wait(firstWaitTime);
		if (exiled != null)
		{
			CloudSlowMo = 0.1f;
			PlayerSlowMo = 0.1f;
			SoundManager.Instance.PlaySound(Stinger, loop: false);
		}
		float newDur = Duration * 0.5f;
		for (float t = 0f; t <= newDur; t += Time.deltaTime)
		{
			int num = (int)(t / newDur * (float)completeString.Length);
			if (num > ((TMP_Text)Text).text.Length)
			{
				((TMP_Text)Text).text = completeString.Substring(0, num);
				((Component)Text).gameObject.SetActive(true);
				if (completeString[num - 1] != ' ')
				{
					SoundManager.Instance.PlaySoundImmediate(TextSound, loop: false, 0.8f);
				}
			}
			yield return null;
		}
		((TMP_Text)Text).text = completeString;
		yield return Effects.Wait(1f);
		CloudSlowMo = 1f;
		PlayerSlowMo = 6f;
		yield return Effects.Wait(0.5f);
		if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
		{
			((Component)ImpostorText).gameObject.SetActive(true);
		}
		yield return Effects.Bloop(0f, ImpostorText.transform);
	}

	private IEnumerator PlayerFall()
	{
		Camera main = Camera.main;
		float num = main.orthographicSize + 1.5f;
		float num2 = main.orthographicSize * main.aspect;
		Vector2 sourcePos = Vector2.up * num + Vector2.right * num2 / 4f;
		Vector2 targetPos = Vector2.down * num + Vector2.left * num2 / 4f;
		Vector2 val = (targetPos - sourcePos) / 2f;
		Vector2 val2 = sourcePos + val;
		Vector2 val3 = val.Rotate(-90f);
		Vector2 anchor = val2 + val3.normalized * 0.5f;
		float d = Duration;
		for (float t = 0f; t <= d; t += Time.deltaTime * PlayerSlowMo)
		{
			float num3 = t / d;
			Vector2 val4 = Effects.Bezier(num3, sourcePos, targetPos, anchor);
			((Component)Player).transform.localPosition = Vector2.op_Implicit(val4);
			float num4 = Mathf.Lerp(0f, 80f, num3);
			((Component)Player).transform.localEulerAngles = new Vector3(0f, 0f, num4);
			yield return null;
		}
	}
}
