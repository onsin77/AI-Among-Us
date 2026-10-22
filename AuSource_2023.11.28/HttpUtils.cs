using System;
using System.Collections.Generic;
using System.Linq;

public static class HttpUtils
{
	public static bool IsSuccess(long httpStatusCode)
	{
		if (httpStatusCode >= 200)
		{
			return httpStatusCode < 300;
		}
		return false;
	}

	public static string ToUriQuery(Dictionary<string, object> uriParams)
	{
		return string.Join("&", uriParams.Select((KeyValuePair<string, object> kvp) => Uri.EscapeUriString(kvp.Key) + "=" + Uri.EscapeUriString(kvp.Value.ToString())));
	}
}
