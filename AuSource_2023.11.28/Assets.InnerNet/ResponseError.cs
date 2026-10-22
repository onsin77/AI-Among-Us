namespace Assets.InnerNet;

public class ResponseError
{
	public int Code;

	public string Title;

	public string Keyword;

	public int Detail;

	public override string ToString()
	{
		return $"ERROR '{Title}' ({Code}): {Keyword} - {Detail}";
	}
}
