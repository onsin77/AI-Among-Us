using System.Collections;
using System.Collections.Generic;

namespace Innersloth.DebugTool;

public class DebugCircleBuffer<T> : IEnumerable<T>, IEnumerable
{
	private readonly T[] data;

	private int idx;

	private int count;

	public DebugCircleBuffer(int size)
	{
		data = new T[size];
		idx = 0;
		count = 0;
	}

	public void Add(T item)
	{
		data[idx] = item;
		idx++;
		if (count < idx)
		{
			count = idx;
		}
		if (idx >= data.Length)
		{
			idx = 0;
		}
	}

	public void Clear()
	{
		idx = 0;
		count = 0;
		for (int i = 0; i < data.Length; i++)
		{
			data[i] = default;
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		int i = 0;
		while (i < count)
		{
			yield return data[i];
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		int i = 0;
		while (i < count)
		{
			yield return data[i];
			int num = i + 1;
			i = num;
		}
	}
}
