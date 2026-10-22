using System;
using Hazel;
using UnityEngine;

namespace InnerNet;

public abstract class InnerNetObject : MonoBehaviour, IComparable<InnerNetObject>
{
	public uint SpawnId;

	public uint NetId;

	protected uint DirtyBits;

	public SpawnFlags SpawnFlags;

	public SendOption sendMode = (SendOption)1;

	public int OwnerId;

	protected bool DespawnOnDestroy = true;

	public virtual bool IsDirty => DirtyBits != 0;

	public virtual bool Chunked => false;

	public bool AmOwner => OwnerId == AmongUsClient.Instance.ClientId;

	public void Despawn()
	{
		AmongUsClient.Instance.Despawn(this);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public virtual void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance) && NetId != uint.MaxValue)
		{
			if (DespawnOnDestroy && (AmOwner || (OwnerId == -2 && AmongUsClient.Instance.AmHost)))
			{
				AmongUsClient.Instance.Despawn(this);
			}
			else
			{
				AmongUsClient.Instance.RemoveNetObject(this);
			}
		}
	}

	public abstract void HandleRpc(byte callId, MessageReader reader);

	public abstract bool Serialize(MessageWriter writer, bool initialState);

	public abstract void Deserialize(MessageReader reader, bool initialState);

	public int CompareTo(InnerNetObject other)
	{
		if (NetId > other.NetId)
		{
			return 1;
		}
		if (NetId < other.NetId)
		{
			return -1;
		}
		return 0;
	}

	protected bool IsDirtyBitSet(int idx)
	{
		return (DirtyBits & (uint)(1 << idx)) != 0;
	}

	protected void ClearDirtyBits()
	{
		DirtyBits = 0u;
	}

	protected void UnsetDirtyBit(uint val)
	{
		DirtyBits &= ~val;
	}

	protected void SetDirtyBit(uint val)
	{
		DirtyBits |= val;
	}

	protected InnerNetObject()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
	}
}
