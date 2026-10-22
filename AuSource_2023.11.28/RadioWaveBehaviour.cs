using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RadioWaveBehaviour : MonoBehaviour
{
	public int NumPoints = 128;

	public FloatRange Width;

	public FloatRange Height;

	private Mesh mesh;

	private Vector3[] vecs;

	public float TickRate = 0.1f;

	private float timer;

	public int Skip = 2;

	public float Frequency = 5f;

	private int Tick;

	public bool Random;

	[Range(0f, 1f)]
	public float NoiseLevel;

	public void Start()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		mesh = new Mesh();
		((Component)this).GetComponent<MeshFilter>().mesh = mesh;
		mesh.MarkDynamic();
		vecs = new Vector3[NumPoints];
		int[] array = new int[NumPoints];
		for (int i = 0; i < vecs.Length; i++)
		{
			Vector3 val = vecs[i];
			val.x = Width.Lerp((float)i / (float)vecs.Length);
			val.y = Height.Next();
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
		Tick++;
		for (int i = 0; i < vecs.Length - Skip; i++)
		{
			vecs[i].y = vecs[i + Skip].y;
		}
		if (Random)
		{
			for (int j = 1; j <= Skip; j++)
			{
				vecs[vecs.Length - j].y = Height.Next();
			}
		}
		else
		{
			float num = 1f - NoiseLevel;
			for (int k = 0; k < Skip; k++)
			{
				vecs[vecs.Length - 1 - Skip + k].y = Mathf.Sin(((float)Tick + (float)k / (float)Skip) * Frequency) * Height.max * num + Height.Next() * NoiseLevel;
			}
		}
		mesh.vertices = vecs;
	}
}
