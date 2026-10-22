using System;
using System.Collections.Generic;

public class PseudoRandomList<T>
{
	private List<T> options = new List<T>();

	private List<T> currentRemaining = new List<T>();

	private XXHash rand;

	private int offset;

	public PseudoRandomList()
	{
		rand = new XXHash((int)DateTime.UtcNow.Ticks);
	}

	public PseudoRandomList(int seed)
	{
		rand = new XXHash(seed);
	}

	public void ClearWithNewSeed(int seed)
	{
		rand = new XXHash(seed);
		offset = 0;
		Clear();
	}

	public void Add(T item)
	{
		options.Add(item);
	}

	public void AddRange(IEnumerable<T> list)
	{
		options.AddRange(list);
	}

	public void Clear()
	{
		options.Clear();
		currentRemaining.Clear();
	}

	public T PickRandom()
	{
		if (currentRemaining.Count <= 0)
		{
			currentRemaining.AddRange(options);
		}
		int index = (int)(rand.GetHash(offset++) % currentRemaining.Count);
		T val = currentRemaining[index];
		currentRemaining.RemoveAt(index);
		if (currentRemaining.Count <= 0)
		{
			currentRemaining.AddRange(options);
			currentRemaining.Remove(val);
		}
		return val;
	}
}
