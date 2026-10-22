namespace Assets.InnerNet;

public class Response<T>
{
	public class ResponseData
	{
		public string Type;

		public T Attributes;
	}

	public ResponseData Data;

	public ResponseError[] Errors;

	public ResponseError[] Warnings;
}
