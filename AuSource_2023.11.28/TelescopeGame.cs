using System.Collections;
using Rewired;
using UnityEngine;

public class TelescopeGame : Minigame
{
	private bool grabbed;

	public Transform Background;

	public SpriteRenderer ItemDisplay;

	public BoxCollider2D[] Items;

	private BoxCollider2D TargetItem;

	public BoxCollider2D Reticle;

	public SpriteRenderer ReticleImage;

	private Coroutine blinky;

	public AudioClip BlipSound;

	public FloatRange BlipDelay = new FloatRange(0.01f, 1f);

	private TouchpadBehavior touchpad;

	private Vector3 initialPos;

	public void Start()
	{
		TargetItem = Items.Random();
		ItemDisplay.sprite = ((Component)TargetItem).GetComponent<SpriteRenderer>().sprite;
		((MonoBehaviour)this).StartCoroutine(RunBlipSound());
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	private IEnumerator RunBlipSound()
	{
		while (true)
		{
			for (float time = 0f; time < BlipDelay.Lerp(Vector2.Distance(Vector2.op_Implicit(((Component)TargetItem).transform.position), Vector2.op_Implicit(((Component)Reticle).transform.position)) / 10f); time += Time.deltaTime)
			{
				yield return null;
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(BlipSound, loop: false);
			}
			VibrationManager.Vibrate(0.3f, 0.3f, 0.01f, VibrationManager.VibrationFalloff.Linear);
		}
	}

	public void Update()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		NormalPlayerTask myNormTask = MyNormTask;
		if (myNormTask != null && myNormTask.IsComplete)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		Player player = ReInput.players.GetPlayer(0);
		Vector2 val = new Vector2(player.GetAxis(13), player.GetAxis(14));
		if (val.magnitude > 0.01f)
		{
			Vector2 val2 = -val * Time.deltaTime * 3.5f;
			zero = ((Component)Background).transform.localPosition;
			zero.x = Mathf.Clamp(zero.x + val2.x, -6f, 6f);
			zero.y = Mathf.Clamp(zero.y + val2.y, -7f, 7f);
			((Component)Background).transform.localPosition = zero;
		}
		else if (touchpad.IsTouching())
		{
			if (touchpad.IsFirstTouch())
			{
				initialPos = ((Component)Background).transform.localPosition;
			}
			Vector2 touchVector = touchpad.GetTouchVector();
			zero.x = Mathf.Clamp(initialPos.x + touchVector.x, -6f, 6f);
			zero.y = Mathf.Clamp(initialPos.y + touchVector.y, -7f, 7f);
			((Component)Background).transform.localPosition = zero;
		}
		if (grabbed)
		{
			Controller controller = DestroyableSingleton<PassiveButtonManager>.Instance.controller;
			Vector2 val3 = controller.DragPosition - controller.DragStartPosition;
			zero = ((Component)Background).transform.localPosition;
			zero.x = Mathf.Clamp(zero.x + val3.x, -6f, 6f);
			zero.y = Mathf.Clamp(zero.y + val3.y, -7f, 7f);
			((Component)Background).transform.localPosition = zero;
			controller.ResetDragPosition();
		}
		if (((Collider2D)Reticle).IsTouching((Collider2D)(object)TargetItem))
		{
			if (blinky == null)
			{
				blinky = ((MonoBehaviour)this).StartCoroutine(CoBlinky());
			}
		}
		else if (blinky != null)
		{
			((MonoBehaviour)this).StopCoroutine(blinky);
			blinky = null;
			ReticleImage.color = Color.white;
		}
	}

	private IEnumerator CoBlinky()
	{
		int i = 0;
		while (i < 3)
		{
			ReticleImage.color = Color.green;
			yield return Effects.Wait(0.1f);
			ReticleImage.color = Color.white;
			yield return Effects.Wait(0.2f);
			int num = i + 1;
			i = num;
		}
		blinky = null;
		ReticleImage.color = Color.green;
		MyNormTask.NextStep();
		yield return CoStartClose();
	}

	public void Grab()
	{
		grabbed = !grabbed;
	}
}
