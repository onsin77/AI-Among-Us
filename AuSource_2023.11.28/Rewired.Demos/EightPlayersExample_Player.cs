using System;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController))]
public class EightPlayersExample_Player : MonoBehaviour
{
	public int playerId;

	public float moveSpeed = 3f;

	public float bulletSpeed = 15f;

	public GameObject bulletPrefab;

	private Player player;

	private CharacterController cc;

	private Vector3 moveVector;

	private bool fire;

	[NonSerialized]
	private bool initialized;

	private void Awake()
	{
		cc = ((Component)this).GetComponent<CharacterController>();
	}

	private void Initialize()
	{
		player = ReInput.players.GetPlayer(playerId);
		initialized = true;
	}

	private void Update()
	{
		if (ReInput.isReady)
		{
			if (!initialized)
			{
				Initialize();
			}
			GetInput();
			ProcessInput();
		}
	}

	private void GetInput()
	{
		moveVector.x = player.GetAxis("Move Horizontal");
		moveVector.y = player.GetAxis("Move Vertical");
		fire = player.GetButtonDown("Fire");
	}

	private void ProcessInput()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (moveVector.x != 0f || moveVector.y != 0f)
		{
			cc.Move(moveVector * moveSpeed * Time.deltaTime);
		}
		if (fire)
		{
			Object.Instantiate<GameObject>(bulletPrefab, ((Component)this).transform.position + ((Component)this).transform.right, ((Component)this).transform.rotation).GetComponent<Rigidbody>().AddForce(((Component)this).transform.right * bulletSpeed, (ForceMode)2);
		}
	}
}
