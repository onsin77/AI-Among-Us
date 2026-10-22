using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Innersloth.DebugTool;

public class DebugOverlay : MonoBehaviour
{
	[SerializeField]
	private GameObject rootUI;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button backButton;

	[SerializeField]
	[Header("Tabs")]
	private DebugTabButton[] tabButtons;

	[SerializeField]
	private DebugOverlayPage[] pages;

	private readonly List<IDebugToolInputProvider> inputProviders = new List<IDebugToolInputProvider>();

	private int activeTabIndex;

	private int totalTabs;

	private bool UIVisible => rootUI.activeSelf;

	private bool SubpageShowingDetails => ((Component)backButton).gameObject.activeSelf;

	private DebugOverlayPage ActivePage => pages.First((DebugOverlayPage page) => page.Name == tabButtons[activeTabIndex].PageName);

	private bool SubpageConsumingNavActions => ActivePage.ConsumingNavActions;

	private event Action<bool> OnToggle;

	private void Awake()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void ShowPage(string pageName)
	{
		bool flag = false;
		for (int i = 0; i < tabButtons.Length; i++)
		{
			DebugTabButton obj = tabButtons[i];
			obj.ToggleSelect(obj.PageName == pageName);
			if (obj.PageName == pageName)
			{
				activeTabIndex = i;
				flag = true;
			}
		}
		DebugOverlayPage[] array = pages;
		foreach (DebugOverlayPage debugOverlayPage in array)
		{
			if (debugOverlayPage.Name == pageName)
			{
				((Component)debugOverlayPage).gameObject.SetActive(true);
				debugOverlayPage.Show();
			}
			else
			{
				((Component)debugOverlayPage).gameObject.SetActive(false);
			}
		}
		if (!flag)
		{
			Debug.LogError((object)("No tab or page was found with page name " + pageName));
		}
		ToggleBackButtonActive(active: false);
	}

	private void Update()
	{
		foreach (IDebugToolInputProvider inputProvider in inputProviders)
		{
			if (inputProvider.GetToggle())
			{
				ToggleUIVisible(!UIVisible);
				break;
			}
			if (UIVisible && !SubpageConsumingNavActions)
			{
				if (inputProvider.GetNavLeft())
				{
					activeTabIndex--;
					if (activeTabIndex < 0)
					{
						activeTabIndex = totalTabs - 1;
					}
					ShowPage(tabButtons[activeTabIndex].PageName);
					break;
				}
				if (inputProvider.GetNavRight())
				{
					activeTabIndex++;
					if (activeTabIndex >= totalTabs)
					{
						activeTabIndex = 0;
					}
					ShowPage(tabButtons[activeTabIndex].PageName);
					break;
				}
			}
			if (!SubpageShowingDetails && inputProvider.GetCancel())
			{
				ToggleUIVisible(visible: false);
				break;
			}
		}
	}

	public void RegisterInputProvider(IDebugToolInputProvider provider)
	{
		inputProviders.Add(provider);
		DebugOverlayPage[] array = pages;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AddInputProvider(provider);
		}
	}

	public void RegisterKeyboardInputProvider(IDebugToolKeyboardInputProvider provider)
	{
		DebugOverlayPage[] array = pages;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetKeyboardInputProvider(provider);
		}
	}

	public void AddToggleActiveCallback(Action<bool> action)
	{
		OnToggle += action;
	}

	public void Initialize()
	{
		foreach (IDebugToolInputProvider inputProvider in inputProviders)
		{
			((MonoBehaviour)this).StartCoroutine(inputProvider.CoInitialize());
		}
	}

	public void ToggleUIVisible(bool visible)
	{
		if (rootUI.gameObject.activeSelf != visible)
		{
			rootUI.gameObject.SetActive(visible);
			OnToggle?.Invoke(visible);
			if (visible)
			{
				ShowPage(tabButtons[0].PageName);
			}
		}
	}

	private void ToggleBackButtonActive(bool active)
	{
		((Component)backButton).gameObject.SetActive(active);
		((Component)closeButton).gameObject.SetActive(!active);
	}
}
