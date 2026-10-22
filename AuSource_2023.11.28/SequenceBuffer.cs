using System;
using System.Collections.Generic;

public class SequenceBuffer<T>
{
	private struct SequencedData<T>(ushort sid, T data) : IComparable<SequencedData<T>>
	{
		public readonly ushort Sid = sid;

		public readonly T Data = data;

		public int CompareTo(SequencedData<T> other)
		{
			return Sid.CompareTo(other.Sid);
		}
	}

	private readonly List<SequencedData<T>> buffer = new List<SequencedData<T>>();

	public ushort LastSid { get; set; }

	public SequenceBuffer(ushort sidStart = 0)
	{
		LastSid = sidStart;
	}

	public void Add(ushort sid, T info)
	{
		buffer.Add(new SequencedData<T>(sid, info));
	}

	public void BumpSid()
	{
		LastSid++;
	}

	public bool IsInvalidSid(ushort sid)
	{
		return !NetHelpers.SidGreaterThan(sid, LastSid);
	}

	public bool IsNextSid(ushort sid)
	{
		return sid == (ushort)(LastSid + 1);
	}

	public IEnumerable<T> SubsequentObjs()
	{
		Sort();
		while (HasElements() && IsNextSid(Peek().Sid))
		{
			yield return Pop().Data;
		}
	}

	private void Sort()
	{
		buffer.Sort();
	}

	private bool HasElements()
	{
		return buffer.Count > 0;
	}

	private SequencedData<T> Pop()
	{
		if (!HasElements())
		{
			throw new InvalidOperationException("No elements to pop");
		}
		SequencedData<T> result = Peek();
		buffer.RemoveAt(0);
		return result;
	}

	private SequencedData<T> Peek()
	{
		if (!HasElements())
		{
			throw new InvalidOperationException("No elements to peek");
		}
		return buffer[0];
	}
}
