using UnityEngine;

public static class StringExtensions
{
	private static char[] buffer = new char[256];

	public static string Lerp(string a, string b, float t)
	{
		int num = Mathf.Max(a.Length, b.Length);
		int num2 = (int)Mathf.Lerp(0f, (float)num, t);
		for (int i = 0; i < num; i++)
		{
			if (i < num2)
			{
				if (i < b.Length)
				{
					buffer[i] = b[i];
				}
				else
				{
					buffer[i] = ' ';
				}
			}
			else if (i < a.Length)
			{
				buffer[i] = a[i];
			}
		}
		return new string(buffer, 0, num);
	}

	public static string Color(this string toColor, Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return toColor.Color("#" + ColorUtility.ToHtmlStringRGBA(color));
	}

	public static string Color(this string toColor, string color)
	{
		return "<color=" + color + ">" + toColor + "</color>";
	}
}
