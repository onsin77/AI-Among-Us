using System.Collections;
using UnityEngine;

public class KillAnimation : MonoBehaviour
{
	public AnimationClip BlurAnim;

	public Vector3 BodyOffset;

	public IEnumerator CoPerformKill(PlayerControl source, PlayerControl target)
	{
		FollowerCamera cam = ((Component)Camera.main).GetComponent<FollowerCamera>();
		bool isParticipant = (Object)(object)PlayerControl.LocalPlayer == (Object)(object)source || (Object)(object)PlayerControl.LocalPlayer == (Object)(object)target;
		PlayerPhysics sourcePhys = source.MyPhysics;
		SetMovement(source, canMove: false);
		SetMovement(target, canMove: false);
		if (isParticipant)
		{
			PlayerControl.LocalPlayer.isKilling = true;
			source.isKilling = true;
		}
		DeadBody deadBody = Object.Instantiate<DeadBody>(GameManager.Instance.DeadBodyPrefab);
		((Behaviour)deadBody).enabled = false;
		deadBody.ParentId = target.PlayerId;
		deadBody.bodyRenderers.ForEach((SpriteRenderer b) =>
		{
			target.SetPlayerMaterialColors((Renderer)(object)b);
		});
		target.SetPlayerMaterialColors((Renderer)(object)deadBody.bloodSplatter);
		Vector3 val = ((Component)target).transform.position + BodyOffset;
		val.z = val.y / 1000f;
		((Component)deadBody).transform.position = val;
		if (isParticipant)
		{
			cam.Locked = true;
			ConsoleJoystick.SetMode_Task();
			if (PlayerControl.LocalPlayer.AmOwner)
			{
				((Behaviour)PlayerControl.LocalPlayer.MyPhysics.inputHandler).enabled = true;
			}
		}
		target.Die(DeathReason.Kill, assignGhostRole: true);
		yield return source.MyPhysics.Animations.CoPlayCustomAnimation(BlurAnim);
		source.NetTransform.SnapTo(Vector2.op_Implicit(((Component)target).transform.position));
		sourcePhys.Animations.PlayIdleAnimation();
		SetMovement(source, canMove: true);
		SetMovement(target, canMove: true);
		((Behaviour)deadBody).enabled = true;
		if (isParticipant)
		{
			cam.Locked = false;
			PlayerControl.LocalPlayer.isKilling = false;
			source.isKilling = false;
		}
	}

	public static void SetMovement(PlayerControl source, bool canMove)
	{
		source.moveable = canMove;
		source.MyPhysics.ResetMoveState(stopCoroutines: false);
		((Behaviour)source.NetTransform).enabled = canMove;
		((Behaviour)source.MyPhysics).enabled = canMove;
		source.NetTransform.Halt();
	}
}
