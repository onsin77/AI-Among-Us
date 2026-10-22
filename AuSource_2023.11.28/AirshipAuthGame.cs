using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AirshipAuthGame : Minigame
{
	public TextMeshPro TargetText;

	public TextMeshPro NumberText;

	public TextMeshPro OtherStatusText;

	public int number;

	public string numString = string.Empty;

	private bool animating;

	private HeliSabotageSystem system;

	public SpriteRenderer OurLight;

	public SpriteRenderer TheirLight;

	public SpriteRenderer TimeBar;

	public AudioClip ButtonSound;

	public AudioClip AcceptSound;

	public AudioClip RejectSound;

	private int OtherConsoleId;

	private bool evenColor;

	public UiElement defaultSelection;

	public UiElement closeButton;

	public List<UiElement> selectableButtons;

	public override void Begin(PlayerTask task)
	{
		OtherConsoleId = (ConsoleId + 1) % 2;
		base.Begin(task);
		system = ShipStatus.Instance.Systems[SystemTypes.HeliSabotage] as HeliSabotageSystem;
		ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, (byte)(0x40 | ConsoleId));
		ControllerManager.Instance.OpenOverlayMenu(((Object)((Component)this).gameObject).name, closeButton, defaultSelection, selectableButtons);
	}

	public override void Close()
	{
		base.Close();
		ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, (byte)(0x20 | ConsoleId));
		ControllerManager.Instance.CloseOverlayMenu(((Object)((Component)this).gameObject).name);
	}

	public void Update()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		evenColor = (int)Time.time * 2 % 2 == 0;
		Vector3 localScale = ((Component)TimeBar).transform.localScale;
		localScale.x = system.PercentActive;
		((Component)TimeBar).transform.localScale = localScale;
		if (system.PercentActive < 0.25f)
		{
			TimeBar.color = new Color(1f, 0.45f, 0.25f);
		}
		else if ((double)system.PercentActive < 0.5)
		{
			TimeBar.color = Color.yellow;
		}
		else
		{
			TimeBar.color = Color.white;
		}
		((TMP_Text)TargetText).text = system.TargetCode.ToString("D5");
		if (system.IsConsoleOkay(ConsoleId))
		{
			OurLight.color = Color.green;
		}
		else
		{
			OurLight.color = (evenColor ? Color.white : Color.yellow);
		}
		if (amClosing == CloseState.None && !system.IsActive)
		{
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
		if (system.IsConsoleOkay(OtherConsoleId))
		{
			TheirLight.color = Color.green;
			StringNames id = ((OtherConsoleId == 1) ? StringNames.AuthRightOkay : StringNames.AuthLeftOkay);
			((TMP_Text)OtherStatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(id);
		}
		else if (system.IsConsoleActive(OtherConsoleId))
		{
			TheirLight.color = (evenColor ? Color.white : Color.yellow);
			StringNames id2 = ((OtherConsoleId == 1) ? StringNames.AuthRightActive : StringNames.AuthLeftActive);
			((TMP_Text)OtherStatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(id2);
		}
		else
		{
			TheirLight.color = Color.red;
			StringNames id3 = ((OtherConsoleId == 1) ? StringNames.AuthRightNotActive : StringNames.AuthLeftNotActive);
			((TMP_Text)OtherStatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(id3);
		}
	}

	public void ClickNumber(int i)
	{
		if (!animating && ((TMP_Text)NumberText).text.Length < 5)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ButtonSound, loop: false);
			}
			numString += i;
			number = number * 10 + i;
			((TMP_Text)NumberText).text = numString;
		}
	}

	public void ClearEntry()
	{
		if (!animating)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ButtonSound, loop: false);
			}
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
		if (system.TargetCode == number)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(AcceptSound, loop: false);
			}
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, (byte)(0x10 | ConsoleId));
			try
			{
				((SabotageTask)MyTask).MarkContributed();
			}
			catch
			{
			}
			((TMP_Text)NumberText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK);
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return wait;
			((TMP_Text)NumberText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK);
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return wait;
			((TMP_Text)NumberText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK);
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(RejectSound, loop: false);
			}
			((TMP_Text)NumberText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bad);
			yield return wait;
			((TMP_Text)NumberText).text = string.Empty;
			yield return wait;
			((TMP_Text)NumberText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bad);
			yield return wait;
			numString = string.Empty;
			number = 0;
			((TMP_Text)NumberText).text = numString;
		}
		number = 0;
		numString = string.Empty;
		((TMP_Text)NumberText).text = string.Empty;
		animating = false;
	}
}
