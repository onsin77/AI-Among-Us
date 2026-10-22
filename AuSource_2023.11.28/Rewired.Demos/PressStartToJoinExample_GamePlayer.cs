using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController))]
public class PressStartToJoinExample_GamePlayer : MonoBehaviour
{
	public int gamePlayerId;

	public float moveSpeed = 3f;

	public float bulletSpeed = 15f;

	public GameObject bulletPrefab;

	private CharacterController cc;

	private Vector3 moveVector;

	private bool fire;

	private Player player => PressStartToJoinExample_Assigner.GetRewiredPlayer(gamePlayerId);

	private void OnEnable()
	{
		cc = ((Component)this).GetComponent<CharacterController>();
	}

	private void Update()
	{
		if (ReInput.isReady && player != null)
		{
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
