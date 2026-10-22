using UnityEngine;

public class DeadBody : MonoBehaviour
{
	public bool Reported;

	public byte ParentId;

	public Collider2D myCollider;

	public SpriteRenderer bloodSplatter;

	public SpriteRenderer[] bodyRenderers;

	public Vector2 TruePosition
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			return Vector2.op_Implicit(((Component)this).transform.TransformPoint(Vector2.op_Implicit(myCollider.offset)));
		}
	}

	public void OnClick()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (!Reported && GameManager.Instance.CanReportBodies())
		{
			Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
			Vector2 truePosition2 = TruePosition;
			if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, useTriggers: false))
			{
				Reported = true;
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(ParentId);
				PlayerControl.LocalPlayer.CmdReportDeadBody(playerById);
			}
		}
	}
}
