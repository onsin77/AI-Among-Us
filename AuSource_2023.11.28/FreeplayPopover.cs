using System.Linq;
using UnityEngine;

public class FreeplayPopover : MonoBehaviour
{
	[SerializeField]
	public GameObject content;

	[SerializeField]
	public GameObject background;

	[SerializeField]
	private FreeplayPopoverButton[] buttons;

	[SerializeField]
	[Space(10f)]
	private HostLocalGameButton hostGameButton;

	[SerializeField]
	[Header("Console Controller Navigation")]
	private UiElement backButton;

	private void Awake()
	{
		background.SetActive(false);
		content.SetActive(false);
	}

	public void Show()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].OnPressEvent += OnMapButtonPressed;
		}
		content.SetActive(true);
		background.SetActive(true);
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, backButton, buttons[0].Button, (from x in buttons
			where ((Component)x).gameObject.activeSelf
			select x.Button).ToList());
	}

	public void Close()
	{
		content.SetActive(false);
		background.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void OnMapButtonPressed(FreeplayPopoverButton button)
	{
		PlayMap(button.Map);
	}

	private void PlayMap(MapNames map)
	{
		AmongUsClient.Instance.TutorialMapId = (int)map;
		hostGameButton.OnClick();
	}
}
