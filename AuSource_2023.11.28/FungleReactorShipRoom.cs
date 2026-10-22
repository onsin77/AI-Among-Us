using UnityEngine;

public class FungleReactorShipRoom : PlainShipRoom, IReactorShipRoom
{
	[SerializeField]
	private GameObject reactorCableLeft;

	[SerializeField]
	private GameObject reactorCableRight;

	[SerializeField]
	private GameObject reactorMain;

	[SerializeField]
	private GameObject reactorMainBroken;

	private void Awake()
	{
		StopMeltdown();
	}

	public void StartMeltdown()
	{
		reactorMain.SetActive(false);
		reactorMainBroken.SetActive(true);
		reactorCableLeft.SetActive(true);
		reactorCableRight.SetActive(true);
	}

	public void StopMeltdown()
	{
		reactorMain.SetActive(true);
		reactorMainBroken.SetActive(false);
		reactorCableLeft.SetActive(false);
		reactorCableRight.SetActive(false);
	}
}
