using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class FixShowerMinigame : Minigame
{
	private const float PowerRatio = 0.32f;

	private const float BasePower = 0.04f;

	private const float CompleteTolerance = 0.07f;

	private FloatRange hammerAngles = new FloatRange(0f, -45f);

	private FloatRange showerAngles = new FloatRange(-30f, 30f);

	private float showerPos;

	public SpriteRenderer mallet;

	public Collider2D showerHead;

	public AnimationCurve hammerAnim;

	private Controller controller = new Controller();

	private float powerTime;

	public PowerBar powerBar;

	public AudioClip[] bashSounds;

	public AudioClip swingSound;

	public GameObject leftGlyph;

	public GameObject rightGlyph;

	private bool prevButtonHeld;

	private bool animating;

	private float Power => Mathf.Sin(-(float)Math.PI / 2f + powerTime * 4f) / 2f + 0.5f;

	public void Start()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		showerPos = BitConverter.ToSingle(MyNormTask.Data, 0);
		((Component)showerHead).transform.localEulerAngles = new Vector3(0f, 0f, showerAngles.Lerp(showerPos));
		((Component)powerBar).gameObject.SetActive(false);
		((Renderer)mallet).enabled = false;
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		if (amClosing != CloseState.None || animating)
		{
			return;
		}
		controller.Update();
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			bool flag = showerPos < 0.5f;
			if (leftGlyph.activeSelf != flag)
			{
				leftGlyph.SetActive(flag);
			}
			if (rightGlyph.activeSelf == flag)
			{
				rightGlyph.SetActive(!flag);
			}
			bool button = player.GetButton(flag ? 24 : 21);
			if (button)
			{
				if (!prevButtonHeld)
				{
					((Component)powerBar).gameObject.SetActive(true);
					powerTime = 0f;
					powerBar.SetValue(0f);
				}
				else
				{
					powerTime += Time.deltaTime;
					powerBar.SetValue(Power);
				}
			}
			else if (prevButtonHeld)
			{
				((Component)powerBar).gameObject.SetActive(false);
				((MonoBehaviour)this).StartCoroutine(Bash(Power * 0.32f + 0.04f));
			}
			prevButtonHeld = button;
		}
		else
		{
			switch (controller.CheckDrag(showerHead))
			{
			case DragState.TouchStart:
				((Component)powerBar).gameObject.SetActive(true);
				powerTime = 0f;
				powerBar.SetValue(0f);
				break;
			case DragState.Holding:
			case DragState.Dragging:
				powerTime += Time.deltaTime;
				powerBar.SetValue(Power);
				break;
			case DragState.Released:
				((Component)powerBar).gameObject.SetActive(false);
				((MonoBehaviour)this).StartCoroutine(Bash(Power * 0.32f + 0.04f));
				break;
			}
		}
	}

	public IEnumerator Bash(float power)
	{
		animating = true;
		((Component)mallet).transform.localEulerAngles = Vector3.zero;
		if ((double)showerPos < 0.5)
		{
			mallet.flipX = false;
			((Component)mallet).transform.localPosition = new Vector3(-2.5f, -0.4f, 0f);
		}
		else
		{
			mallet.flipX = true;
			((Component)mallet).transform.localPosition = new Vector3(2.5f, -0.4f, 0f);
		}
		((Renderer)mallet).enabled = true;
		yield return Effects.Wait(0.05f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(swingSound, loop: false);
		}
		yield return Effects.Lerp(0.15f, (float t) =>
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			float num = hammerAngles.Lerp(hammerAnim.Evaluate(t));
			if (mallet.flipX)
			{
				num = 0f - num;
			}
			((Component)mallet).transform.localEulerAngles = new Vector3(0f, 0f, num);
		});
		if (showerPos < 0.5f)
		{
			VibrationManager.Vibrate(power, 0f, 0.3f, VibrationManager.VibrationFalloff.Linear);
		}
		else
		{
			VibrationManager.Vibrate(0f, power, 0.3f, VibrationManager.VibrationFalloff.Linear);
		}
		if ((double)showerPos > 0.5)
		{
			power = 0f - power;
		}
		showerPos += power;
		((Component)showerHead).transform.localEulerAngles = new Vector3(0f, 0f, showerAngles.Lerp(showerPos));
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(bashSounds.Random(), loop: false);
		}
		yield return Effects.Wait(0.05f);
		((Renderer)mallet).enabled = false;
		if (Mathf.Abs(showerPos - 0.5f) < 0.07f)
		{
			MyNormTask.NextStep();
			yield return CoStartClose();
		}
		animating = false;
	}

	public override void Close()
	{
		MyNormTask.Data = BitConverter.GetBytes(showerPos);
		base.Close();
	}
}
