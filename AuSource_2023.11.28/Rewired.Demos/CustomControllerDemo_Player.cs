using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController))]
public class CustomControllerDemo_Player : MonoBehaviour
{
	public int playerId;

	public float speed = 1f;

	public float bulletSpeed = 20f;

	public GameObject bulletPrefab;

	private Player _player;

	private CharacterController cc;

	private Player player
	{
		get
		{
			if (_player == null)
			{
				_player = ReInput.players.GetPlayer(playerId);
			}
			return _player;
		}
	}

	private void Awake()
	{
		cc = ((Component)this).GetComponent<CharacterController>();
	}

	private void Update()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		if (ReInput.isReady)
		{
			Vector2 val = new Vector2(player.GetAxis("Move Horizontal"), player.GetAxis("Move Vertical"));
			cc.Move(Vector2.op_Implicit(val * speed * Time.deltaTime));
			if (player.GetButtonDown("Fire"))
			{
				Vector3 val2 = Vector3.Scale(new Vector3(1f, 0f, 0f), ((Component)this).transform.right);
				Object.Instantiate<GameObject>(bulletPrefab, ((Component)this).transform.position + val2, Quaternion.identity).GetComponent<Rigidbody>().velocity = new Vector3(bulletSpeed * ((Component)this).transform.right.x, 0f, 0f);
			}
			if (player.GetButtonDown("Change Color"))
			{
				Renderer component = ((Component)this).GetComponent<Renderer>();
				Material material = component.material;
				material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
				component.material = material;
			}
		}
	}
}
