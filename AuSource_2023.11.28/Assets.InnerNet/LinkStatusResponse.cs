namespace Assets.InnerNet;

public class LinkStatusResponse
{
	public enum LinkStatus
	{
		Pending,
		Success,
		Error
	}

	public LinkStatus Status;

	public PinRequestError[] Errors = new PinRequestError[0];
}
