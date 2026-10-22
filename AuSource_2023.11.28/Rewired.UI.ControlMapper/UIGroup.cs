using TMPro;
using UnityEngine;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
public class UIGroup : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _label;

	[SerializeField]
	private Transform _content;

	public string labelText
	{
		get
		{
			if (!((Object)(object)_label != (Object)null))
			{
				return string.Empty;
			}
			return _label.text;
		}
		set
		{
			if (!((Object)(object)_label == (Object)null))
			{
				_label.text = value;
			}
		}
	}

	public Transform content => _content;

	public void SetLabelActive(bool state)
	{
		if (!((Object)(object)_label == (Object)null))
		{
			((Component)_label).gameObject.SetActive(state);
		}
	}
}
