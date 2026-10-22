using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControllerHeldButtonBehaviour : MonoBehaviour
{
	public RewiredConstsEnum.Action Action;

	public SpriteRenderer targetCooldownSprite;

	public float holdDuration;

	public ActionButton TargetActionButton;

	private Player player;

	private ButtonBehavior buttonBehavior;

	private PassiveButton passiveButton;

	private float holdTimer;

	private bool alreadyPressed;

	private void Start()
	{
		player = ReInput.players.GetPlayer(0);
		buttonBehavior = ((Component)this).GetComponent<ButtonBehavior>();
		passiveButton = ((Component)this).GetComponent<PassiveButton>();
	}

	private void Update()
	{
		if (player.GetButton((int)Action))
		{
			if (alreadyPressed || ((Object)(object)TargetActionButton != (Object)null && !TargetActionButton.CanInteract()))
			{
				return;
			}
			holdTimer += Time.unscaledDeltaTime;
			if (Object.op_Implicit((Object)(object)targetCooldownSprite))
			{
				float num = Mathf.Clamp01(holdTimer / 0.5f);
				targetCooldownSprite.SetCooldownNormalizedUvs();
				((Renderer)targetCooldownSprite).material.SetFloat("_Percent", 1f - num);
			}
			if (!(holdTimer >= holdDuration))
			{
				return;
			}
			alreadyPressed = true;
			if (Object.op_Implicit((Object)(object)buttonBehavior))
			{
				ButtonClickedEvent onClick = buttonBehavior.OnClick;
				if (onClick != null)
				{
					((UnityEvent)onClick).Invoke();
				}
			}
			else if (Object.op_Implicit((Object)(object)passiveButton))
			{
				ButtonClickedEvent onClick2 = passiveButton.OnClick;
				if (onClick2 != null)
				{
					((UnityEvent)onClick2).Invoke();
				}
			}
			targetCooldownSprite.SetCooldownNormalizedUvs();
		}
		else
		{
			if (holdTimer > 0f)
			{
				targetCooldownSprite.SetCooldownNormalizedUvs();
				((Renderer)targetCooldownSprite).material.SetFloat("_Percent", 0f);
			}
			alreadyPressed = false;
			holdTimer = 0f;
		}
	}
}
