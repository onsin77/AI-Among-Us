using Hazel;
using InnerNet;
using UnityEngine;

public class GameOptionsManagerCreator : MonoBehaviour
{
	private void Awake()
	{
		new GameOptionsManager((ILogger)(object)new UnityLogger());
	}

	private void Start()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
