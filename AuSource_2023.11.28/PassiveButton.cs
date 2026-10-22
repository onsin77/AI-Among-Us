using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PassiveButton : PassiveUiElement
{
	public ButtonClickedEvent OnClick = new ButtonClickedEvent();

	public AudioClip ClickSound;

	public AudioClip HoverSound;

	public bool OnUp = true;

	public bool OnDown;

	public bool OnRepeat;

	public float RepeatDuration = 0.3f;

	[Header("Hold to Use")]
	public SpriteRenderer HeldButtonSprite;

	public ActionButton TargetActionButton;

	public bool HoldToUse;

	private bool beingHeldDown;

	private float repeatTimer;

	private float totalHeldTime;

	private bool checkedClickEvent;

	[SerializeField]
	[Header("States")]
	private GameObject activeSprites;

	[SerializeField]
	private GameObject inactiveSprites;

	[SerializeField]
	private GameObject disabledSprites;

	[Space(10f)]
	[SerializeField]
	private TextMeshPro buttonText;

	[SerializeField]
	private Color activeTextColor;

	[SerializeField]
	private Color inactiveTextColor;

	[SerializeField]
	private Color disabledTextColor;

	public override bool HandleUp => OnUp;

	public override bool HandleDown => OnDown;

	public override bool HandleRepeat => OnRepeat;

	protected override void Start()
	{
		base.Start();
		SetPassiveButtonHoverStateInactive();
	}

	protected override void Update()
	{
		base.Update();
		if (HoldToUse && beingHeldDown)
		{
			if (totalHeldTime > 0f)
			{
				float num = Mathf.Clamp01(totalHeldTime / 0.5f);
				HeldButtonSprite.SetCooldownNormalizedUvs();
				((Renderer)HeldButtonSprite).material.SetFloat("_Percent", 1f - num);
			}
			else
			{
				beingHeldDown = false;
				((Renderer)HeldButtonSprite).material.SetFloat("_Percent", 0f);
			}
		}
	}

	private new void OnDisable()
	{
		SetPassiveButtonHoverStateInactive();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (checkedClickEvent)
		{
			return;
		}
		checkedClickEvent = true;
		int persistentEventCount = ((UnityEventBase)OnClick).GetPersistentEventCount();
		CloseButtonConsoleBehaviour component = ((Component)this).GetComponent<CloseButtonConsoleBehaviour>();
		string text = ((Object)((Component)this).gameObject).name.ToLower();
		if (Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		for (int i = 0; i < persistentEventCount; i++)
		{
			string persistentMethodName = ((UnityEventBase)OnClick).GetPersistentMethodName(i);
			if (text.Contains("close") && persistentMethodName.ToLower().Contains("close"))
			{
				((Component)this).gameObject.AddComponent<CloseButtonConsoleBehaviour>();
				break;
			}
		}
	}

	public override void ReceiveClickDown()
	{
		if (((Behaviour)this).enabled && !HoldToUse)
		{
			if (Object.op_Implicit((Object)(object)ClickSound))
			{
				SoundManager.Instance.PlaySound(ClickSound, loop: false);
			}
			((UnityEvent)OnClick).Invoke();
		}
	}

	public override void ReceiveRepeatDown()
	{
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		if (HoldToUse)
		{
			if ((Object)(object)TargetActionButton != (Object)null && !TargetActionButton.CanInteract())
			{
				return;
			}
			totalHeldTime += Time.deltaTime;
			beingHeldDown = true;
			if (totalHeldTime < 0.5f)
			{
				return;
			}
		}
		else
		{
			repeatTimer += Time.deltaTime;
			if (repeatTimer < RepeatDuration)
			{
				return;
			}
			repeatTimer = 0f;
		}
		if (Object.op_Implicit((Object)(object)ClickSound))
		{
			SoundManager.Instance.PlaySound(ClickSound, loop: false);
		}
		((UnityEvent)OnClick).Invoke();
	}

	public override void ReceiveClickUp()
	{
		if (((Behaviour)this).enabled)
		{
			ReceiveClickDown();
		}
	}

	public void SetButtonEnableState(bool enabled)
	{
		((Behaviour)this).enabled = enabled;
		if (enabled)
		{
			SetPassiveButtonHoverStateInactive();
			((Component)this).GetComponent<ButtonRolloverHandler>()?.SetEnabledColors();
		}
		else
		{
			SetPassiveButtonDisabledState();
			((Component)this).GetComponent<ButtonRolloverHandler>()?.SetDisabledColors();
		}
		((Component)this).GetComponentInChildren<ActionMapGlyphDisplay>(true)?.SetSpriteVisibility(((Behaviour)this).enabled);
	}

	public void AddOnClickListeners(params Action[] callbacks)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected Obj, but got Unknown
		((UnityEventBase)OnClick).RemoveAllListeners();
		foreach (Action action in callbacks)
		{
			Action cbCopy = action;
			((UnityEvent)OnClick).AddListener((UnityAction)(() =>
			{
				cbCopy();
			}));
		}
	}

	public override void ReleaseButton()
	{
		totalHeldTime = 0f;
	}

	public override void ReceiveMouseOut()
	{
		if (((Behaviour)this).enabled)
		{
			base.ReceiveMouseOut();
			SetPassiveButtonHoverStateInactive();
		}
	}

	public override void ReceiveMouseOver()
	{
		if (((Behaviour)this).enabled)
		{
			base.ReceiveMouseOver();
			SetPassiveButtonHoverStateActive();
			if (Object.op_Implicit((Object)(object)HoverSound))
			{
				SoundManager.Instance.PlaySound(HoverSound, loop: false);
			}
		}
	}

	private void SetPassiveButtonHoverStateActive()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			if ((Object)(object)activeSprites != (Object)null)
			{
				activeSprites.SetActive(true);
			}
			if ((Object)(object)inactiveSprites != (Object)null)
			{
				inactiveSprites.SetActive(false);
			}
			if ((Object)(object)disabledSprites != (Object)null)
			{
				disabledSprites.SetActive(false);
			}
			if ((Object)(object)buttonText != (Object)null)
			{
				((Graphic)buttonText).color = activeTextColor;
			}
		}
	}

	private void SetPassiveButtonHoverStateInactive()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			if ((Object)(object)activeSprites != (Object)null)
			{
				activeSprites.SetActive(false);
			}
			if ((Object)(object)inactiveSprites != (Object)null)
			{
				inactiveSprites.SetActive(true);
			}
			if ((Object)(object)disabledSprites != (Object)null)
			{
				disabledSprites.SetActive(false);
			}
			if ((Object)(object)buttonText != (Object)null)
			{
				((Graphic)buttonText).color = inactiveTextColor;
			}
		}
	}

	private void SetPassiveButtonDisabledState()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)activeSprites != (Object)null)
		{
			activeSprites.SetActive(false);
		}
		if ((Object)(object)inactiveSprites != (Object)null)
		{
			inactiveSprites.SetActive(false);
		}
		if ((Object)(object)disabledSprites != (Object)null)
		{
			disabledSprites.SetActive(true);
		}
		if ((Object)(object)buttonText != (Object)null)
		{
			((Graphic)buttonText).color = disabledTextColor;
		}
	}

	public PassiveButton()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
	}
}
