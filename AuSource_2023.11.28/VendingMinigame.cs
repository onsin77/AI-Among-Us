using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VendingMinigame : Minigame
{
	public static readonly string[] Letters = new string[3] { "a", "b", "c" };

	public TextMeshPro NumberText;

	public SpriteRenderer TargetImage;

	public string enteredCode = string.Empty;

	private bool animating;

	private bool done;

	private string targetCode;

	public SpriteRenderer AcceptButton;

	public VendingSlot[] Slots;

	public Sprite[] Drinks;

	public Sprite[] DrawnDrinks;

	public AudioClip Ambience;

	public AudioClip Button;

	public AudioClip Error;

	public AudioClip SliderOpen;

	public AudioClip DrinkShake;

	public AudioClip DrinkLand;

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
		base.Begin(task);
		int num = Drinks.RandomIdx();
		TargetImage.sprite = DrawnDrinks[num];
		for (int i = 0; i < Drinks.Length; i++)
		{
			Sprite val = Drinks[i];
			int slotId;
			while (!PickARandomSlot(val, out slotId))
			{
			}
			((Renderer)Slots[slotId].DrinkImage).enabled = true;
			Slots[slotId].DrinkImage.sprite = val;
			if (num == i)
			{
				targetCode = SlotIdToString(slotId);
			}
		}
		((TMP_Text)NumberText).text = string.Empty;
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
	}

	private static int StringToSlotId(string code)
	{
		if (int.TryParse(code[0].ToString(), out var _) || Letters.Any(code.EndsWith))
		{
			return -1;
		}
		int num = Letters.IndexOf(code.StartsWith);
		return int.Parse(code[1].ToString()) - 1 + num * 4;
	}

	private static string SlotIdToString(int slotId)
	{
		int num = slotId % 4 + 1;
		int num2 = slotId / 4;
		return Letters[num2] + num;
	}

	private bool PickARandomSlot(Sprite drink, out int slotId)
	{
		slotId = Slots.RandomIdx();
		return !((Renderer)Slots[slotId].DrinkImage).enabled;
	}

	public void EnterDigit(string s)
	{
		if (animating || done)
		{
			return;
		}
		if (enteredCode.Length >= 2)
		{
			((MonoBehaviour)this).StartCoroutine(BlinkAccept());
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(Button, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		enteredCode += s;
		((TMP_Text)NumberText).text = enteredCode;
	}

	public void ClearDigits()
	{
		if (!animating)
		{
			enteredCode = string.Empty;
			((TMP_Text)NumberText).text = string.Empty;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(Button, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
			}
		}
	}

	public void AcceptDigits()
	{
		if (!animating && enteredCode.Length == 2)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(Button, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
			}
			((MonoBehaviour)this).StartCoroutine(Animate());
		}
	}

	private IEnumerator BlinkAccept()
	{
		int i = 0;
		while (i < 5)
		{
			AcceptButton.color = Color.gray;
			yield return null;
			yield return null;
			AcceptButton.color = Color.white;
			yield return null;
			yield return null;
			int num = i + 1;
			i = num;
		}
	}

	private IEnumerator Animate()
	{
		animating = true;
		int slotId = StringToSlotId(enteredCode);
		if (slotId >= 0 && ((Renderer)Slots[slotId].DrinkImage).enabled)
		{
			yield return Effects.All(CoBlinkVend(), Slots[slotId].CoBuy(SliderOpen, DrinkShake, DrinkLand));
			if (targetCode == enteredCode)
			{
				done = true;
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
			yield return Slots[slotId].CloseSlider(SliderOpen);
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(Error, loop: false);
			}
			WaitForSeconds wait = new WaitForSeconds(0.1f);
			((TMP_Text)NumberText).text = "XXXXXXXX";
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return wait;
			((TMP_Text)NumberText).text = "XXXXXXXX";
			yield return wait;
		}
		enteredCode = string.Empty;
		((TMP_Text)NumberText).text = enteredCode;
		animating = false;
	}

	private IEnumerator CoBlinkVend()
	{
		int i = 0;
		while (i < 5)
		{
			((TMP_Text)NumberText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Vending);
			yield return Effects.Wait(0.1f);
			((TMP_Text)NumberText).text = string.Empty;
			yield return Effects.Wait(0.1f);
			int num = i + 1;
			i = num;
		}
	}
}
