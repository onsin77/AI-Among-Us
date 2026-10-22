namespace Assets.InnerNet;

public static class EmailUtils
{
	public static string CensorEmail(string rawEmail)
	{
		try
		{
			string[] array = rawEmail.Split('@');
			string text = array[0];
			int length = text.Length;
			text = ((length > 3) ? ("***" + text.Substring(length - 3, 3)) : "*****");
			return text + "@" + array[1];
		}
		catch
		{
			return "<invalid email>";
		}
	}
}
