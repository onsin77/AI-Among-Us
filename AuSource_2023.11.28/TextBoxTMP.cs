using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextBoxTMP : MonoBehaviour, IFocusHolder
{
	public static readonly HashSet<char> SymbolChars = new HashSet<char>
	{
		'?', '!', ',', '.', '\'', ':', ';', '(', ')', '/',
		'\\', '%', '^', '&', '-', '=', '¿', '？', '#'
	};

	public static readonly HashSet<char> EmailChars = new HashSet<char> { '!', '~', '@', '.', '-', '_', '+' };

	public const char HiddenChar = '*';

	public bool allowAllCharacters;

	public string text;

	private string compoText = "";

	public int characterLimit = -1;

	[SerializeField]
	public TextMeshPro outputText;

	public SpriteRenderer Background;

	public MeshRenderer Pipe;

	private float pipeBlinkTimer;

	public bool ClearOnFocus;

	public bool ForceUppercase;

	public ButtonClickedEvent OnEnter;

	public ButtonClickedEvent OnChange;

	public ButtonClickedEvent OnFocusLost;

	private TouchScreenKeyboard keyboard;

	public bool AllowSymbols;

	public bool AllowEmail;

	public bool IpMode;

	public bool AllowPaste;

	public bool Hidden;

	private Collider2D[] colliders;

	private bool hasFocus;

	private StringBuilder tempTxt = new StringBuilder();

	public SpriteRenderer sendButtonGlyph;

	public float TextHeight => outputText.GetNotDumbRenderedHeight();

	public void Start()
	{
		colliders = ((Component)this).GetComponents<Collider2D>();
		DestroyableSingleton<PassiveButtonManager>.Instance.RegisterOne(this);
		if (Object.op_Implicit((Object)(object)Pipe))
		{
			((Renderer)Pipe).enabled = false;
		}
	}

	public void OnDestroy()
	{
		if (keyboard != null)
		{
			keyboard.active = false;
			keyboard = null;
		}
		if (DestroyableSingleton<PassiveButtonManager>.InstanceExists)
		{
			DestroyableSingleton<PassiveButtonManager>.Instance.RemoveOne(this);
		}
	}

	public void Clear()
	{
		SetText(string.Empty, string.Empty);
	}

	public void Update()
	{
		if (!((Behaviour)this).enabled || !hasFocus)
		{
			return;
		}
		if (AllowPaste && (Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305)) && Input.GetKeyDown((KeyCode)118))
		{
			string clipboardString = ClipboardHelper.GetClipboardString();
			if (!string.IsNullOrWhiteSpace(clipboardString))
			{
				SetText(text + clipboardString);
			}
		}
		string inputString = Input.inputString;
		if (inputString.Length > 0 || compoText != Input.compositionString)
		{
			if (text == null || text == "Enter Name")
			{
				text = "";
			}
			SetText(text + inputString, Input.compositionString);
		}
		if (Object.op_Implicit((Object)(object)Pipe) && hasFocus)
		{
			pipeBlinkTimer += Time.deltaTime * 2f;
			((Renderer)Pipe).enabled = (int)pipeBlinkTimer % 2 == 0;
		}
	}

	public void GiveFocus()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		Input.imeCompositionMode = (IMECompositionMode)1;
		if (hasFocus)
		{
			if (TouchScreenKeyboard.isSupported && (keyboard == null || !TouchScreenKeyboard.visible))
			{
				if (keyboard != null)
				{
					keyboard.active = false;
				}
				keyboard = TouchScreenKeyboard.Open(text);
			}
			return;
		}
		if (ClearOnFocus)
		{
			text = string.Empty;
			compoText = string.Empty;
			((TMP_Text)outputText).text = string.Empty;
		}
		hasFocus = true;
		if (TouchScreenKeyboard.isSupported)
		{
			keyboard = TouchScreenKeyboard.Open(text);
		}
		if (Object.op_Implicit((Object)(object)Background))
		{
			Background.color = Color.green;
		}
		pipeBlinkTimer = 0f;
		if (Object.op_Implicit((Object)(object)Pipe))
		{
			((Component)Pipe).transform.localPosition = Vector2.op_Implicit(outputText.CursorPos());
		}
	}

	public void ForceKeyboardClose()
	{
		if (keyboard != null)
		{
			keyboard.active = false;
			keyboard = null;
		}
	}

	public void LoseFocus()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (hasFocus)
		{
			Input.imeCompositionMode = (IMECompositionMode)2;
			if (compoText.Length > 0)
			{
				SetText(text + compoText);
				compoText = string.Empty;
			}
			hasFocus = false;
			if (keyboard != null)
			{
				keyboard.active = false;
				keyboard = null;
			}
			if (Object.op_Implicit((Object)(object)Background))
			{
				Background.color = Color.white;
			}
			if (Object.op_Implicit((Object)(object)Pipe))
			{
				((Renderer)Pipe).enabled = false;
			}
			if (OnFocusLost != null)
			{
				((UnityEvent)OnFocusLost).Invoke();
			}
			if (Object.op_Implicit((Object)(object)sendButtonGlyph))
			{
				((Renderer)sendButtonGlyph).enabled = true;
			}
		}
	}

	public bool CheckCollision(Vector2 pt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].OverlapPoint(pt))
			{
				return true;
			}
		}
		return false;
	}

	public void SetText(string input, string inputCompo = "")
	{
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		char c = ' ';
		tempTxt.Clear();
		for (int i = 0; i < input.Length; i++)
		{
			char c2 = input[i];
			if (c != ' ' || c2 != ' ')
			{
				if (c2 == '\r' || c2 == '\n')
				{
					flag = true;
				}
				if (c2 == '\b')
				{
					tempTxt.Length = Math.Max(tempTxt.Length - 1, 0);
				}
				if (ForceUppercase)
				{
					c2 = char.ToUpperInvariant(c2);
				}
				if (IsCharAllowed(c2))
				{
					tempTxt.Append(c2);
					c = c2;
				}
			}
		}
		if (!tempTxt.ToString().Equals(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterName), StringComparison.OrdinalIgnoreCase) && characterLimit > 0)
		{
			tempTxt.Length = Math.Min(tempTxt.Length, characterLimit);
		}
		input = tempTxt.ToString();
		if (!input.Equals(this.text) || !inputCompo.Equals(compoText))
		{
			this.text = input;
			compoText = inputCompo;
			string text = this.text;
			string text2 = compoText;
			if (Hidden)
			{
				text = "";
				for (int j = 0; j < this.text.Length; j++)
				{
					text += "*";
				}
			}
			((TMP_Text)outputText).text = text + text2;
			((TMP_Text)outputText).ForceMeshUpdate(true, true);
			if (keyboard != null)
			{
				keyboard.text = this.text;
			}
			((UnityEvent)OnChange).Invoke();
		}
		if (flag)
		{
			((UnityEvent)OnEnter).Invoke();
		}
		if (Object.op_Implicit((Object)(object)Pipe))
		{
			((Component)Pipe).transform.localPosition = Vector2.op_Implicit(outputText.CursorPos());
		}
	}

	public bool IsCharAllowed(char i)
	{
		if (IpMode)
		{
			if (i >= '0' && i <= '9')
			{
				return true;
			}
			if (i == '.')
			{
				return true;
			}
			return false;
		}
		switch (i)
		{
		case ' ':
			return true;
		case 'A':
		case 'B':
		case 'C':
		case 'D':
		case 'E':
		case 'F':
		case 'G':
		case 'H':
		case 'I':
		case 'J':
		case 'K':
		case 'L':
		case 'M':
		case 'N':
		case 'O':
		case 'P':
		case 'Q':
		case 'R':
		case 'S':
		case 'T':
		case 'U':
		case 'V':
		case 'W':
		case 'X':
		case 'Y':
		case 'Z':
			return true;
		default:
			if (i >= 'a' && i <= 'z')
			{
				return true;
			}
			if (i >= '0' && i <= '9')
			{
				return true;
			}
			if (i >= 'À' && i <= 'ÿ')
			{
				return true;
			}
			if (i >= 'Ѐ' && i <= 'џ')
			{
				return true;
			}
			if (i >= '\u3040' && i <= '㆟')
			{
				return true;
			}
			if (i >= 'ⱡ' && i <= '힣')
			{
				return true;
			}
			if (AllowSymbols && SymbolChars.Contains(i))
			{
				return true;
			}
			if (AllowEmail && EmailChars.Contains(i))
			{
				return true;
			}
			return false;
		}
	}

	public void AddText(string text)
	{
		if (this.text.Length > 0 && this.text[this.text.Length - 1] == ' ')
		{
			SetText(this.text + text + " ");
		}
		else
		{
			SetText(this.text + " " + text + " ");
		}
	}

	public void Backspace()
	{
		SetText(text + "\b");
	}

	public void ClearLastWord()
	{
		if (text.Length > 2)
		{
			int num = text.Length - 2;
			while (num > 0 && (text[num] != ' ' || text[num + 1] == ' '))
			{
				num--;
			}
			SetText(text.Substring(0, num));
		}
		else
		{
			SetText("");
		}
	}
}
