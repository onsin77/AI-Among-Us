using UnityEngine;
using UnityEngine.UI;

namespace Rewired.Demos.GamepadTemplateUI;

[RequireComponent(typeof(Image))]
public class ControllerUIElement : MonoBehaviour
{
	[SerializeField]
	private Color _highlightColor = Color.white;

	[SerializeField]
	private ControllerUIEffect _positiveUIEffect;

	[SerializeField]
	private ControllerUIEffect _negativeUIEffect;

	[SerializeField]
	private Text _label;

	[SerializeField]
	private Text _positiveLabel;

	[SerializeField]
	private Text _negativeLabel;

	[SerializeField]
	private ControllerUIElement[] _childElements = new ControllerUIElement[0];

	private Image _image;

	private Color _color;

	private Color _origColor;

	private bool _isActive;

	private float _highlightAmount;

	private bool hasEffects
	{
		get
		{
			if (!((Object)(object)_positiveUIEffect != (Object)null))
			{
				return (Object)(object)_negativeUIEffect != (Object)null;
			}
			return true;
		}
	}

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		_image = ((Component)this).GetComponent<Image>();
		_origColor = ((Graphic)_image).color;
		_color = _origColor;
		ClearLabels();
	}

	public void Activate(float amount)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		amount = Mathf.Clamp(amount, -1f, 1f);
		if (hasEffects)
		{
			if (amount < 0f && (Object)(object)_negativeUIEffect != (Object)null)
			{
				_negativeUIEffect.Activate(Mathf.Abs(amount));
			}
			if (amount > 0f && (Object)(object)_positiveUIEffect != (Object)null)
			{
				_positiveUIEffect.Activate(Mathf.Abs(amount));
			}
		}
		else
		{
			if (_isActive && amount == _highlightAmount)
			{
				return;
			}
			_highlightAmount = amount;
			_color = Color.Lerp(_origColor, _highlightColor, _highlightAmount);
		}
		_isActive = true;
		RedrawImage();
		if (_childElements.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _childElements.Length; i++)
		{
			if (!((Object)(object)_childElements[i] == (Object)null))
			{
				_childElements[i].Activate(amount);
			}
		}
	}

	public void Deactivate()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!_isActive)
		{
			return;
		}
		_color = _origColor;
		_highlightAmount = 0f;
		if ((Object)(object)_positiveUIEffect != (Object)null)
		{
			_positiveUIEffect.Deactivate();
		}
		if ((Object)(object)_negativeUIEffect != (Object)null)
		{
			_negativeUIEffect.Deactivate();
		}
		_isActive = false;
		RedrawImage();
		if (_childElements.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _childElements.Length; i++)
		{
			if (!((Object)(object)_childElements[i] == (Object)null))
			{
				_childElements[i].Deactivate();
			}
		}
	}

	public void SetLabel(string text, AxisRange labelType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Text val = (int)labelType switch
		{
			0 => _label, 
			1 => _positiveLabel, 
			2 => _negativeLabel, 
			_ => (Text)null, 
		};
		if ((Object)(object)val != (Object)null)
		{
			val.text = text;
		}
		if (_childElements.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _childElements.Length; i++)
		{
			if (!((Object)(object)_childElements[i] == (Object)null))
			{
				_childElements[i].SetLabel(text, labelType);
			}
		}
	}

	public void ClearLabels()
	{
		if ((Object)(object)_label != (Object)null)
		{
			_label.text = string.Empty;
		}
		if ((Object)(object)_positiveLabel != (Object)null)
		{
			_positiveLabel.text = string.Empty;
		}
		if ((Object)(object)_negativeLabel != (Object)null)
		{
			_negativeLabel.text = string.Empty;
		}
		if (_childElements.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _childElements.Length; i++)
		{
			if (!((Object)(object)_childElements[i] == (Object)null))
			{
				_childElements[i].ClearLabels();
			}
		}
	}

	private void RedrawImage()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_image).color = _color;
	}

	public ControllerUIElement()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
