using System.Text;
using TMPro;

public static class UnicodeSupport
{
	private static StringBuilder sb = new StringBuilder();

	public static string FilterUnsupportedCharacters(TextMeshPro tr, FontData data)
	{
		sb.Clear();
		for (int i = 0; i < ((TMP_Text)tr).text.Length; i++)
		{
			if (((TMP_Text)tr).text[i] > ' ' && !data.charMap.ContainsKey(((TMP_Text)tr).text[i]))
			{
				sb.Append('□');
			}
			else
			{
				sb.Append(((TMP_Text)tr).text[i]);
			}
		}
		return sb.ToString();
	}
}
