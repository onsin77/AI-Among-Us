using System;
using System.Collections;
using GameCore;
using UnityEngine;

public class SignInUserOnButtonPress : MonoBehaviour
{
	public Action OnButtonPress;

	public SceneChanger SceneChanger;

	public TextTranslatorTMP Text;

	private bool inviteReceived;

	private void OnEnable()
	{
		NetworkManager networkManager = GameCoreManager.Instance.NetworkManager;
		networkManager.OnInvited = (NetworkManager.OnInvitedCallback)Delegate.Combine(networkManager.OnInvited, new NetworkManager.OnInvitedCallback(HandleInvite));
	}

	private void OnDisable()
	{
		NetworkManager networkManager = GameCoreManager.Instance.NetworkManager;
		networkManager.OnInvited = (NetworkManager.OnInvitedCallback)Delegate.Remove(networkManager.OnInvited, new NetworkManager.OnInvitedCallback(HandleInvite));
	}

	private void Update()
	{
	}

	public void AddUserWithUI()
	{
	}

	private void AddUserCompleted(UserManager.UserOpResult result)
	{
	}

	private IEnumerator ContinueToNextScene()
	{
		yield return null;
		SceneChanger.AllowFinishLoadingScene();
	}

	public void HandleInvite(string connectionString)
	{
		SceneChanger.BeginLoadingScene();
		inviteReceived = true;
		AddUserWithUI();
	}
}
