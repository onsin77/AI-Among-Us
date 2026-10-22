using TMPro;
using UnityEngine;

public class ReactorMinigame : Minigame
{
	private Color bad = new Color(1f, 41f / 255f, 0f);

	private Color good = new Color(77f / 255f, 226f / 255f, 213f / 255f);

	private ReactorSystemType reactor;

	public TextMeshPro statusText;

	public SpriteRenderer hand;

	private FloatRange YSweep = new FloatRange(-2.15f, 1.56f);

	public SpriteRenderer sweeper;

	public AudioClip HandSound;

	private bool isButtonDown;

	public override void Begin(PlayerTask task)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		reactor = ShipStatus.Instance.Systems[MyTask.StartAt] as ReactorSystemType;
		hand.color = bad;
		SetupInput(disableCursor: false);
	}

	public void ButtonDown()
	{
		if (!reactor.IsActive || amClosing != CloseState.None)
		{
			return;
		}
		isButtonDown = !isButtonDown;
		if (isButtonDown)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(HandSound, loop: true);
			}
			ShipStatus.Instance.RpcUpdateSystem(MyTask.StartAt, (byte)(0x40 | ConsoleId));
		}
		else
		{
			SoundManager.Instance.StopSound(HandSound);
			ShipStatus.Instance.RpcUpdateSystem(MyTask.StartAt, (byte)(0x20 | ConsoleId));
		}
		try
		{
			((SabotageTask)MyTask).MarkContributed();
		}
		catch
		{
		}
	}

	public void FixedUpdate()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		VirtualCursor.instance.SetScreenPosition(Vector2.zero);
		if (!reactor.IsActive)
		{
			if (amClosing == CloseState.None)
			{
				hand.color = good;
				((TMP_Text)statusText).text = DestroyableSingleton<TranslationController>.Instance.GetString((MyTask.StartAt == SystemTypes.Reactor) ? StringNames.ReactorNominal : StringNames.SeismicNominal);
				((Renderer)sweeper).enabled = false;
				SoundManager.Instance.StopSound(HandSound);
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
		}
		else if (!isButtonDown)
		{
			((TMP_Text)statusText).text = DestroyableSingleton<TranslationController>.Instance.GetString((MyTask.StartAt == SystemTypes.Reactor) ? StringNames.ReactorHoldToStop : StringNames.SeismicHoldToStop);
			((Renderer)sweeper).enabled = false;
		}
		else
		{
			((TMP_Text)statusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ReactorWaiting);
			Vector3 localPosition = ((Component)sweeper).transform.localPosition;
			localPosition.y = YSweep.Lerp(Mathf.Sin(Time.time) * 0.5f + 0.5f);
			((Component)sweeper).transform.localPosition = localPosition;
			((Renderer)sweeper).enabled = true;
		}
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(HandSound);
		if (Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			ShipStatus.Instance.RpcUpdateSystem(MyTask.StartAt, (byte)(0x20 | ConsoleId));
		}
		base.Close();
	}

	public ReactorMinigame()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
	}
}
