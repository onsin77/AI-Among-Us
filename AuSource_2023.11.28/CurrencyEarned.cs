using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Innersloth.Assets;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyEarned : MonoBehaviour
{
	private static readonly float[] PotentialMultipliers = new float[6] { 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

	public Sprite BeansIcon;

	public SpriteRenderer Icon;

	public TextMeshPro AmountEarnedNowText;

	public TextMeshPro WalletCountText;

	public TextMeshPro NameText;

	public GameObject MultiplierContainer;

	public TextMeshPro[] MultiplierTexts;

	public SpriteAnim Animator;

	public AnimationClip ShowAmountAnimation;

	public AnimationClip EnterWalletAnimation;

	public AudioClip ShowAmountSound;

	public AudioClip EnterWalletSound;

	public AudioClip MultiplierLoopSound;

	public AudioClip MultiplierEndSound;

	private ProgressionManager.CurrencyGrantResult grantResult;

	public void Initialize(ProgressionManager.CurrencyGrantResult grantResult)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		this.grantResult = grantResult;
		((TMP_Text)WalletCountText).text = TextFormatter.Commas(grantResult.OldPodAmount);
		((TMP_Text)AmountEarnedNowText).text = "+" + TextFormatter.Commas(grantResult.GrantedPodsPreMultiplier);
		((Graphic)AmountEarnedNowText).color = Palette.ClearWhite;
		CosmicubeData dataForPodId = DestroyableSingleton<CosmicubeManager>.Instance.GetDataForPodId(grantResult.PodId);
		((TMP_Text)NameText).text = (Object.op_Implicit((Object)(object)dataForPodId) ? dataForPodId.GetPodName() : Constants.Beans);
		if (Object.op_Implicit((Object)(object)dataForPodId))
		{
			Icon.sprite = null;
			AddressableSpriteBehavior.AddToSpriteRenderer(Icon, new AddressableAsset<Sprite>(dataForPodId.podIconRef));
		}
		else
		{
			Icon.sprite = BeansIcon;
		}
		MultiplierContainer.SetActive(false);
	}

	public IEnumerator ShowAmount()
	{
		yield return new WaitForAnimationFinish(Animator, ShowAmountAnimation);
		SoundManager.Instance.PlaySoundImmediate(ShowAmountSound, loop: false);
		yield return Effects.Wait(0.25f);
	}

	public IEnumerator ShowMultiplier()
	{
		MultiplierContainer.SetActive(true);
		int desiredIndex = IntRange.Next(0, MultiplierTexts.Length);
		((TMP_Text)MultiplierTexts[desiredIndex]).text = TextFormatter.Multiplier(grantResult.Multiplier);
		List<float> list = new List<float>(PotentialMultipliers.Where((float m) => !Mathf.Approximately(m, grantResult.Multiplier)));
		list.Shuffle();
		for (int num = 0; num < MultiplierTexts.Length; num++)
		{
			MultiplierTexts[num].transform.localPosition = new Vector3(0f, (float)num * 0.4f, 0f);
			if (num != desiredIndex)
			{
				float num2 = list.Last();
				list.Remove(num2);
				((TMP_Text)MultiplierTexts[num]).text = TextFormatter.Multiplier(num2);
			}
		}
		SoundManager.Instance.PlaySound(MultiplierLoopSound, loop: true, 1f, SoundManager.Instance.SfxChannel);
		float timeToSpin = FloatRange.Next(1f, 2f);
		float t = 0f;
		while (t < timeToSpin)
		{
			t += Time.deltaTime;
			for (int num3 = 0; num3 < MultiplierTexts.Length; num3++)
			{
				Transform transform = MultiplierTexts[num3].transform;
				transform.localPosition -= new Vector3(0f, Time.deltaTime * 5f, 0f);
				if (transform.localPosition.y < -0.7f)
				{
					int num4 = num3 - 1;
					if (num4 < 0)
					{
						num4 = MultiplierTexts.Length - 1;
					}
					transform.localPosition = MultiplierTexts[num4].transform.localPosition + new Vector3(0f, 0.4f, 0f);
				}
			}
			if (Mathf.Abs(t - timeToSpin) < 0.3f && Mathf.Abs(MultiplierTexts[desiredIndex].transform.localPosition.x) < 0.1f)
			{
				break;
			}
			yield return null;
		}
		for (int num5 = 0; num5 < MultiplierTexts.Length; num5++)
		{
			if (num5 == desiredIndex)
			{
				MultiplierTexts[num5].transform.localPosition = Vector3.zero;
			}
			else
			{
				((Component)MultiplierTexts[num5]).gameObject.SetActive(false);
			}
		}
		SoundManager.Instance.StopSound(MultiplierLoopSound);
		SoundManager.Instance.PlaySoundImmediate(MultiplierEndSound, loop: false);
		yield return new WaitForAnimationFinish(Animator, ShowAmountAnimation);
		((TMP_Text)AmountEarnedNowText).text = "+" + TextFormatter.Commas(grantResult.GrantedPodsWithMultiplierApplied);
		yield return Effects.ColorFade(MultiplierTexts[desiredIndex], Color.white, Palette.ClearWhite, 0.4f);
	}

	public IEnumerator EnterWallet()
	{
		Animator.Play(EnterWalletAnimation);
		yield return Effects.Wait(0.4f);
		SoundManager.Instance.PlaySoundImmediate(EnterWalletSound, loop: false, 0.5f);
		((TMP_Text)WalletCountText).text = TextFormatter.Commas(grantResult.OldPodAmount + grantResult.GrantedPodsWithMultiplierApplied);
	}

	private void OnDisable()
	{
		SoundManager.Instance.StopSound(MultiplierLoopSound);
	}
}
