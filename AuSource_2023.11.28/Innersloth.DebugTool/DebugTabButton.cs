using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Innersloth.DebugTool;

public class DebugTabButton : MonoBehaviour
{
	[SerializeField]
	private string pageName;

	[SerializeField]
	private TextMeshProUGUI buttonLabel;

	[SerializeField]
	private Button button;

	public string PageName => pageName;

	private void Awake()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		((TMP_Text)buttonLabel).text = pageName;
		((UnityEvent)button.onClick).AddListener((UnityAction)(() =>
		{
			DebugUIEvents.OnTabClicked(pageName);
		}));
	}

	public void ToggleSelect(bool selected)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)((Selectable)button).image).color = (selected ? Color.gray.SetAlpha(0.5f) : Color.black.SetAlpha(0.5f));
	}
}
