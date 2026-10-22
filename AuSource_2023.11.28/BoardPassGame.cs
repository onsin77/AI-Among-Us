using System.Collections;
using QRCoder;
using QRCoder.Unity;
using Rewired;
using Rewired.ControllerExtensions;
using TMPro;
using UnityEngine;

public class BoardPassGame : Minigame
{
	private static Color[] BgColors = new Color[5]
	{
		Color32.op_Implicit(new Color32((byte)101, (byte)170, (byte)119, byte.MaxValue)),
		Color32.op_Implicit(new Color32((byte)85, (byte)93, (byte)182, byte.MaxValue)),
		Color32.op_Implicit(new Color32((byte)198, (byte)127, (byte)174, byte.MaxValue)),
		Color32.op_Implicit(new Color32((byte)161, (byte)126, (byte)100, byte.MaxValue)),
		Color32.op_Implicit(new Color32((byte)149, (byte)219, (byte)209, byte.MaxValue))
	};

	public SpriteRenderer renderer;

	public SpriteRenderer pass;

	public Sprite passBack;

	public TextMeshPro NameText;

	public SpriteRenderer ImageBg;

	public SpriteRenderer Image;

	public Sprite[] Photos;

	public PassiveButton pullButton;

	public PassiveButton flipButton;

	public SpriteRenderer Scanner;

	public Sprite ScannerAccept;

	public Sprite ScannerScanning;

	public Sprite ScannerWaiting;

	public Collider2D Sensor;

	public Collider2D BarCode;

	public AudioClip slideinSound;

	public AudioClip flipSound;

	public AudioClip scanStartSound;

	public AudioClip scanSound;

	private Coroutine blinky;

	private Controller controller;

	private TouchpadBehavior touchpad;

	private bool prevHadInput;

	private float rotateAngle;

	private Vector2 prevStickDir;

	private bool enableControllerPassMovement;

	private bool grabbed;

	public void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Texture2D graphic = new UnityQRCode(new QRCodeGenerator().CreateQrCode("Yo holmes, smell you later", (ECCLevel)1, false, false, (EciMode)0, -1)).GetGraphic(1);
		renderer.sprite = Sprite.Create(graphic, new Rect(0f, 0f, (float)((Texture)graphic).width, (float)((Texture)graphic).height), Vector2.one / 2f);
		Image.sprite = Photos[PlayerControl.LocalPlayer.PlayerId % Photos.Length];
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)Image);
		((TMP_Text)NameText).text = PlayerControl.LocalPlayer.Data.PlayerName;
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
		controller = new Controller();
	}

	public void Update()
	{
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			((Controller)player.controllers.Joysticks[0]).GetExtension<IDualShock4Extension>();
			if (((Component)pullButton).gameObject.activeSelf)
			{
				if (player.GetAxisRaw(13) > 0.7f)
				{
					PullPass();
					((Component)pullButton).gameObject.SetActive(false);
				}
				else if (touchpad.IsTouching() && touchpad.GetTouchVector().x > 1f)
				{
					PullPass();
					((Component)pullButton).gameObject.SetActive(false);
				}
			}
			else if (((Component)flipButton).gameObject.activeSelf)
			{
				Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
				if (axis2DRaw.sqrMagnitude > 0.9f)
				{
					Vector2 normalized = axis2DRaw.normalized;
					if (prevHadInput)
					{
						float num = Vector2.SignedAngle(prevStickDir, normalized);
						if (num > 0f)
						{
							rotateAngle += num;
						}
						if (rotateAngle > 45f)
						{
							FlipPass();
							((Component)flipButton).gameObject.SetActive(false);
						}
					}
					prevStickDir = normalized;
					prevHadInput = true;
				}
				else if (touchpad.IsTouching())
				{
					if (touchpad.GetTouchVector().x < -1f)
					{
						FlipPass();
						((Component)flipButton).gameObject.SetActive(false);
					}
				}
				else
				{
					prevHadInput = false;
				}
			}
			else if (enableControllerPassMovement)
			{
				Vector3 localPosition = Vector2.op_Implicit(VirtualCursor.currentPosition) - ((Component)this).transform.position;
				localPosition.z = -1f;
				((Component)pass).transform.localPosition = localPosition;
			}
		}
		else if (grabbed)
		{
			Vector3 localPosition2 = Vector2.op_Implicit(DestroyableSingleton<PassiveButtonManager>.Instance.controller.DragPosition) - ((Component)this).transform.position;
			localPosition2.z = -1f;
			((Component)pass).transform.localPosition = localPosition2;
		}
		if (((Behaviour)flipButton).isActiveAndEnabled || ((Behaviour)pullButton).isActiveAndEnabled || MyNormTask.IsComplete)
		{
			return;
		}
		if (Sensor.IsTouching(BarCode))
		{
			if (blinky == null)
			{
				blinky = ((MonoBehaviour)this).StartCoroutine(CoRunBlinky());
			}
		}
		else if (blinky != null)
		{
			((MonoBehaviour)this).StopCoroutine(blinky);
			blinky = null;
			Scanner.sprite = ScannerWaiting;
		}
	}

	private IEnumerator CoRunBlinky()
	{
		int i = 0;
		while (i < 3)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(scanStartSound, loop: false);
			}
			Scanner.sprite = ScannerAccept;
			yield return Effects.Wait(0.1f);
			Scanner.sprite = ScannerScanning;
			yield return Effects.Wait(0.2f);
			int num = i + 1;
			i = num;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(scanSound, loop: false);
		}
		blinky = null;
		Scanner.sprite = ScannerAccept;
		MyNormTask.NextStep();
		yield return CoStartClose();
	}

	public void PullPass()
	{
		((MonoBehaviour)this).StartCoroutine(CoPullPass());
	}

	private IEnumerator CoPullPass()
	{
		((Component)pullButton).gameObject.SetActive(false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(slideinSound, loop: false);
		}
		yield return Effects.Slide2D(((Component)pass).transform, new Vector2(-10f, 0f), new Vector2(-1.4f, 0f), 0.3f);
		((Component)flipButton).gameObject.SetActive(true);
	}

	public void Grab()
	{
		if (!((Behaviour)flipButton).isActiveAndEnabled && !((Behaviour)pullButton).isActiveAndEnabled)
		{
			grabbed = !grabbed;
		}
	}

	public void FlipPass()
	{
		((MonoBehaviour)this).StartCoroutine(CoFlipPass());
	}

	private IEnumerator CoFlipPass()
	{
		((Component)flipButton).gameObject.SetActive(false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(flipSound, loop: false);
		}
		yield return Effects.Lerp(0.2f, (float t) =>
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			((Component)pass).transform.localEulerAngles = new Vector3(0f, Mathf.Lerp(0f, 90f, t), 0f);
		});
		pass.sprite = passBack;
		((Component)renderer).gameObject.SetActive(false);
		((Component)ImageBg).gameObject.SetActive(false);
		((Component)NameText).gameObject.SetActive(false);
		yield return Effects.Lerp(0.2f, (float t) =>
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			((Component)pass).transform.localEulerAngles = new Vector3(0f, Mathf.Lerp(90f, 180f, t), 0f);
		});
		enableControllerPassMovement = true;
		inputHandler.disableVirtualCursor = false;
		VirtualCursor.instance.SetWorldPosition(Vector2.op_Implicit(((Component)pass).transform.position));
	}

	static BoardPassGame()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
	}
}
