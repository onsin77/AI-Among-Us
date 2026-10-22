using UnityEngine;

public class AirshipUploadGame : Minigame
{
	public SpriteRenderer Phone;

	public Collider2D Hotspot;

	public Collider2D Perfect;

	public Collider2D Good;

	public Collider2D Poor;

	public GameObject PerfectIcon;

	public GameObject GoodIcon;

	public GameObject PoorIcon;

	public GameObject NoneIcon;

	public HorizontalGauge gauge;

	public float moveSpeed = 1f;

	private const float MaxTimer = 20f;

	private float timer;

	public AudioClip nearSound;

	public float BeepPeriod = 0.5f;

	private float beepTimer;

	public Controller cont = new Controller();

	public SpriteRenderer promptGlyph;

	private Color glyphColor;

	private float glyphDisappearDelay = 5f;

	private bool phoneGrabbed;

	public void Start()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)Hotspot).transform;
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		transform.localPosition = Vector2.op_Implicit(insideUnitCircle.normalized * 2.5f);
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)Phone);
		SetupInput(disableCursor: false);
		glyphColor = promptGlyph.color;
	}

	public void Update()
	{
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		float num = 0f;
		if (Hotspot.IsTouching(Perfect))
		{
			if (!PerfectIcon.activeSelf)
			{
				DeactivateIcons();
				PerfectIcon.SetActive(true);
			}
			num = Time.deltaTime * 4f;
		}
		else if (Hotspot.IsTouching(Good))
		{
			if (!GoodIcon.activeSelf)
			{
				DeactivateIcons();
				GoodIcon.SetActive(true);
			}
			num = Time.deltaTime * 2f;
		}
		else if (Hotspot.IsTouching(Poor))
		{
			if (!PoorIcon.activeSelf)
			{
				DeactivateIcons();
				PoorIcon.SetActive(true);
			}
			num = Time.deltaTime;
		}
		else if (!NoneIcon.activeSelf)
		{
			DeactivateIcons();
			NoneIcon.SetActive(true);
		}
		cont.Update();
		if (glyphColor.a > 0f)
		{
			if (glyphDisappearDelay > 0f)
			{
				glyphDisappearDelay -= Time.deltaTime;
			}
			else
			{
				promptGlyph.color = glyphColor;
				glyphColor.a -= Time.deltaTime;
			}
		}
		else if (((Component)promptGlyph).gameObject.activeSelf)
		{
			((Component)promptGlyph).gameObject.SetActive(false);
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			((Component)Phone).transform.position = Vector2.op_Implicit(VirtualCursor.currentPosition);
		}
		timer += num;
		if (Constants.ShouldPlaySfx())
		{
			beepTimer += (10f - Vector2.Distance(Vector2.op_Implicit(((Component)Hotspot).transform.localPosition), Vector2.op_Implicit(((Component)Phone).transform.localPosition))) * num / BeepPeriod;
			if (beepTimer >= 1f)
			{
				beepTimer = 0f;
				SoundManager.Instance.PlaySoundImmediate(nearSound, loop: false);
			}
		}
		if (phoneGrabbed)
		{
			Vector3 position = ((Component)Phone).transform.position;
			float z = position.z;
			position = Vector2.op_Implicit(DestroyableSingleton<PassiveButtonManager>.Instance.controller.Touches[0].Position);
			position.z = z;
			((Component)Phone).transform.position = position;
		}
		gauge.Value = timer / 20f;
		if (timer >= 20f)
		{
			MyNormTask.NextStep();
			Close();
		}
	}

	public void ToggleGrab()
	{
		phoneGrabbed = !phoneGrabbed;
	}

	private void DeactivateIcons()
	{
		PerfectIcon.SetActive(false);
		GoodIcon.SetActive(false);
		PoorIcon.SetActive(false);
		NoneIcon.SetActive(false);
	}
}
