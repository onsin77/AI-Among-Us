using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace AmongUs.HTTP;

public class RetryableWebRequest
{
	private readonly Dictionary<string, string> requestHeaders = new Dictionary<string, string>();

	private readonly byte[] body;

	private Action<string> successCallback;

	private Action<RetryableWebRequest> errorCallback;

	public string Url { get; private set; }

	public string Method { get; private set; }

	public long ResponseCode { get; private set; }

	public string ResponseText { get; private set; }

	public string Error { get; private set; }

	public bool IsSuccess
	{
		get
		{
			if (Error == null && ResponseCode >= 200)
			{
				return ResponseCode < 300;
			}
			return false;
		}
	}

	public bool IsFailure => !IsSuccess;

	public bool IsAuthError => ResponseCode == 401;

	public bool IsTransientError
	{
		get
		{
			if ((Error == null || ResponseCode != 0L) && ResponseCode != 408)
			{
				return ResponseCode >= 500;
			}
			return true;
		}
	}

	private RetryableWebRequest(string url, string method, byte[] body = null)
	{
		Url = url;
		Method = method;
		this.body = body;
	}

	public static RetryableWebRequest Head(string url)
	{
		return new RetryableWebRequest(url, "HEAD");
	}

	public static RetryableWebRequest Get(string url)
	{
		return new RetryableWebRequest(url, "GET");
	}

	public static RetryableWebRequest Post(string url, string body)
	{
		return Post(url, Encoding.UTF8.GetBytes(body));
	}

	public static RetryableWebRequest Post(string url, byte[] body)
	{
		return new RetryableWebRequest(url, "POST", body);
	}

	public static RetryableWebRequest Put(string url, string body)
	{
		return Put(url, Encoding.UTF8.GetBytes(body));
	}

	public static RetryableWebRequest Put(string url, byte[] body)
	{
		return new RetryableWebRequest(url, "PUT", body);
	}

	public static RetryableWebRequest Delete(string url)
	{
		return new RetryableWebRequest(url, "DELETE");
	}

	public void SetOrReplaceRequestHeader(string name, string value)
	{
		requestHeaders[name] = value;
	}

	public void SetOrReplaceSuccessCallback(Action<string> callback)
	{
		successCallback = callback;
	}

	public void SetOrReplaceErrorCallback(Action<RetryableWebRequest> callback)
	{
		errorCallback = callback;
	}

	public IEnumerator CoSend(Logger logger)
	{
		UnityWebRequest request = BuildRequest();
		yield return request.SendWebRequest();
		ResponseCode = request.responseCode;
		ResponseText = request.downloadHandler.text;
		Error = request.error;
		if (IsSuccess)
		{
			try
			{
				successCallback?.Invoke(request.downloadHandler.text);
				yield break;
			}
			catch (Exception ex)
			{
				logger?.Error("The success callback passed to RetryableWebRequest threw an exception: " + ex.Message);
				logger?.Exception(ex);
				ResponseCode = 0L;
				ResponseText = null;
				Error = ex.Message;
			}
		}
		try
		{
			errorCallback?.Invoke(this);
		}
		catch (Exception ex2)
		{
			logger?.Error("The error callback passed to RetryableWebRequest threw an exception: " + ex2.Message);
			logger?.Exception(ex2);
		}
	}

	private UnityWebRequest BuildRequest()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		//IL_001d: Expected Obj, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		UnityWebRequest val = new UnityWebRequest(Url, Method)
		{
			downloadHandler = (DownloadHandler)new DownloadHandlerBuffer()
		};
		if (body != null && body.Length != 0)
		{
			val.uploadHandler = (UploadHandler)new UploadHandlerRaw(body);
		}
		if (Method == "PUT")
		{
			val.useHttpContinue = false;
		}
		foreach (KeyValuePair<string, string> requestHeader in requestHeaders)
		{
			val.SetRequestHeader(requestHeader.Key, requestHeader.Value);
		}
		return val;
	}
}
