using System.Linq;
using PowerTools;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrillMinigame : Minigame
{
	public SpriteRenderer CaseImage;

	public TextMeshPro statusText;

	public SpriteAnim[] Buttons;

	public AnimationClip BadAnim;

	public AudioClip ButtonSound;

	private int MaxState = 4;

	private int[] states = new int[4];

	private SpriteAnim prevFixedButton;

	private float changeButtonDelay;

	private int[] drillButtonMaps = new int[4] { 20, 22, 21, 24 };

	public void Start()
	{
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)CaseImage);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		SetupInput(disableCursor: true);
		do
		{
			for (int i = 0; i < states.Length; i++)
			{
				int num = (states[i] = 0);
				((Component)Buttons[i]).transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.5f, (float)num / (float)MaxState);
				((Component)Buttons[i]).gameObject.SetActive(num != MaxState);
			}
		}
		while (states.All((int s) => s == MaxState));
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		if ((Object)(object)prevFixedButton != (Object)null && changeButtonDelay >= 0f)
		{
			changeButtonDelay -= Time.deltaTime;
			if (changeButtonDelay <= 0f)
			{
				prevFixedButton = null;
			}
		}
		for (int i = 0; i < 4; i++)
		{
			if (player.GetButtonDown(drillButtonMaps[i]) && ((Object)(object)prevFixedButton == (Object)null || (Object)(object)prevFixedButton == (Object)(object)Buttons[i]))
			{
				prevFixedButton = Buttons[i];
				changeButtonDelay = 0.25f;
				FixButton(Buttons[i]);
			}
		}
	}

	public void FixButton(SpriteAnim button)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		int num = Buttons.IndexOf(button);
		if (states[num] != MaxState)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ButtonSound, loop: false);
			}
			int num2 = ++states[num];
			((Component)Buttons[num]).transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.5f, (float)num2 / (float)MaxState);
			((Component)Buttons[num]).gameObject.SetActive(num2 != MaxState);
			if (num2 == MaxState)
			{
				changeButtonDelay = 0f;
			}
			if (states.All((int ss) => ss == MaxState))
			{
				((TMP_Text)statusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Fine);
				((Graphic)statusText).color = Color.green;
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
		}
	}
}
