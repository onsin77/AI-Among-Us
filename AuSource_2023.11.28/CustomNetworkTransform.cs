using System.Collections.Generic;
using System.Linq;
using Hazel;
using InnerNet;
using InnerNet.GizmoHelpers;
using UnityEngine;

[DisallowMultipleComponent]
public class CustomNetworkTransform : InnerNetObject
{
	private const float SEND_MOVEMENT_THRESHOLD = 0.0001f;

	private const float REPLAY_POSITION_THRESHOLD = 0.003f;

	private const float NEUTRAL_BAND_MODIFIER = 0.995f;

	private const float SMOOTHING_BAND_MODIFIER = 0.5f;

	private const float SMOOTHING_LERP_RATE = 3f;

	private const int QUEUE_LENGTH_FOR_SNAPPING = 12;

	private const int QUEUE_LENGTH_FOR_SECOND_SNAP = 14;

	private const int QUEUE_THRESHOLD_FOR_SMOOTHING = 5;

	private const int SNAP_TO_SEQUENCE_ID_BUFFER = 2;

	private PlayerControl myPlayer;

	private Rigidbody2D body;

	private Queue<Vector2> sendQueue = new Queue<Vector2>();

	private Queue<Vector2> incomingPosQueue = new Queue<Vector2>();

	[SerializeField]
	private float rubberbandModifier = 1f;

	[SerializeField]
	private float idealSpeed;

	private bool isPaused;

	private ushort lastSequenceId;

	private Vector2 lastPosition;

	private Vector2 lastPosSent;

	private Vector2? tempSnapPosition;

	private ITransformGhost debugPopPositions = new NullTransformGhost();

	private ITransformGhost debugTargetPositions = new NullTransformGhost();

	private ITransformGhost debugTruePositions = new NullTransformGhost();

	private INetTransformLogger debugNetLogger = new NullNetLogger();

	private void Awake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		body = ((Component)this).GetComponent<Rigidbody2D>();
		myPlayer = ((Component)this).GetComponent<PlayerControl>();
		lastPosSent = Vector2.op_Implicit(((Component)this).transform.position);
		incomingPosQueue.Enqueue(Vector2.op_Implicit(((Component)this).transform.position));
	}

	public void OnDrawGizmos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		debugTargetPositions.Render(Color.red, crosses: true);
		debugPopPositions.Render(new Color(1f, 1f, 0f, 0.25f), crosses: false);
		debugTruePositions.Render(new Color(0f, 0f, 1f, 0.25f), crosses: false);
		debugNetLogger.Render(((Component)this).transform.position, rubberbandModifier);
	}

	public void OnEnable()
	{
		DummyBehaviour component = ((Component)this).GetComponent<DummyBehaviour>();
		if (Object.op_Implicit((Object)(object)component) && ((Behaviour)component).enabled)
		{
			((Behaviour)this).enabled = false;
		}
		SetDirtyBit(3u);
	}

	public void SetPaused(bool isPaused)
	{
		this.isPaused = isPaused;
	}

	public void Halt()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ushort minSid = (ushort)(lastSequenceId + 1);
		SnapTo(Vector2.op_Implicit(((Component)this).transform.position), minSid);
	}

	public void RpcSnapTo(Vector2 position)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (AmongUsClient.Instance.AmClient)
		{
			SnapTo(position, (ushort)(lastSequenceId + 1));
		}
		ushort num = (ushort)(lastSequenceId + 2);
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 21, (SendOption)1);
		NetHelpers.WriteVector2(position, val);
		val.Write(num);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void SnapTo(Vector2 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		ushort minSid = (ushort)(lastSequenceId + 2);
		SnapTo(position, minSid);
	}

	public void ClearPositionQueues()
	{
		if (AmOwner)
		{
			sendQueue.Clear();
		}
		else
		{
			incomingPosQueue.Clear();
		}
	}

	private bool IsInMiddleOfAnimationThatMakesPlayerInvisible()
	{
		if (!myPlayer.MyPhysics.Animations.IsPlayingEnterVentAnimation())
		{
			return myPlayer.walkingToVent;
		}
		return true;
	}

	private void SnapTo(Vector2 position, ushort minSid)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (NetHelpers.SidGreaterThan(minSid, lastSequenceId))
		{
			if (IsInMiddleOfAnimationThatMakesPlayerInvisible())
			{
				tempSnapPosition = position;
				return;
			}
			ClearPositionQueues();
			lastSequenceId = minSid;
			tempSnapPosition = null;
			Transform transform = ((Component)this).transform;
			Vector2 val = (body.position = position);
			transform.position = Vector2.op_Implicit(val);
			body.velocity = Vector2.zero;
		}
	}

	private void FixedUpdate()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (isPaused || !Object.op_Implicit((Object)(object)myPlayer))
		{
			return;
		}
		if (AmOwner)
		{
			if (HasMoved())
			{
				sendQueue.Enqueue(body.position);
				SetDirtyBit(2u);
			}
			return;
		}
		if (tempSnapPosition.HasValue && !IsInMiddleOfAnimationThatMakesPlayerInvisible())
		{
			SnapTo(tempSnapPosition.Value);
			tempSnapPosition = null;
		}
		if (incomingPosQueue.Count >= 1)
		{
			SkipExcessiveFrames();
			SetMovementSmoothingModifier();
			MoveTowardNextPoint();
		}
	}

	private bool HasMoved()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		num = ((!((Object)(object)body != (Object)null)) ? Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), lastPosSent) : Vector2.Distance(body.position, lastPosSent));
		if (num > 0.0001f)
		{
			return true;
		}
		return false;
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).isActiveAndEnabled && callId == 21)
		{
			Vector2 position = NetHelpers.ReadVector2(reader);
			ushort minSid = reader.ReadUInt16();
			SnapTo(position, minSid);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (isPaused)
		{
			return false;
		}
		if (initialState)
		{
			writer.Write(lastSequenceId);
			NetHelpers.WriteVector2(body.position, writer);
			return true;
		}
		if (!((Behaviour)this).isActiveAndEnabled)
		{
			ClearDirtyBits();
			return false;
		}
		if (sendQueue.Count == 0)
		{
			return false;
		}
		lastSequenceId++;
		writer.Write(lastSequenceId);
		ushort num = (ushort)sendQueue.Count;
		writer.WritePacked((int)num);
		foreach (Vector2 item in sendQueue)
		{
			NetHelpers.WriteVector2(item, writer);
			lastPosSent = item;
		}
		sendQueue.Clear();
		lastSequenceId += (ushort)(num - 1);
		DirtyBits--;
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (isPaused)
		{
			return;
		}
		if (initialState)
		{
			lastSequenceId = reader.ReadUInt16();
			((Component)this).transform.position = Vector2.op_Implicit(NetHelpers.ReadVector2(reader));
			incomingPosQueue.Clear();
			incomingPosQueue.Enqueue(Vector2.op_Implicit(((Component)this).transform.position));
		}
		else
		{
			if (AmOwner)
			{
				return;
			}
			ushort num = reader.ReadUInt16();
			int num2 = reader.ReadPackedInt32();
			Vector2 value = ((incomingPosQueue.Count <= 0) ? body.position : incomingPosQueue.Last());
			for (int i = 0; i < num2; i++)
			{
				ushort newSid = (ushort)(num + i);
				Vector2 val = NetHelpers.ReadVector2(reader);
				if (NetHelpers.SidGreaterThan(newSid, lastSequenceId))
				{
					lastSequenceId = newSid;
					incomingPosQueue.Enqueue(val);
					debugTargetPositions.AddPt(val);
					value = val;
				}
			}
			debugTruePositions.AddPt(Vector2.op_Implicit(((Component)this).transform.position));
			if (IsInMiddleOfAnimationThatMakesPlayerInvisible())
			{
				tempSnapPosition = value;
			}
			else
			{
				tempSnapPosition = null;
			}
		}
	}

	private void MoveTowardNextPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = incomingPosQueue.Peek();
		Vector2 position = body.position;
		Vector2 val2 = val - position;
		if (ShouldExtendCurrentFrame(val, position))
		{
			debugNetLogger.SetSlowbandingLog(isSlowbanding: true);
			Vector2 val3 = val2.normalized * idealSpeed * rubberbandModifier;
			val3 = Vector2.ClampMagnitude(val3, 10f);
			body.velocity = val3;
			lastPosition = body.position;
			return;
		}
		debugNetLogger.SetSlowbandingLog(isSlowbanding: false);
		if (incomingPosQueue.Count <= 1)
		{
			if (Vector2.Distance(body.position, val) > 0.01f)
			{
				body.position = val;
				body.velocity = Vector2.zero;
				lastPosition = body.position;
			}
			return;
		}
		debugPopPositions.AddPt(position);
		Vector2 val4 = incomingPosQueue.Dequeue();
		if (Vector2.Distance(body.position, val4) > 0.05f)
		{
			body.position = val4;
		}
		val = incomingPosQueue.Peek();
		position = body.position;
		val2 = val - position;
		idealSpeed = val2.magnitude / Time.fixedDeltaTime;
		Vector2 val5 = val2.normalized * idealSpeed * rubberbandModifier;
		val5 = Vector2.ClampMagnitude(val5, 10f);
		body.velocity = val5;
		lastPosition = body.position;
	}

	private void SkipExcessiveFrames()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (incomingPosQueue.Count < 12)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)body))
		{
			body.position = incomingPosQueue.Peek();
			MoveTowardNextPoint();
			if (incomingPosQueue.Count >= 14)
			{
				body.position = incomingPosQueue.Peek();
				MoveTowardNextPoint();
			}
		}
		else
		{
			((Component)this).transform.position = Vector2.op_Implicit(incomingPosQueue.Dequeue());
		}
	}

	private bool ShouldExtendCurrentFrame(Vector2 nextPos, Vector2 currentPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!DidPassPosition(nextPos, lastPosition, currentPos))
		{
			return incomingPosQueue.Count <= 5;
		}
		return false;
	}

	private bool DidPassPosition(Vector2 nextPos, Vector2 lastPos, Vector2 currentPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(lastPos, currentPos);
		float num2 = Vector2.Distance(currentPos, nextPos);
		float num3 = Vector2.Distance(lastPos, nextPos);
		return Mathf.Abs(num - (num3 + num2)) < 0.003f;
	}

	private void SetMovementSmoothingModifier()
	{
		float num = ((incomingPosQueue.Count <= 5) ? 0.5f : 0.995f);
		rubberbandModifier = Mathf.Lerp(rubberbandModifier, num, Time.fixedDeltaTime * 3f);
	}
}
