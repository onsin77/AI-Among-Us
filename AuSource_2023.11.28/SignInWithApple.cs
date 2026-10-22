using System;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using UnityEngine;

public class SignInWithApple : MonoBehaviour
{
	private IAppleAuthManager appleAuthManager;

	public void Initialize(Action onRevoke)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		if (appleAuthManager == null)
		{
			PayloadDeserializer val = new PayloadDeserializer();
			appleAuthManager = (IAppleAuthManager)new AppleAuthManager((IPayloadDeserializer)(object)val);
			appleAuthManager.SetCredentialsRevokedCallback((Action<string>)((string result) =>
			{
				Logger.GlobalInstance.Info("SIWA: Received revoked callback " + result);
				onRevoke();
			}));
		}
	}

	public void GetCredentialState(string userId, Action<CredentialState> onSuccess, Action onFail)
	{
		appleAuthManager.GetCredentialState(userId, (Action<CredentialState>)((CredentialState state) =>
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			Logger.GlobalInstance.Info($"SIWA: Got credential state back from apple: {state}");
			onSuccess(state);
		}), (Action<IAppleError>)((IAppleError error) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			AuthorizationErrorCode authorizationErrorCode = AppleErrorExtensions.GetAuthorizationErrorCode(error);
			Logger.GlobalInstance.Warning($"SIWA: Sign in with Apple failed {authorizationErrorCode} {error}");
			onFail();
		}));
	}

	public void LoginWithAppleId(Action<ICredential> onSuccess, Action<IAppleError> onFail)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		AppleAuthLoginArgs val = default;
		Debug.Log((object)"SIWA: Trying full login");
		appleAuthManager.LoginWithAppleId(val, (Action<ICredential>)((ICredential credential) =>
		{
			onSuccess(credential);
		}), (Action<IAppleError>)((IAppleError error) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			AuthorizationErrorCode authorizationErrorCode = AppleErrorExtensions.GetAuthorizationErrorCode(error);
			Logger.GlobalInstance.Warning($"SIWA: Sign in with Apple failed {authorizationErrorCode} {error} Code: {error.Code}");
			onFail(error);
		}));
	}

	public void QuickLogin(Action<ICredential> onSuccess, Action<IAppleError> onFail)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"SIWA: Trying Quick login");
		appleAuthManager.QuickLogin(default(AppleAuthQuickLoginArgs), (Action<ICredential>)((ICredential credential) =>
		{
			Logger.GlobalInstance.Info("SIWA: Successfully got credential in quick login");
			onSuccess(credential);
		}), (Action<IAppleError>)((IAppleError error) =>
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Logger.GlobalInstance.Warning($"SIWA: There was an error doing Quick login: {AppleErrorExtensions.GetAuthorizationErrorCode(error)} {error}");
			onFail(error);
		}));
	}

	private void Update()
	{
		IAppleAuthManager val = appleAuthManager;
		if (val != null)
		{
			val.Update();
		}
	}
}
