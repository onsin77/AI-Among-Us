using UnityEngine;
using UnityEngine.UI;

namespace Rewired.Demos.GamepadTemplateUI;

[RequireComponent(typeof(Image))]
public class ControllerUIEffect : MonoBehaviour
{
	[SerializeField]
	private Color _highlightColor = Color.white;

	private Image _image;

	private Color _color;

	private Color _origColor;

	private bool _isActive;

	private float _highlightAmount;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		_image = ((Component)this).GetComponent<Image>();
		_origColor = ((Graphic)_image).color;
		_color = _origColor;
	}

	public void Activate(float amount)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		amount = Mathf.Clamp01(amount);
		if (!_isActive || amount != _highlightAmount)
		{
			_highlightAmount = amount;
			_color = Color.Lerp(_origColor, _highlightColor, _highlightAmount);
			_isActive = true;
			RedrawImage();
		}
	}

	public void Deactivate()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (_isActive)
		{
			_color = _origColor;
			_highlightAmount = 0f;
			_isActive = false;
			RedrawImage();
		}
	}

	private void RedrawImage()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_image).color = _color;
		((Behaviour)_image).enabled = _isActive;
	}

	public ControllerUIEffect()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
