using System.Collections.Generic;
using UnityEngine;

public class MultiPrefabInstantiator : MonoBehaviour
{
	[SerializeField]
	private GameObject prefab;

	private List<GameObject> instances = new List<GameObject>();

	public int Count => instances.Count;

	public T Create<T>() where T : Component
	{
		GameObject val = Object.Instantiate<GameObject>(prefab, ((Component)this).transform);
		instances.Add(val);
		return val.GetComponent<T>();
	}

	public GameObject GetInstance(int index)
	{
		return instances[index];
	}

	public T GetInstanceComponent<T>(int index) where T : Component
	{
		return instances[index].GetComponent<T>();
	}

	public void DestroyInstance(GameObject obj)
	{
		instances.Remove(obj);
		Object.Destroy((Object)(object)obj);
	}

	public void DestroyAll()
	{
		foreach (GameObject instance in instances)
		{
			Object.Destroy((Object)(object)instance);
		}
		instances.Clear();
	}
}
