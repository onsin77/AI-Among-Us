using AmongUs.Data;
using TMPro;
using UnityEngine;

public class SetNameText : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro nameText;

	private void Start()
	{
		((TMP_Text)nameText).text = DataManager.Player.Customization.Name;
	}
}
