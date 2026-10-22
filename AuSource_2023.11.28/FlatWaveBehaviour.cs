using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FlatWaveBehaviour : MonoBehaviour
{
	public int NumPoints = 128;

	public FloatRange Width;

	public FloatRange Delta;

	public float Center;

	private Mesh mesh;

	private Vector3[] vecs;

	public float TickRate = 0.1f;

	private float timer;

	public int Skip = 3;

	[Range(0f, 1f)]
	public float NoiseP = 0.5f;

	public void Start()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		mesh = new Mesh();
		((Component)this).GetComponent<MeshFilter>().mesh = mesh;
		mesh.MarkDynamic();
		vecs = new Vector3[NumPoints];
		int[] array = new int[NumPoints];
		for (int i = 0; i < vecs.Length; i++)
		{
			Vector3 val = vecs[i];
			val.x = Width.Lerp((float)i / (float)vecs.Length);
			val.y = Center;
			if (BoolRange.Next(NoiseP))
			{
				val.y += Delta.Next();
			}
			vecs[i] = val;
			array[i] = i;
		}
		mesh.vertices = vecs;
		mesh.SetIndices(array, (MeshTopology)4, 0);
	}

	public void Update()
	{
		timer += Time.deltaTime;
		if (!(timer > TickRate))
		{
			return;
		}
		timer = 0f;
		for (int i = 0; i < vecs.Length - Skip; i++)
		{
			vecs[i].y = vecs[i + Skip].y;
		}
		for (int j = 1; j <= Skip; j++)
		{
			vecs[vecs.Length - j].y = Center;
			if (BoolRange.Next(NoiseP))
			{
				vecs[vecs.Length - j].y += Delta.Next();
			}
		}
		mesh.vertices = vecs;
	}
}
