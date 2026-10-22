using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CircleBuffer<T> : IEnumerable<T>, IEnumerable where T : class
{
	private T[] data;

	private int idx;

	private int count;

	public CircleBuffer(int size)
	{
		data = new T[size];
	}

	public void Sort(IComparer<T> comparer)
	{
		Array.Sort(data, 0, count, comparer);
		if (count >= data.Length)
		{
			idx = 0;
		}
	}

	public void Add(T item)
	{
		lock (this)
		{
			for (int i = 0; i < count; i++)
			{
				if (data[idx] == item)
				{
					return;
				}
			}
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
	}

	public void Clear()
	{
		idx = 0;
		count = 0;
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

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		T[] array = data;
		foreach (T value in array)
		{
			stringBuilder.Append(value).Append('\n');
		}
		return stringBuilder.ToString();
	}
}
