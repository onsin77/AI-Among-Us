using System.Collections.Generic;
using System.Linq;
using Hazel.Udp;
using InnerNet;
using UnityEngine;

public class GameDiscovery : MonoBehaviour
{
	public JoinGameButton ButtonPrefab;

	public Transform ItemLocation;

	public float YStart = 0.56f;

	public float YOffset = -0.75f;

	public Scroller TargetArea;

	private Dictionary<string, JoinGameButton> received = new Dictionary<string, JoinGameButton>();

	public void Start()
	{
		InnerDiscover component = ((Component)this).GetComponent<InnerDiscover>();
		component.OnPacketGet += Receive;
		component.StartAsClient();
	}

	public void Update()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		string[] array = received.Keys.ToArray();
		Vector3 val = new Vector3(0f, YStart, -1f);
		foreach (string key in array)
		{
			JoinGameButton joinGameButton = received[key];
			if (time - joinGameButton.timeRecieved > 3f)
			{
				received.Remove(key);
				Object.Destroy((Object)(object)((Component)joinGameButton).gameObject);
			}
			else
			{
				((Component)joinGameButton).transform.localPosition = val;
				val.y += YOffset;
			}
		}
		TargetArea.SetBoundsMax(Mathf.Max(0f, 0f - val.y - 2f * YStart), 0f);
	}

	private void Receive(BroadcastPacket packet)
	{
		string[] array = packet.Data.Split('~');
		string address = packet.GetAddress();
		if (received.TryGetValue(address, out var value))
		{
			value.timeRecieved = Time.time;
			value.SetGameName(array);
		}
		else if (array[1].Equals("Open"))
		{
			CreateButtonForAddess(address, array);
		}
	}

	private void CreateButtonForAddess(string fromAddress, string[] gameNameParts)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		JoinGameButton value = null;
		bool flag = false;
		if (!received.TryGetValue(fromAddress, out value))
		{
			value = Object.Instantiate<JoinGameButton>(ButtonPrefab, ItemLocation);
			flag = true;
			Debug.Log((object)("GameDiscovery.CreateButtonForAddess: Instantiate(" + fromAddress + ")"));
		}
		if (flag)
		{
			((Component)value).transform.localPosition = new Vector3(0f, YStart + (float)(ItemLocation.childCount - 1) * YOffset, -1f);
			value.netAddress = fromAddress;
			((Renderer)((Component)value).GetComponentInChildren<MeshRenderer>()).material.SetInt("_Mask", 4);
		}
		value.timeRecieved = Time.time;
		value.SetGameName(gameNameParts);
		if (flag)
		{
			ControllerManager.Instance.AddSelectableUiElement(((Component)value).GetComponent<PassiveButton>());
		}
		received[fromAddress] = value;
	}
}
