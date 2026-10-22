using System.Linq;
using Rewired;
using UnityEngine;

public class PolishRubyGame : Minigame
{
	public PassiveButton[] Buttons;

	public SpriteRenderer[] Sparkles;

	public int[] swipes;

	public Vector2[] directions;

	public int swipesToClean = 6;

	public AudioClip[] rubSounds;

	public AudioClip sparkleSound;

	public Transform cursorObject;

	public Transform handWipeObject;

	public SpriteRenderer[] handSprites;

	public BoxCollider2D rubyCursorBounds;

	private Controller cont = new Controller();

	private bool oldCursorOverlapsSmudge;

	public void Start()
	{
		swipes = new int[Buttons.Length];
		directions = new Vector2[Buttons.Length];
		Buttons.ForEach((PassiveButton b) =>
		{
			((Component)b).gameObject.SetActive(false);
		});
		int num = IntRange.Next(3, 5);
		for (int num2 = 0; num2 < num; num2++)
		{
			((Component)Buttons.Where((PassiveButton t) => !((Behaviour)t).isActiveAndEnabled).Random()).gameObject.SetActive(true);
		}
		SetupInput(disableCursor: true);
		SpriteRenderer[] array = handSprites;
		foreach (SpriteRenderer playerMaterialColors in array)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
		UpdateSpriteColor(cursorOverlapsSmudge: false);
	}

	private void UpdateSpriteColor(bool cursorOverlapsSmudge)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Color white = Color.white;
		white.a = (cursorOverlapsSmudge ? 1f : 0.5f);
		SpriteRenderer[] array = handSprites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = white;
		}
	}

	public void PlaySparkleSound()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(sparkleSound, loop: false);
		}
	}

	public void Update()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		cont.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			((Component)cursorObject).gameObject.SetActive(true);
			Player player = ReInput.players.GetPlayer(0);
			Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
			Vector2 axis2DRaw2 = player.GetAxis2DRaw(16, 17);
			Vector3 localPosition = cursorObject.localPosition;
			Vector3 localPosition2 = handWipeObject.localPosition;
			Vector2 val = rubyCursorBounds.size * 0.5f;
			localPosition.x += axis2DRaw.x * Time.deltaTime * 3f;
			localPosition.y += axis2DRaw.y * Time.deltaTime * 3f;
			localPosition.x = Mathf.Clamp(localPosition.x, 0f - val.x, val.x);
			localPosition.y = Mathf.Clamp(localPosition.y, 0f - val.y, val.y);
			localPosition2.x = axis2DRaw2.x * 0.35f;
			localPosition2.y = axis2DRaw2.y * 0.35f;
			((Component)cursorObject).transform.localPosition = localPosition;
			((Component)handWipeObject).transform.localPosition = localPosition2;
			Vector3 position = cursorObject.position;
			bool flag = false;
			for (int i = 0; i < Buttons.Length; i++)
			{
				PassiveButton passiveButton = Buttons[i];
				if (!((Behaviour)passiveButton).isActiveAndEnabled || !passiveButton.Colliders[0].OverlapPoint(Vector2.op_Implicit(position)))
				{
					continue;
				}
				if (!((Renderer)Sparkles[i]).enabled)
				{
					flag = true;
				}
				Vector2 val2 = directions[i];
				float y = val2.y;
				val2.y = localPosition2.x - val2.x;
				val2.x = localPosition2.x;
				directions[i] = val2;
				if (Mathf.Sign(y) == Mathf.Sign(val2.y) || val2.y == 0f)
				{
					continue;
				}
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySoundImmediate(rubSounds.Random(), loop: false);
				}
				VibrationManager.Vibrate(0.1f, 0.1f, 0.05f, VibrationManager.VibrationFalloff.Linear);
				int num = ++swipes[i];
				if (num <= swipesToClean)
				{
					((Component)passiveButton).GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Palette.ClearWhite, (float)num / (float)swipesToClean);
					if (num == swipesToClean)
					{
						((Renderer)Sparkles[i]).enabled = true;
					}
				}
			}
			if (oldCursorOverlapsSmudge != flag)
			{
				oldCursorOverlapsSmudge = flag;
				UpdateSpriteColor(flag);
			}
		}
		else
		{
			((Component)cursorObject).gameObject.SetActive(false);
			Controller controller = DestroyableSingleton<PassiveButtonManager>.Instance.controller;
			if (controller.AnyTouch)
			{
				Vector2 position2 = controller.Touches[0].Position;
				for (int j = 0; j < Buttons.Length; j++)
				{
					PassiveButton passiveButton2 = Buttons[j];
					if (!((Behaviour)passiveButton2).isActiveAndEnabled || !passiveButton2.Colliders[0].OverlapPoint(position2))
					{
						continue;
					}
					Vector2 val3 = directions[j];
					float y2 = val3.y;
					val3.y = position2.x - val3.x;
					val3.x = position2.x;
					directions[j] = val3;
					if (Mathf.Sign(y2) == Mathf.Sign(val3.y) || val3.y == 0f)
					{
						continue;
					}
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySoundImmediate(rubSounds.Random(), loop: false);
					}
					int num2 = ++swipes[j];
					if (num2 <= swipesToClean)
					{
						((Component)passiveButton2).GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Palette.ClearWhite, (float)num2 / (float)swipesToClean);
						if (num2 == swipesToClean)
						{
							((Renderer)Sparkles[j]).enabled = true;
						}
					}
				}
			}
		}
		bool flag2 = true;
		for (int k = 0; k < Buttons.Length; k++)
		{
			if (((Behaviour)Buttons[k]).isActiveAndEnabled && swipes[k] < swipesToClean)
			{
				flag2 = false;
				break;
			}
		}
		if (flag2)
		{
			MyNormTask.NextStep();
			Close();
		}
	}
}
