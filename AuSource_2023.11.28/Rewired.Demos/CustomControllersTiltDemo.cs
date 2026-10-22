using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class CustomControllersTiltDemo : MonoBehaviour
{
	public Transform target;

	public float speed = 10f;

	private CustomController controller;

	private Player player;

	private void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected Obj, but got Unknown
		Screen.orientation = (ScreenOrientation)3;
		player = ReInput.players.GetPlayer(0);
		ReInput.InputSourceUpdateEvent += OnInputUpdate;
		controller = (CustomController)player.controllers.GetControllerWithTag((ControllerType)20, "TiltController");
	}

	private void Update()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			Vector3 val = Vector3.zero;
			val.y = player.GetAxis("Tilt Vertical");
			val.x = player.GetAxis("Tilt Horizontal");
			if (val.sqrMagnitude > 1f)
			{
				val.Normalize();
			}
			val *= Time.deltaTime;
			target.Translate(val * speed);
		}
	}

	private void OnInputUpdate()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Vector3 acceleration = Input.acceleration;
		controller.SetAxisValue(0, acceleration.x);
		controller.SetAxisValue(1, acceleration.y);
		controller.SetAxisValue(2, acceleration.z);
	}
}
