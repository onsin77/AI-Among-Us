using UnityEngine;

public class CreditsScreenPopUp : MonoBehaviour
{
	[SerializeField]
	private TransitionOpen TransitionOpen;

	[SerializeField]
	private CreditsController CreditsController;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu("Credits", BackButton);
		CreditsController.OnFinish += TransitionOpen.Close;
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu("Credits");
		CreditsController.OnFinish -= TransitionOpen.Close;
	}
}
