using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class EmergencyMinigame : Minigame
{
	public SpriteRenderer ClosedLid;

	public SpriteRenderer OpenLid;

	public Transform meetingButton;

	public TextMeshPro StatusText;

	public TextMeshPro NumberText;

	public bool ButtonActive = true;

	public AudioClip ButtonSound;

	private int state;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public const int MinEmergencyTime = 15;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		Update();
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected);
	}

	public void Update()
	{
		if (ShipStatus.Instance.Timer < 15f || ShipStatus.Instance.EmergencyCooldown > 0f)
		{
			int num = Mathf.CeilToInt(15f - ShipStatus.Instance.Timer);
			num = Mathf.Max(Mathf.CeilToInt(ShipStatus.Instance.EmergencyCooldown), num);
			ButtonActive = false;
			((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyNotReady);
			((TMP_Text)NumberText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecondsAbbv, num);
			((Component)ClosedLid).gameObject.SetActive(true);
			((Component)OpenLid).gameObject.SetActive(false);
		}
		else if (!PlayerControl.LocalPlayer.myTasks.Any(PlayerTask.TaskIsEmergency))
		{
			if (state != 1)
			{
				state = 1;
				int remainingEmergencies = PlayerControl.LocalPlayer.RemainingEmergencies;
				((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyCount, PlayerControl.LocalPlayer.Data.PlayerName);
				((TMP_Text)NumberText).text = remainingEmergencies.ToString();
				ButtonActive = remainingEmergencies > 0;
				((Component)ClosedLid).gameObject.SetActive(!ButtonActive);
				((Component)OpenLid).gameObject.SetActive(ButtonActive);
			}
		}
		else if (state != 2)
		{
			state = 2;
			ButtonActive = false;
			((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyDuringCrisis);
			((TMP_Text)NumberText).text = string.Empty;
			((Component)ClosedLid).gameObject.SetActive(true);
			((Component)OpenLid).gameObject.SetActive(false);
		}
	}

	public void CallMeeting()
	{
		if (!PlayerControl.LocalPlayer.myTasks.Any(PlayerTask.TaskIsEmergency) && PlayerControl.LocalPlayer.RemainingEmergencies > 0 && ButtonActive)
		{
			((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyRequested);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ButtonSound, loop: false);
			}
			PlayerControl.LocalPlayer.CmdReportDeadBody(null);
			ButtonActive = false;
			VibrationManager.Vibrate(1f, 1f, 0.2f);
		}
	}

	private float easeOutElastic(float t)
	{
		float num = 0.3f;
		return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - num / 4f) * ((float)Math.PI * 2f) / num) + 1f;
	}

	protected override IEnumerator CoAnimateOpen()
	{
		for (float timer = 0f; timer < 0.2f; timer += Time.deltaTime)
		{
			float num = timer / 0.2f;
			((Component)this).transform.localPosition = new Vector3(0f, Mathf.SmoothStep(-8f, 0f, num), -50f);
			yield return null;
		}
		((Component)this).transform.localPosition = new Vector3(0f, 0f, -50f);
		Vector3 meetingPos = meetingButton.localPosition;
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num2 = timer / 0.1f;
			meetingPos.y = Mathf.Sin((float)Math.PI * num2) * 1f / (num2 * 5f + 4f) - 0.882f;
			meetingButton.localPosition = meetingPos;
			yield return null;
		}
		meetingPos.y = -0.882f;
		meetingButton.localPosition = meetingPos;
	}
}
