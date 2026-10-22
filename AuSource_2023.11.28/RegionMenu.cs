using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RegionMenu : MonoBehaviour
{
	public ObjectPoolBehavior ButtonPool;

	public TextMeshPro RegionText;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private UiElement defaultButtonSelected;

	private List<UiElement> controllerSelectable;

	private void Awake()
	{
		defaultButtonSelected = null;
		controllerSelectable = new List<UiElement>();
	}

	public void OnEnable()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected Obj, but got Unknown
		controllerSelectable.Clear();
		int num = 0;
		foreach (IRegionInfo item in DestroyableSingleton<ServerManager>.Instance.AvailableRegions.OrderBy((IRegionInfo s) => (uint)ServerManager.DefaultRegions.IndexOf((IRegionInfo d) => d.Name.Equals(s.Name))))
		{
			IRegionInfo region = item;
			ServerListButton serverListButton = ButtonPool.Get<ServerListButton>();
			((Component)serverListButton).transform.localPosition = new Vector3(0f, 2f - 0.5f * (float)num, 0f);
			serverListButton.SetTextTranslationId(item.TranslateName, item.Name);
			((TMP_Text)serverListButton.Text).text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(item.TranslateName, item.Name);
			((TMP_Text)serverListButton.Text).ForceMeshUpdate(false, false);
			((UnityEventBase)serverListButton.Button.OnClick).RemoveAllListeners();
			((UnityEvent)serverListButton.Button.OnClick).AddListener((UnityAction)(() =>
			{
				ChooseOption(region);
			}));
			serverListButton.SetSelected(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Equals(item));
			if (DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Equals(item))
			{
				defaultButtonSelected = serverListButton.Button;
			}
			controllerSelectable.Add(serverListButton.Button);
			num++;
		}
		if ((Object)(object)defaultButtonSelected == (Object)null && controllerSelectable.Count > 0)
		{
			defaultButtonSelected = controllerSelectable[0];
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, defaultButtonSelected, controllerSelectable);
		num++;
	}

	private void OpenCustomRegion()
	{
		throw new NotImplementedException();
	}

	public void OnDisable()
	{
		ButtonPool.ReclaimAll();
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void ChooseOption(IRegionInfo region)
	{
		DestroyableSingleton<ServerManager>.Instance.SetRegion(region);
		((TMP_Text)RegionText).text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(region.TranslateName, region.Name);
		Close();
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
