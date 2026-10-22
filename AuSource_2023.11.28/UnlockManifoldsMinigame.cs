using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnlockManifoldsMinigame : Minigame
{
	public SpriteRenderer[] Buttons;

	public byte SystemId;

	private int buttonCounter;

	private bool animating;

	public AudioClip PressButtonSound;

	public AudioClip FailSound;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		int num = 2;
		int num2 = Buttons.Length / num;
		float[] array = FloatRange.SpreadToEdges(-1.7f, 1.7f, num2).ToArray();
		float[] array2 = FloatRange.SpreadToEdges(-0.43f, 0.43f, num).ToArray();
		SpriteRenderer[] array3 = Buttons.ToArray();
		array3.Shuffle();
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num3 = i + j * num2;
				((Component)array3[num3]).transform.localPosition = new Vector3(array[i], array2[j], 0f);
			}
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
	}

	public void HitButton(int idx)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (MyNormTask.IsComplete || animating)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(PressButtonSound, loop: false).pitch = Mathf.Lerp(0.5f, 1.5f, (float)idx / 10f);
		}
		if (idx == buttonCounter)
		{
			Buttons[idx].color = Color.green;
			buttonCounter++;
			if (buttonCounter == Buttons.Length)
			{
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
		}
		else
		{
			buttonCounter = 0;
			((MonoBehaviour)this).StartCoroutine(ResetAll());
		}
	}

	private IEnumerator ResetAll()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(FailSound, loop: false);
		}
		animating = true;
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].color = Color.red;
		}
		yield return (object)new WaitForSeconds(0.25f);
		for (int j = 0; j < Buttons.Length; j++)
		{
			Buttons[j].color = Color.white;
		}
		yield return (object)new WaitForSeconds(0.25f);
		for (int k = 0; k < Buttons.Length; k++)
		{
			Buttons[k].color = Color.red;
		}
		yield return (object)new WaitForSeconds(0.25f);
		for (int l = 0; l < Buttons.Length; l++)
		{
			Buttons[l].color = Color.white;
		}
		animating = false;
	}
}
