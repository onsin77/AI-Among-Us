using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[Serializable]
public class ThemeSettings : ScriptableObject
{
	[Serializable]
	private abstract class SelectableSettings_Base
	{
		[SerializeField]
		protected Transition _transition;

		[SerializeField]
		protected CustomColorBlock _colors;

		[SerializeField]
		protected CustomSpriteState _spriteState;

		[SerializeField]
		protected CustomAnimationTriggers _animationTriggers;

		public Transition transition
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _transition;
			}
		}

		public CustomColorBlock selectableColors => _colors;

		public CustomSpriteState spriteState => _spriteState;

		public CustomAnimationTriggers animationTriggers => _animationTriggers;

		public virtual void Apply(Selectable item)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Invalid comparison between Unknown and I4
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Invalid comparison between Unknown and I4
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Invalid comparison between Unknown and I4
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			Transition val = _transition;
			bool flag = item.transition != val;
			item.transition = val;
			ICustomSelectable customSelectable = item as ICustomSelectable;
			if ((int)val == 1)
			{
				CustomColorBlock colors = _colors;
				colors.fadeDuration = 0f;
				item.colors = colors;
				colors.fadeDuration = _colors.fadeDuration;
				item.colors = colors;
				if (customSelectable != null)
				{
					customSelectable.disabledHighlightedColor = colors.disabledHighlightedColor;
				}
			}
			else if ((int)val == 2)
			{
				item.spriteState = _spriteState;
				if (customSelectable != null)
				{
					customSelectable.disabledHighlightedSprite = _spriteState.disabledHighlightedSprite;
				}
			}
			else if ((int)val == 3)
			{
				item.animationTriggers.disabledTrigger = _animationTriggers.disabledTrigger;
				item.animationTriggers.highlightedTrigger = _animationTriggers.highlightedTrigger;
				item.animationTriggers.normalTrigger = _animationTriggers.normalTrigger;
				item.animationTriggers.pressedTrigger = _animationTriggers.pressedTrigger;
				if (customSelectable != null)
				{
					customSelectable.disabledHighlightedTrigger = _animationTriggers.disabledHighlightedTrigger;
				}
			}
			if (flag)
			{
				item.targetGraphic.CrossFadeColor(item.targetGraphic.color, 0f, true, true);
			}
		}
	}

	[Serializable]
	private class SelectableSettings : SelectableSettings_Base
	{
		[SerializeField]
		private ImageSettings _imageSettings;

		public ImageSettings imageSettings => _imageSettings;

		public override void Apply(Selectable item)
		{
			if (!((Object)(object)item == (Object)null))
			{
				base.Apply(item);
				if (_imageSettings != null)
				{
					ImageSettings imageSettings = _imageSettings;
					Graphic targetGraphic = item.targetGraphic;
					imageSettings.CopyTo((Image)(object)((targetGraphic is Image) ? targetGraphic : null));
				}
			}
		}
	}

	[Serializable]
	private class SliderSettings : SelectableSettings_Base
	{
		[SerializeField]
		private ImageSettings _handleImageSettings;

		[SerializeField]
		private ImageSettings _fillImageSettings;

		[SerializeField]
		private ImageSettings _backgroundImageSettings;

		public ImageSettings handleImageSettings => _handleImageSettings;

		public ImageSettings fillImageSettings => _fillImageSettings;

		public ImageSettings backgroundImageSettings => _backgroundImageSettings;

		private void Apply(Slider item)
		{
			if ((Object)(object)item == (Object)null)
			{
				return;
			}
			if (_handleImageSettings != null)
			{
				ImageSettings imageSettings = _handleImageSettings;
				Graphic targetGraphic = ((Selectable)item).targetGraphic;
				imageSettings.CopyTo((Image)(object)((targetGraphic is Image) ? targetGraphic : null));
			}
			if (_fillImageSettings != null)
			{
				RectTransform fillRect = item.fillRect;
				if ((Object)(object)fillRect != (Object)null)
				{
					_fillImageSettings.CopyTo(((Component)fillRect).GetComponent<Image>());
				}
			}
			if (_backgroundImageSettings != null)
			{
				Transform val = ((Component)item).transform.Find("Background");
				if ((Object)(object)val != (Object)null)
				{
					_backgroundImageSettings.CopyTo(((Component)val).GetComponent<Image>());
				}
			}
		}

		public override void Apply(Selectable item)
		{
			base.Apply(item);
			Apply((Slider)(object)((item is Slider) ? item : null));
		}
	}

	[Serializable]
	private class ScrollbarSettings : SelectableSettings_Base
	{
		[SerializeField]
		private ImageSettings _handleImageSettings;

		[SerializeField]
		private ImageSettings _backgroundImageSettings;

		public ImageSettings handle => _handleImageSettings;

		public ImageSettings background => _backgroundImageSettings;

		private void Apply(Scrollbar item)
		{
			if (!((Object)(object)item == (Object)null))
			{
				if (_handleImageSettings != null)
				{
					ImageSettings handleImageSettings = _handleImageSettings;
					Graphic targetGraphic = ((Selectable)item).targetGraphic;
					handleImageSettings.CopyTo((Image)(object)((targetGraphic is Image) ? targetGraphic : null));
				}
				if (_backgroundImageSettings != null)
				{
					_backgroundImageSettings.CopyTo(((Component)item).GetComponent<Image>());
				}
			}
		}

		public override void Apply(Selectable item)
		{
			base.Apply(item);
			Apply((Scrollbar)(object)((item is Scrollbar) ? item : null));
		}
	}

	[Serializable]
	private class ImageSettings
	{
		[SerializeField]
		private Color _color = Color.white;

		[SerializeField]
		private Sprite _sprite;

		[SerializeField]
		private Material _materal;

		[SerializeField]
		private Type _type;

		[SerializeField]
		private bool _preserveAspect;

		[SerializeField]
		private bool _fillCenter;

		[SerializeField]
		private FillMethod _fillMethod;

		[SerializeField]
		private float _fillAmout;

		[SerializeField]
		private bool _fillClockwise;

		[SerializeField]
		private int _fillOrigin;

		public Color color
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _color;
			}
		}

		public Sprite sprite => _sprite;

		public Material materal => _materal;

		public Type type
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _type;
			}
		}

		public bool preserveAspect => _preserveAspect;

		public bool fillCenter => _fillCenter;

		public FillMethod fillMethod
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _fillMethod;
			}
		}

		public float fillAmout => _fillAmout;

		public bool fillClockwise => _fillClockwise;

		public int fillOrigin => _fillOrigin;

		public virtual void CopyTo(Image image)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)image == (Object)null))
			{
				((Graphic)image).color = _color;
				image.sprite = _sprite;
				((Graphic)image).material = _materal;
				image.type = _type;
				image.preserveAspect = _preserveAspect;
				image.fillCenter = _fillCenter;
				image.fillMethod = _fillMethod;
				image.fillAmount = _fillAmout;
				image.fillClockwise = _fillClockwise;
				image.fillOrigin = _fillOrigin;
			}
		}

		public ImageSettings()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	private struct CustomColorBlock
	{
		[SerializeField]
		private float m_ColorMultiplier;

		[SerializeField]
		private Color m_DisabledColor;

		[SerializeField]
		private float m_FadeDuration;

		[SerializeField]
		private Color m_HighlightedColor;

		[SerializeField]
		private Color m_NormalColor;

		[SerializeField]
		private Color m_PressedColor;

		[SerializeField]
		private Color m_SelectedColor;

		[SerializeField]
		private Color m_DisabledHighlightedColor;

		public float colorMultiplier
		{
			get
			{
				return m_ColorMultiplier;
			}
			set
			{
				m_ColorMultiplier = value;
			}
		}

		public Color disabledColor
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return m_DisabledColor;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				m_DisabledColor = value;
			}
		}

		public float fadeDuration
		{
			get
			{
				return m_FadeDuration;
			}
			set
			{
				m_FadeDuration = value;
			}
		}

		public Color highlightedColor
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return m_HighlightedColor;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				m_HighlightedColor = value;
			}
		}

		public Color normalColor
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return m_NormalColor;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				m_NormalColor = value;
			}
		}

		public Color pressedColor
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return m_PressedColor;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				m_PressedColor = value;
			}
		}

		public Color selectedColor
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return m_SelectedColor;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				m_SelectedColor = value;
			}
		}

		public Color disabledHighlightedColor
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return m_DisabledHighlightedColor;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				m_DisabledHighlightedColor = value;
			}
		}

		public static implicit operator ColorBlock(CustomColorBlock item)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			ColorBlock result = default;
			result.selectedColor = item.m_SelectedColor;
			result.colorMultiplier = item.m_ColorMultiplier;
			result.disabledColor = item.m_DisabledColor;
			result.fadeDuration = item.m_FadeDuration;
			result.highlightedColor = item.m_HighlightedColor;
			result.normalColor = item.m_NormalColor;
			result.pressedColor = item.m_PressedColor;
			return result;
		}
	}

	[Serializable]
	private struct CustomSpriteState
	{
		[SerializeField]
		private Sprite m_DisabledSprite;

		[SerializeField]
		private Sprite m_HighlightedSprite;

		[SerializeField]
		private Sprite m_PressedSprite;

		[SerializeField]
		private Sprite m_SelectedSprite;

		[SerializeField]
		private Sprite m_DisabledHighlightedSprite;

		public Sprite disabledSprite
		{
			get
			{
				return m_DisabledSprite;
			}
			set
			{
				m_DisabledSprite = value;
			}
		}

		public Sprite highlightedSprite
		{
			get
			{
				return m_HighlightedSprite;
			}
			set
			{
				m_HighlightedSprite = value;
			}
		}

		public Sprite pressedSprite
		{
			get
			{
				return m_PressedSprite;
			}
			set
			{
				m_PressedSprite = value;
			}
		}

		public Sprite selectedSprite
		{
			get
			{
				return m_SelectedSprite;
			}
			set
			{
				m_SelectedSprite = value;
			}
		}

		public Sprite disabledHighlightedSprite
		{
			get
			{
				return m_DisabledHighlightedSprite;
			}
			set
			{
				m_DisabledHighlightedSprite = value;
			}
		}

		public static implicit operator SpriteState(CustomSpriteState item)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			SpriteState result = default;
			result.selectedSprite = item.m_SelectedSprite;
			result.disabledSprite = item.m_DisabledSprite;
			result.highlightedSprite = item.m_HighlightedSprite;
			result.pressedSprite = item.m_PressedSprite;
			return result;
		}
	}

	[Serializable]
	private class CustomAnimationTriggers
	{
		[SerializeField]
		private string m_DisabledTrigger;

		[SerializeField]
		private string m_HighlightedTrigger;

		[SerializeField]
		private string m_NormalTrigger;

		[SerializeField]
		private string m_PressedTrigger;

		[SerializeField]
		private string m_SelectedTrigger;

		[SerializeField]
		private string m_DisabledHighlightedTrigger;

		public string disabledTrigger
		{
			get
			{
				return m_DisabledTrigger;
			}
			set
			{
				m_DisabledTrigger = value;
			}
		}

		public string highlightedTrigger
		{
			get
			{
				return m_HighlightedTrigger;
			}
			set
			{
				m_HighlightedTrigger = value;
			}
		}

		public string normalTrigger
		{
			get
			{
				return m_NormalTrigger;
			}
			set
			{
				m_NormalTrigger = value;
			}
		}

		public string pressedTrigger
		{
			get
			{
				return m_PressedTrigger;
			}
			set
			{
				m_PressedTrigger = value;
			}
		}

		public string selectedTrigger
		{
			get
			{
				return m_SelectedTrigger;
			}
			set
			{
				m_SelectedTrigger = value;
			}
		}

		public string disabledHighlightedTrigger
		{
			get
			{
				return m_DisabledHighlightedTrigger;
			}
			set
			{
				m_DisabledHighlightedTrigger = value;
			}
		}

		public CustomAnimationTriggers()
		{
			m_DisabledTrigger = string.Empty;
			m_HighlightedTrigger = string.Empty;
			m_NormalTrigger = string.Empty;
			m_PressedTrigger = string.Empty;
			m_SelectedTrigger = string.Empty;
			m_DisabledHighlightedTrigger = string.Empty;
		}

		public static implicit operator AnimationTriggers(CustomAnimationTriggers item)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected Obj, but got Unknown
			return new AnimationTriggers
			{
				selectedTrigger = item.m_SelectedTrigger,
				disabledTrigger = item.m_DisabledTrigger,
				highlightedTrigger = item.m_HighlightedTrigger,
				normalTrigger = item.m_NormalTrigger,
				pressedTrigger = item.m_PressedTrigger
			};
		}
	}

	[Serializable]
	private class TextSettings
	{
		[SerializeField]
		private Color _color = Color.white;

		[SerializeField]
		private TMP_FontAsset _font;

		[SerializeField]
		private FontStyleOverride _style;

		[SerializeField]
		private float _sizeMultiplier = 1f;

		[SerializeField]
		private float _lineSpacing = 1f;

		[SerializeField]
		private float _characterSpacing = 1f;

		[SerializeField]
		private float _wordSpacing = 1f;

		public Color color
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _color;
			}
		}

		public TMP_FontAsset font => _font;

		public FontStyleOverride style => _style;

		public float sizeMultiplier => _sizeMultiplier;

		public float lineSpacing => _lineSpacing;

		public float chracterSpacing => _characterSpacing;

		public float wordSpacing => _wordSpacing;

		public TextSettings()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	private enum FontStyleOverride
	{
		Default,
		Normal,
		Bold,
		Italic,
		BoldAndItalic
	}

	[SerializeField]
	private ImageSettings _mainWindowBackground;

	[SerializeField]
	private ImageSettings _popupWindowBackground;

	[SerializeField]
	private ImageSettings _areaBackground;

	[SerializeField]
	private SelectableSettings _selectableSettings;

	[SerializeField]
	private SelectableSettings _buttonSettings;

	[SerializeField]
	private SelectableSettings _inputGridFieldSettings;

	[SerializeField]
	private ScrollbarSettings _scrollbarSettings;

	[SerializeField]
	private SliderSettings _sliderSettings;

	[SerializeField]
	private ImageSettings _invertToggle;

	[SerializeField]
	private Color _invertToggleDisabledColor;

	[SerializeField]
	private ImageSettings _calibrationBackground;

	[SerializeField]
	private ImageSettings _calibrationValueMarker;

	[SerializeField]
	private ImageSettings _calibrationRawValueMarker;

	[SerializeField]
	private ImageSettings _calibrationZeroMarker;

	[SerializeField]
	private ImageSettings _calibrationCalibratedZeroMarker;

	[SerializeField]
	private ImageSettings _calibrationDeadzone;

	[SerializeField]
	private TextSettings _textSettings;

	[SerializeField]
	private TextSettings _buttonTextSettings;

	[SerializeField]
	private TextSettings _inputGridFieldTextSettings;

	public void Apply(ThemedElement.ElementInfo[] elementInfo)
	{
		if (elementInfo == null)
		{
			return;
		}
		for (int i = 0; i < elementInfo.Length; i++)
		{
			if (elementInfo[i] != null)
			{
				Apply(elementInfo[i].themeClass, elementInfo[i].component);
			}
		}
	}

	private void Apply(string themeClass, Component component)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected Obj, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected Obj, but got Unknown
		if ((Object)(object)((component is Selectable) ? component : null) != (Object)null)
		{
			Apply(themeClass, (Selectable)component);
		}
		else if ((Object)(object)((component is Image) ? component : null) != (Object)null)
		{
			Apply(themeClass, (Image)component);
		}
		else if ((Object)(object)((component is TMP_Text) ? component : null) != (Object)null)
		{
			Apply(themeClass, (TMP_Text)component);
		}
		else if ((Object)(object)(component as UIImageHelper) != (Object)null)
		{
			Apply(themeClass, (UIImageHelper)(object)component);
		}
	}

	private void Apply(string themeClass, Selectable item)
	{
		if (!((Object)(object)item == (Object)null))
		{
			SelectableSettings_Base selectableSettings_Base;
			if ((Object)(object)((item is Button) ? item : null) != (Object)null)
			{
				selectableSettings_Base = ((themeClass == null || !(themeClass == "inputGridField")) ? _buttonSettings : _inputGridFieldSettings);
			}
			else if ((Object)(object)((item is Scrollbar) ? item : null) != (Object)null)
			{
				selectableSettings_Base = _scrollbarSettings;
			}
			else if ((Object)(object)((item is Slider) ? item : null) != (Object)null)
			{
				selectableSettings_Base = _sliderSettings;
			}
			else if ((Object)(object)((item is Toggle) ? item : null) != (Object)null)
			{
				selectableSettings_Base = ((themeClass == null || !(themeClass == "button")) ? _selectableSettings : _buttonSettings);
			}
			else
			{
				selectableSettings_Base = _selectableSettings;
			}
			selectableSettings_Base.Apply(item);
		}
	}

	private void Apply(string themeClass, Image item)
	{
		if ((Object)(object)item == (Object)null || themeClass == null)
		{
			return;
		}
		switch (themeClass)
		{
		case "area":
			if (_areaBackground != null)
			{
				_areaBackground.CopyTo(item);
			}
			break;
		case "popupWindow":
			if (_popupWindowBackground != null)
			{
				_popupWindowBackground.CopyTo(item);
			}
			break;
		case "mainWindow":
			if (_mainWindowBackground != null)
			{
				_mainWindowBackground.CopyTo(item);
			}
			break;
		case "calibrationValueMarker":
			if (_calibrationValueMarker != null)
			{
				_calibrationValueMarker.CopyTo(item);
			}
			break;
		case "calibrationRawValueMarker":
			if (_calibrationRawValueMarker != null)
			{
				_calibrationRawValueMarker.CopyTo(item);
			}
			break;
		case "calibrationBackground":
			if (_calibrationBackground != null)
			{
				_calibrationBackground.CopyTo(item);
			}
			break;
		case "calibrationZeroMarker":
			if (_calibrationZeroMarker != null)
			{
				_calibrationZeroMarker.CopyTo(item);
			}
			break;
		case "calibrationCalibratedZeroMarker":
			if (_calibrationCalibratedZeroMarker != null)
			{
				_calibrationCalibratedZeroMarker.CopyTo(item);
			}
			break;
		case "calibrationDeadzone":
			if (_calibrationDeadzone != null)
			{
				_calibrationDeadzone.CopyTo(item);
			}
			break;
		case "invertToggle":
			if (_invertToggle != null)
			{
				_invertToggle.CopyTo(item);
			}
			break;
		case "invertToggleBackground":
			if (_inputGridFieldSettings != null)
			{
				_inputGridFieldSettings.imageSettings.CopyTo(item);
			}
			break;
		case "invertToggleButtonBackground":
			if (_buttonSettings != null)
			{
				_buttonSettings.imageSettings.CopyTo(item);
			}
			break;
		}
	}

	private void Apply(string themeClass, TMP_Text item)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)item == (Object)null))
		{
			TextSettings textSettings = themeClass switch
			{
				"button" => _buttonTextSettings, 
				"inputGridField" => _inputGridFieldTextSettings, 
				_ => _textSettings, 
			};
			if ((Object)(object)textSettings.font != (Object)null)
			{
				item.font = textSettings.font;
			}
			((Graphic)item).color = textSettings.color;
			item.lineSpacing = textSettings.lineSpacing;
			if (textSettings.sizeMultiplier != 1f)
			{
				item.fontSize = (int)(item.fontSize * textSettings.sizeMultiplier);
				item.fontSizeMax = (int)(item.fontSizeMax * textSettings.sizeMultiplier);
				item.fontSizeMin = (int)(item.fontSizeMin * textSettings.sizeMultiplier);
			}
			item.characterSpacing = textSettings.chracterSpacing;
			item.wordSpacing = textSettings.wordSpacing;
			if (textSettings.style != FontStyleOverride.Default)
			{
				item.fontStyle = GetFontStyle(textSettings.style);
			}
		}
	}

	private void Apply(string themeClass, UIImageHelper item)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)item == (Object)null))
		{
			item.SetEnabledStateColor(_invertToggle.color);
			item.SetDisabledStateColor(_invertToggleDisabledColor);
			item.Refresh();
		}
	}

	private static FontStyles GetFontStyle(FontStyleOverride style)
	{
		switch (style)
		{
		case FontStyleOverride.Bold:
			return (FontStyles)1;
		case FontStyleOverride.BoldAndItalic:
			return (FontStyles)3;
		case FontStyleOverride.Italic:
			return (FontStyles)2;
		case FontStyleOverride.Default:
		case FontStyleOverride.Normal:
			return (FontStyles)0;
		default:
			throw new NotImplementedException();
		}
	}
}
