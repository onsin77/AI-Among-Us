namespace Assets.InnerNet;

public class PinRequestError
{
	public int Code;

	public string Title;

	public string Detail;

	public override string ToString()
	{
		return $"ERROR '{Title}' ({Code}): {Detail}";
	}
}
