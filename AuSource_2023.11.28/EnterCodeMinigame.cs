using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnterCodeMinigame : Minigame
{
	public TextMeshPro NumberText;

	public TextMeshPro TargetText;

	public SpriteRenderer Card;

	public int number;

	public string numString = string.Empty;

	private bool animating;

	private bool cardOut;

	private bool done;

	private int targetNumber;

	public AudioClip WalletOut;

	public AudioClip NumberSound;

	public AudioClip AcceptSound;

	public AudioClip RejectSound;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void ShowCard()
	{
		((MonoBehaviour)this).StartCoroutine(CoShowCard());
	}

	private IEnumerator CoShowCard()
	{
		if (cardOut)
		{
			yield break;
		}
		cardOut = true;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(WalletOut, loop: false);
		}
		Vector3 pos = ((Component)Card).transform.localPosition;
		Vector3 targ = new Vector3(pos.x, 0.84f, pos.z);
		float time = 0f;
		while (true)
		{
			float num = Mathf.Min(1f, time / 0.6f);
			((Component)Card).transform.localPosition = Vector3.Lerp(pos, targ, num);
			((Component)Card).transform.localScale = Vector3.Lerp(Vector3.one * 0.75f, Vector3.one, num);
			if (!(time > 0.6f))
			{
				yield return null;
				time += Time.deltaTime;
				continue;
			}
			break;
		}
	}

	public void EnterDigit(int i)
	{
		if (animating || done)
		{
			return;
		}
		if (((TMP_Text)NumberText).text.Length >= 5)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(RejectSound, loop: false);
			}
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(NumberSound, loop: false).pitch = Mathf.Lerp(0.8f, 1.2f, (float)i / 9f);
		}
		numString += i;
		number = number * 10 + i;
		((TMP_Text)NumberText).text = numString;
	}

	public void ClearDigits()
	{
		if (!animating)
		{
			number = 0;
			numString = string.Empty;
			((TMP_Text)NumberText).text = string.Empty;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(NumberSound, loop: false);
			}
		}
	}

	public void AcceptDigits()
	{
		if (!animating)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(NumberSound, loop: false);
			}
			((MonoBehaviour)this).StartCoroutine(Animate());
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		targetNumber = BitConverter.ToInt32(MyNormTask.Data, 0);
		((TMP_Text)NumberText).text = string.Empty;
		((TMP_Text)TargetText).text = targetNumber.ToString("D5");
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
	}

	private IEnumerator Animate()
	{
		animating = true;
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		yield return wait;
		((TMP_Text)NumberText).text = string.Empty;
		yield return wait;
		if (targetNumber == number)
		{
			done = true;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(AcceptSound, loop: false);
			}
			MyNormTask.NextStep();
			string okStr = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK);
			((TMP_Text)NumberText).text = okStr;
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return wait;
			((TMP_Text)NumberText).text = okStr;
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return CoStartClose(0.5f);
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(RejectSound, loop: false);
			}
			string okStr = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bad);
			((TMP_Text)NumberText).text = okStr;
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return wait;
			((TMP_Text)NumberText).text = okStr;
			yield return wait;
			numString = string.Empty;
			number = 0;
			((TMP_Text)NumberText).text = numString;
		}
		animating = false;
	}
}
