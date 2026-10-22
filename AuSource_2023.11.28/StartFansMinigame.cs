using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class StartFansMinigame : Minigame
{
	public TextMeshPro ActionText;

	public SpriteRenderer[] CodeIcons;

	public Sprite[] IconSprites;

	public AudioClip revealSound;

	public AudioClip cycleSound;

	public AudioClip completeSound;

	public PassiveButton mainCodeButton;

	public PassiveButton closeButton;

	public List<UiElement> codeButtons;

	public ControllerButtonBehavior enterCodeHotkey;

	public override void Begin(PlayerTask task)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected Obj, but got Unknown
		base.Begin(task);
		if (ConsoleId == 0)
		{
			((TMP_Text)ActionText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RevealCode);
		}
		else
		{
			((TMP_Text)ActionText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterCode);
		}
		byte[] data = MyNormTask.Data;
		for (int i = 0; i < CodeIcons.Length; i++)
		{
			SpriteRenderer target = CodeIcons[i];
			((Component)((Component)CodeIcons[i]).transform.parent).gameObject.SetActive(false);
			if (ConsoleId == 0)
			{
				target.sprite = IconSprites[data[i]];
				continue;
			}
			((UnityEvent)((Component)CodeIcons[i]).GetComponent<PassiveButton>().OnClick).AddListener((UnityAction)(() =>
			{
				RotateImage(target);
			}));
		}
		List<UiElement> list = new List<UiElement>(1);
		list.Add(mainCodeButton);
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, closeButton, mainCodeButton, list);
	}

	public override void Close()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		base.Close();
	}

	public void RevealCode()
	{
		for (int i = 0; i < CodeIcons.Length; i++)
		{
			((Component)((Component)CodeIcons[i]).transform.parent).gameObject.SetActive(true);
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(revealSound, loop: false);
		}
		if (MyNormTask.taskStep == 0)
		{
			MyNormTask.NextStep();
		}
		if (ConsoleId != 0)
		{
			ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
			ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, closeButton, codeButtons[0], codeButtons);
		}
	}

	public void RotateImage(SpriteRenderer target)
	{
		if (ConsoleId == 0 || amClosing != CloseState.None)
		{
			return;
		}
		int num = IconSprites.IndexOf(target.sprite);
		num = (num + 1) % IconSprites.Length;
		target.sprite = IconSprites[num];
		for (int i = 0; i < CodeIcons.Length; i++)
		{
			SpriteRenderer val = CodeIcons[i];
			int num2 = IconSprites.IndexOf(val.sprite);
			if (MyNormTask.Data[i] != num2)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(cycleSound, loop: false);
				}
				return;
			}
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(completeSound, loop: false);
		}
		MyNormTask.NextStep();
		((MonoBehaviour)this).StartCoroutine(CoStartClose());
	}
}
