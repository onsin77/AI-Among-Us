using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Innersloth.DebugTool;

public class DebugCommandButton : MonoBehaviour
{
	private const int INDENT_SIZE = 50;

	[SerializeField]
	private TextMeshProUGUI buttonLabel;

	[SerializeField]
	private Button button;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image expandCollapseImage;

	[SerializeField]
	private float baseArrowXOffset = 15f;

	[SerializeField]
	private Sprite expandedIcon;

	[SerializeField]
	private Sprite collapsedIcon;

	[SerializeField]
	private Color evenColor;

	[SerializeField]
	private Color oddColor;

	[SerializeField]
	private Color highlightColor;

	private int index;

	public bool IsDirectory { get; private set; }

	public string Path { get; private set; }

	public bool Expanded
	{
		get
		{
			if (IsDirectory)
			{
				return (Object)(object)expandCollapseImage.sprite == (Object)(object)expandedIcon;
			}
			return true;
		}
		set
		{
			expandCollapseImage.sprite = (value ? expandedIcon : collapsedIcon);
		}
	}

	public void Initialize(string label, string path, Action action, int index)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected Obj, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)buttonLabel).text = label;
		((Object)this).name = $"DebugCommandButton [{path}] [{index}]";
		this.index = index;
		Path = path;
		((UnityEventBase)button.onClick).RemoveAllListeners();
		((UnityEvent)button.onClick).AddListener((UnityAction)(() =>
		{
			action();
		}));
		((Graphic)background).color = ((index % 2 == 0) ? evenColor : oddColor);
	}

	public void ToggleDirectory(bool directory)
	{
		((Behaviour)expandCollapseImage).enabled = directory;
		IsDirectory = directory;
	}

	public void SetIndentLevel(int indentLevel)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)expandCollapseImage).rectTransform.anchoredPosition = new Vector2(baseArrowXOffset + (float)(indentLevel * 50), 0f);
		((TMP_Text)buttonLabel).rectTransform.anchoredPosition = new Vector2((float)(indentLevel * 50) + ((Graphic)expandCollapseImage).rectTransform.sizeDelta.x, 0f);
	}

	public void ToggleHighlight(bool highlighted)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (highlighted)
		{
			((Graphic)background).color = highlightColor;
		}
		else
		{
			((Graphic)background).color = ((index % 2 == 0) ? evenColor : oddColor);
		}
	}
}
