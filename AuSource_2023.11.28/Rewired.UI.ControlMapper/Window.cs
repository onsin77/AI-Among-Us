using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
[RequireComponent(typeof(CanvasGroup))]
public class Window : MonoBehaviour
{
	public class Timer
	{
		private bool _started;

		private float end;

		public bool started => _started;

		public bool finished
		{
			get
			{
				if (!started)
				{
					return false;
				}
				if (Time.realtimeSinceStartup < end)
				{
					return false;
				}
				_started = false;
				return true;
			}
		}

		public float remaining
		{
			get
			{
				if (!_started)
				{
					return 0f;
				}
				return end - Time.realtimeSinceStartup;
			}
		}

		public void Start(float length)
		{
			end = Time.realtimeSinceStartup + length;
			_started = true;
		}

		public void Stop()
		{
			_started = false;
		}
	}

	public Image backgroundImage;

	public GameObject content;

	private bool _initialized;

	private int _id = -1;

	private RectTransform _rectTransform;

	private TMP_Text _titleText;

	private List<TMP_Text> _contentText;

	private GameObject _defaultUIElement;

	private Action<int> _updateCallback;

	private Func<int, bool> _isFocusedCallback;

	private Timer _timer;

	private CanvasGroup _canvasGroup;

	public UnityAction cancelCallback;

	private GameObject lastUISelection;

	public bool hasFocus
	{
		get
		{
			if (_isFocusedCallback == null)
			{
				return false;
			}
			return _isFocusedCallback(_id);
		}
	}

	public int id => _id;

	public RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)_rectTransform == (Object)null)
			{
				_rectTransform = ((Component)this).gameObject.GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public TMP_Text titleText => _titleText;

	public List<TMP_Text> contentText => _contentText;

	public GameObject defaultUIElement
	{
		get
		{
			return _defaultUIElement;
		}
		set
		{
			_defaultUIElement = value;
		}
	}

	public Action<int> updateCallback
	{
		get
		{
			return _updateCallback;
		}
		set
		{
			_updateCallback = value;
		}
	}

	public Timer timer => _timer;

	public int width
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return (int)rectTransform.sizeDelta.x;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			Vector2 sizeDelta = rectTransform.sizeDelta;
			sizeDelta.x = value;
			rectTransform.sizeDelta = sizeDelta;
		}
	}

	public int height
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return (int)rectTransform.sizeDelta.y;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			Vector2 sizeDelta = rectTransform.sizeDelta;
			sizeDelta.y = value;
			rectTransform.sizeDelta = sizeDelta;
		}
	}

	protected bool initialized => _initialized;

	private void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine("OnEnableAsync");
	}

	protected virtual void Update()
	{
		if (_initialized && hasFocus)
		{
			CheckUISelection();
			if (_updateCallback != null)
			{
				_updateCallback(_id);
			}
		}
	}

	public virtual void Initialize(int id, Func<int, bool> isFocusedCallback)
	{
		if (_initialized)
		{
			Debug.LogError((object)"Window is already initialized!");
			return;
		}
		_id = id;
		_isFocusedCallback = isFocusedCallback;
		_timer = new Timer();
		_contentText = new List<TMP_Text>();
		_canvasGroup = ((Component)this).GetComponent<CanvasGroup>();
		_initialized = true;
	}

	public void SetSize(int width, int height)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.sizeDelta = new Vector2((float)width, (float)height);
	}

	public void CreateTitleText(GameObject prefab, Vector2 offset)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		CreateText(prefab, ref _titleText, "Title Text", UIPivot.TopCenter, UIAnchor.TopHStretch, offset);
	}

	public void CreateTitleText(GameObject prefab, Vector2 offset, string text)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		CreateTitleText(prefab, offset);
		SetTitleText(text);
	}

	public void AddContentText(GameObject prefab, UIPivot pivot, UIAnchor anchor, Vector2 offset)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TMP_Text textComponent = null;
		CreateText(prefab, ref textComponent, "Content Text", pivot, anchor, offset);
		_contentText.Add(textComponent);
	}

	public void AddContentText(GameObject prefab, UIPivot pivot, UIAnchor anchor, Vector2 offset, string text)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddContentText(prefab, pivot, anchor, offset);
		SetContentText(text, _contentText.Count - 1);
	}

	public void AddContentImage(GameObject prefab, UIPivot pivot, UIAnchor anchor, Vector2 offset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		CreateImage(prefab, "Image", pivot, anchor, offset);
	}

	public void AddContentImage(GameObject prefab, UIPivot pivot, UIAnchor anchor, Vector2 offset, string text)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddContentImage(prefab, pivot, anchor, offset);
	}

	public void CreateButton(GameObject prefab, UIPivot pivot, UIAnchor anchor, Vector2 offset, string buttonText, UnityAction confirmCallback, UnityAction cancelCallback, bool setDefault)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)prefab == (Object)null)
		{
			return;
		}
		GameObject val = CreateButton(prefab, "Button", anchor, pivot, offset, out var buttonInfo);
		if (!((Object)(object)val == (Object)null))
		{
			Button component = val.GetComponent<Button>();
			if (confirmCallback != null)
			{
				((UnityEvent)component.onClick).AddListener(confirmCallback);
			}
			CustomButton customButton = component as CustomButton;
			if (cancelCallback != null && (Object)(object)customButton != (Object)null)
			{
				customButton.CancelEvent += cancelCallback;
			}
			if ((Object)(object)buttonInfo.text != (Object)null)
			{
				buttonInfo.text.text = buttonText;
			}
			if (setDefault)
			{
				_defaultUIElement = val;
			}
		}
	}

	public string GetTitleText(string text)
	{
		if ((Object)(object)_titleText == (Object)null)
		{
			return string.Empty;
		}
		return _titleText.text;
	}

	public void SetTitleText(string text)
	{
		if (!((Object)(object)_titleText == (Object)null))
		{
			_titleText.text = text;
		}
	}

	public string GetContentText(int index)
	{
		if (_contentText == null || _contentText.Count <= index || (Object)(object)_contentText[index] == (Object)null)
		{
			return string.Empty;
		}
		return _contentText[index].text;
	}

	public float GetContentTextHeight(int index)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (_contentText == null || _contentText.Count <= index || (Object)(object)_contentText[index] == (Object)null)
		{
			return 0f;
		}
		return _contentText[index].rectTransform.sizeDelta.y;
	}

	public void SetContentText(string text, int index)
	{
		if (_contentText != null && _contentText.Count > index && !((Object)(object)_contentText[index] == (Object)null))
		{
			_contentText[index].text = text;
		}
	}

	public void SetUpdateCallback(Action<int> callback)
	{
		updateCallback = callback;
	}

	public virtual void TakeInputFocus()
	{
		if (!((Object)(object)EventSystem.current == (Object)null))
		{
			EventSystem.current.SetSelectedGameObject(_defaultUIElement);
			Enable();
		}
	}

	public virtual void Enable()
	{
		_canvasGroup.interactable = true;
	}

	public virtual void Disable()
	{
		_canvasGroup.interactable = false;
	}

	public virtual void Cancel()
	{
		if (initialized && cancelCallback != null)
		{
			cancelCallback.Invoke();
		}
	}

	private void CreateText(GameObject prefab, ref TMP_Text textComponent, string name, UIPivot pivot, UIAnchor anchor, Vector2 offset)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)prefab == (Object)null || (Object)(object)content == (Object)null)
		{
			return;
		}
		if ((Object)(object)textComponent != (Object)null)
		{
			Debug.LogError((object)("Window already has " + name + "!"));
			return;
		}
		GameObject val = UITools.InstantiateGUIObject<TMP_Text>(prefab, content.transform, name, UIPivot.op_Implicit(pivot), anchor.min, anchor.max, offset);
		if (!((Object)(object)val == (Object)null))
		{
			textComponent = val.GetComponent<TMP_Text>();
		}
	}

	private void CreateImage(GameObject prefab, string name, UIPivot pivot, UIAnchor anchor, Vector2 offset)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)prefab == (Object)null) && !((Object)(object)content == (Object)null))
		{
			UITools.InstantiateGUIObject<Image>(prefab, content.transform, name, UIPivot.op_Implicit(pivot), anchor.min, anchor.max, offset);
		}
	}

	private GameObject CreateButton(GameObject prefab, string name, UIAnchor anchor, UIPivot pivot, Vector2 offset, out ButtonInfo buttonInfo)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		buttonInfo = null;
		if ((Object)(object)prefab == (Object)null)
		{
			return null;
		}
		GameObject val = UITools.InstantiateGUIObject<ButtonInfo>(prefab, content.transform, name, UIPivot.op_Implicit(pivot), anchor.min, anchor.max, offset);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		buttonInfo = val.GetComponent<ButtonInfo>();
		if ((Object)(object)val.GetComponent<Button>() == (Object)null)
		{
			Debug.Log((object)"Button prefab is missing Button component!");
			return null;
		}
		if ((Object)(object)buttonInfo == (Object)null)
		{
			Debug.Log((object)"Button prefab is missing ButtonInfo component!");
			return null;
		}
		return val;
	}

	private IEnumerator OnEnableAsync()
	{
		yield return 1;
		if (!((Object)(object)EventSystem.current == (Object)null))
		{
			if ((Object)(object)defaultUIElement != (Object)null)
			{
				EventSystem.current.SetSelectedGameObject(defaultUIElement);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
			}
		}
	}

	private void CheckUISelection()
	{
		if (hasFocus && !((Object)(object)EventSystem.current == (Object)null))
		{
			if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null)
			{
				RestoreDefaultOrLastUISelection();
			}
			lastUISelection = EventSystem.current.currentSelectedGameObject;
		}
	}

	private void RestoreDefaultOrLastUISelection()
	{
		if (hasFocus)
		{
			if ((Object)(object)lastUISelection == (Object)null || !lastUISelection.activeInHierarchy)
			{
				SetUISelection(_defaultUIElement);
			}
			else
			{
				SetUISelection(lastUISelection);
			}
		}
	}

	private void SetUISelection(GameObject selection)
	{
		if (!((Object)(object)EventSystem.current == (Object)null))
		{
			EventSystem.current.SetSelectedGameObject(selection);
		}
	}
}
