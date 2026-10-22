using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeypadGame : Minigame
{
	public TextMeshPro TargetText;

	public TextMeshPro NumberText;

	public int number;

	public string numString = string.Empty;

	private bool animating;

	public SpriteRenderer AcceptButton;

	private LifeSuppSystemType system;

	private NoOxyTask oxyTask;

	private bool done;

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
		oxyTask = (NoOxyTask)task;
		((TMP_Text)TargetText).text = "today's code:\r\n" + oxyTask.targetNumber.ToString("D5");
		((TMP_Text)NumberText).text = string.Empty;
		system = (LifeSuppSystemType)ShipStatus.Instance.Systems[SystemTypes.LifeSupp];
		done = system.GetConsoleComplete(ConsoleId);
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
	}

	public void ClickNumber(int i)
	{
		if (!animating && !done)
		{
			if (((TMP_Text)NumberText).text.Length >= 5)
			{
				((MonoBehaviour)this).StartCoroutine(BlinkAccept());
				return;
			}
			numString += i;
			number = number * 10 + i;
			((TMP_Text)NumberText).text = numString;
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

	public void ClearEntry()
	{
		if (!animating)
		{
			number = 0;
			numString = string.Empty;
			((TMP_Text)NumberText).text = string.Empty;
		}
	}

	public void Enter()
	{
		if (!animating)
		{
			((MonoBehaviour)this).StartCoroutine(Animate());
		}
	}

	private IEnumerator Animate()
	{
		animating = true;
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		yield return wait;
		((TMP_Text)NumberText).text = string.Empty;
		yield return wait;
		if (oxyTask.targetNumber == number)
		{
			done = true;
			byte amount = (byte)(ConsoleId | 0x40);
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, amount);
			try
			{
				((SabotageTask)MyTask).MarkContributed();
			}
			catch
			{
			}
			string okStr = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK);
			((TMP_Text)NumberText).text = okStr;
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return wait;
			((TMP_Text)NumberText).text = okStr;
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return wait;
			((TMP_Text)NumberText).text = okStr;
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
		else
		{
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
