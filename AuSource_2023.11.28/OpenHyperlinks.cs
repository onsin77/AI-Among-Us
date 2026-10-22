using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class OpenHyperlinks : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro pTextMeshPro;

	[SerializeField]
	private Color linkColor;

	public string Text
	{
		get
		{
			return ((TMP_Text)pTextMeshPro).text;
		}
		set
		{
			((TMP_Text)pTextMeshPro).text = value;
		}
	}

	public Color LinkColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return linkColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			linkColor = value;
		}
	}

	public bool GetWordPosition(string text, List<string> wordsPositioned, out Vector3 bottomLeft, out Vector3 topRight, out int lineBreakCount)
	{
		return pTextMeshPro.GetWordPosition(text, wordsPositioned, out bottomLeft, out topRight, out lineBreakCount);
	}

	public void UpdateTMPMesh()
	{
		((TMP_Text)pTextMeshPro).ForceMeshUpdate(false, false);
		((TMP_Text)pTextMeshPro).UpdateVertexData();
	}
}
