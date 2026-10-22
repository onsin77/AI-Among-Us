using Rewired;
using TMPro;
using UnityEngine;

public class WeatherSwitchGame : Minigame
{
	public static StringNames[] ControlNames = new StringNames[6]
	{
		StringNames.NodeCA,
		StringNames.NodeTB,
		StringNames.NodeIRO,
		StringNames.NodePD,
		StringNames.NodeGI,
		StringNames.NodeMLG
	};

	public WeatherControl[] Controls;

	private WeatherNodeTask WeatherTask;

	public Transform buttonGlyph;

	public Vector3 buttonGlyphOffset_Off;

	public Vector3 buttonGlyphOffset_On;

	public AudioClip SwitchSound;

	public void Start()
	{
		for (int i = 0; i < Controls.Length; i++)
		{
			WeatherControl obj = Controls[i];
			((Object)obj).name = DestroyableSingleton<TranslationController>.Instance.GetString(ControlNames[i]);
			((TMP_Text)obj.Label).text = DestroyableSingleton<TranslationController>.Instance.GetString(ControlNames[i]);
		}
	}

	public override void Begin(PlayerTask task)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		WeatherTask = MyNormTask as WeatherNodeTask;
		Controls[WeatherTask.NodeId].SetInactive();
		SetupInput(disableCursor: true);
		Vector3 localPosition = ((Component)Controls[WeatherTask.NodeId]).transform.localPosition + buttonGlyphOffset_On;
		localPosition.z = ((Component)buttonGlyph).transform.localPosition.z;
		((Component)buttonGlyph).transform.localPosition = localPosition;
	}

	private void Update()
	{
		if (ReInput.players.GetPlayer(0).GetButtonDown(20))
		{
			FlipSwitch(WeatherTask.NodeId);
		}
	}

	public void FlipSwitch(int i)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing == CloseState.None && i == WeatherTask.NodeId)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(SwitchSound, loop: false);
			}
			WeatherTask.NextStep();
			Controls[WeatherTask.NodeId].SetActive();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
			Vector3 localPosition = ((Component)Controls[WeatherTask.NodeId]).transform.localPosition + buttonGlyphOffset_Off;
			localPosition.z = ((Component)buttonGlyph).transform.localPosition.z;
			((Component)buttonGlyph).transform.localPosition = localPosition;
		}
	}
}
