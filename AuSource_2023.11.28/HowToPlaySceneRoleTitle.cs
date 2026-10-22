using TMPro;
using UnityEngine;

public class HowToPlaySceneRoleTitle : MonoBehaviour
{
	[SerializeField]
	private TMP_Text text;

	[SerializeField]
	private StringNames teamString;

	[SerializeField]
	private StringNames roleName;

	[SerializeField]
	private Color roleColor;

	private void Awake()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		text.text = DestroyableSingleton<TranslationController>.Instance.GetString(teamString, "<b>" + DestroyableSingleton<TranslationController>.Instance.GetString(roleName).Color(roleColor) + "</b>");
	}
}
