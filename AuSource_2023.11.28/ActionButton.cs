using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ActionButton : MonoBehaviour
{
	[SerializeField]
	protected SpriteRenderer graphic;

	[SerializeField]
	protected SpriteRenderer usesRemainingSprite;

	[SerializeField]
	protected TextMeshPro usesRemainingText;

	[SerializeField]
	protected TextMeshPro buttonLabelText;

	[SerializeField]
	protected TextMeshPro cooldownTimerText;

	[SerializeField]
	private ActionMapGlyphDisplay glyph;

	protected bool isCoolingDown;

	protected readonly Logger logger = new Logger("ActionButton", Logger.Level.Debug, Logger.Category.UI);

	protected bool canInteract = true;

	private Vector3 position;

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		position = ((Component)graphic).transform.localPosition;
	}

	public abstract void DoClick();

	public bool CanInteract()
	{
		return canInteract;
	}

	public void SetCoolDown(float timer, float maxTimer)
	{
		float num = Mathf.Clamp(timer / maxTimer, 0f, 1f);
		isCoolingDown = num > 0f;
		SetCooldownFill(num);
		if (isCoolingDown)
		{
			((TMP_Text)cooldownTimerText).text = Mathf.CeilToInt(timer).ToString();
			((Component)cooldownTimerText).gameObject.SetActive(true);
		}
		else
		{
			((Component)cooldownTimerText).gameObject.SetActive(false);
		}
	}

	public void SetUsesRemaining(int num)
	{
		((Component)usesRemainingText).gameObject.SetActive(true);
		((TMP_Text)usesRemainingText).text = num.ToString();
		((Component)usesRemainingSprite).gameObject.SetActive(true);
	}

	public void SetInfiniteUses()
	{
		((Component)usesRemainingText).gameObject.SetActive(false);
		((Component)usesRemainingSprite).gameObject.SetActive(false);
	}

	public void SetFillUp(float timer, float maxTimer)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp((maxTimer - timer) / maxTimer, 0f, 1f);
		isCoolingDown = num > 0f;
		if (isCoolingDown && timer < 3f)
		{
			((Component)graphic).transform.localPosition = position + Vector2.op_Implicit(Random.insideUnitCircle) * 0.05f;
			((TMP_Text)cooldownTimerText).text = Mathf.CeilToInt(timer).ToString();
			((Component)cooldownTimerText).gameObject.SetActive(true);
		}
		else
		{
			((Component)graphic).transform.localPosition = position;
		}
		SetCooldownFill(num);
	}

	public void SetEnabled()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		canInteract = true;
		graphic.color = Palette.EnabledColor;
		((Graphic)buttonLabelText).color = Palette.EnabledColor;
		((Renderer)graphic).material.SetFloat("_Desat", 0f);
		glyph.SetColor(Palette.EnabledColor);
	}

	public void SetDisabled()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		canInteract = false;
		((Graphic)buttonLabelText).color = Palette.DisabledClear;
		graphic.color = Palette.DisabledClear;
		((Renderer)graphic).material.SetFloat("_Desat", 1f);
		glyph.SetColor(Palette.DisabledClear);
	}

	public void ToggleVisible(bool visible)
	{
		if (visible)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void SetCooldownFill(float percentCool)
	{
		((Renderer)graphic).material.SetFloat("_Percent", percentCool);
	}

	public void OverrideText(string text)
	{
		((TMP_Text)buttonLabelText).text = text;
	}

	public void OverrideColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		graphic.color = color;
	}

	protected void ResetCoolDown()
	{
		isCoolingDown = false;
		SetCooldownFill(1f);
		((Component)cooldownTimerText).gameObject.SetActive(false);
	}
}
