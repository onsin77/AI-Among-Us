public static class TextFormatter
{
	public static string Commas(uint number)
	{
		return number.ToString("N0");
	}

	public static string Multiplier(float multiplier)
	{
		return "x" + multiplier.ToString("F1");
	}
}
