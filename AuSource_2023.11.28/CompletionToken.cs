using System.Collections;

public class CompletionToken<T>
{
	private bool complete;

	public string errorStr = "";

	public static CompletionToken<T> Default => new CompletionToken<T>(default);

	public T Value { get; private set; }

	public CompletionToken(T defaultValue)
	{
		Value = defaultValue;
	}

	public void MarkComplete(T resultValue)
	{
		complete = true;
		Value = resultValue;
	}

	public void MarkComplete(T resultValue, string error)
	{
		complete = true;
		Value = resultValue;
		errorStr = error;
	}

	public IEnumerator WaitForCompletion()
	{
		while (!complete)
		{
			yield return null;
		}
	}
}
