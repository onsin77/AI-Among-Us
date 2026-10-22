using System;
using AmongUs.Data;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class CrossplayConditionalObject : MonoBehaviour
{
	public enum CrossplayConditionalType
	{
		ShowIfCrossplayEnabled,
		HideIfCrossplayEnabled
	}

	public bool onlyOnPlayStation = true;

	public CrossplayConditionalType crossplayConditionalType;

	public static Action OnCrossplayChanged;

	private void Awake()
	{
		if (!onlyOnPlayStation)
		{
			DataManager.Settings.Multiplayer.OnCrossPlayChanged += UpdateEnabledState;
		}
		UpdateEnabledState();
	}

	private void OnDestroy()
	{
		if (!onlyOnPlayStation)
		{
			DataManager.Settings.Multiplayer.OnCrossPlayChanged -= UpdateEnabledState;
		}
	}

	private void UpdateEnabledState()
	{
		bool active = false;
		switch (crossplayConditionalType)
		{
		case CrossplayConditionalType.ShowIfCrossplayEnabled:
			active = DataManager.Settings.Multiplayer.CrossPlay;
			break;
		case CrossplayConditionalType.HideIfCrossplayEnabled:
			active = !DataManager.Settings.Multiplayer.CrossPlay;
			break;
		}
		((Component)this).gameObject.SetActive(active);
	}
}
