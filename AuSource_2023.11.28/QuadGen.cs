using System.Collections.Generic;
using UnityEngine;

public class QuadGen : MonoBehaviour
{
	public float Width = 1f;

	public float Height = 1f;

	public int WidthSubdivisions;

	public int HeightSubdivisions;

	public float PerlinFreqX = 10f;

	[ContextMenu("Generate")]
	public void Generate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected Obj, but got Unknown
		//IL_0013: Expected Obj, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
		Mesh val = new Mesh();
		Mesh val2 = val;
		component.mesh = val;
		Mesh val3 = val2;
		int num = WidthSubdivisions + 2;
		int num2 = HeightSubdivisions + 2;
		Vector3[] array = new Vector3[num * num2];
		Vector2[] array2 = new Vector2[num * num2];
		Vector3 val4 = default;
		Vector2 val5 = default;
		for (int i = 0; i < num2; i++)
		{
			val4.y = Height * (0.5f - (float)i / ((float)HeightSubdivisions + 1f));
			for (int j = 0; j < num; j++)
			{
				val4.x = Width * (0.5f - (float)j / ((float)WidthSubdivisions + 1f));
				int num3 = j + i * num;
				array[num3] = val4;
				val5.y = Mathf.Lerp(-1f, 1f, Mathf.PerlinNoise((float)j * PerlinFreqX, (float)i));
				array2[num3] = val5;
			}
		}
		List<int> list = new List<int>();
		for (int k = 0; k < array.Length; k++)
		{
			if ((k + 1) % num != 0)
			{
				if (k + num >= array.Length)
				{
					break;
				}
				list.Add(k);
				list.Add(k + num);
				list.Add(k + num + 1);
				list.Add(k);
				list.Add(k + num + 1);
				list.Add(k + 1);
			}
		}
		val3.vertices = array;
		val3.uv = array2;
		val3.SetIndices(list.ToArray(), (MeshTopology)0, 0);
	}
}
