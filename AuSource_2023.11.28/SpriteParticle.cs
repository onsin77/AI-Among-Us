using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SpriteParticle : MonoBehaviour
{
	private const float FrameRate = 24f;

	public Sprite[] Sprites;

	public ParticleInfo[] Particles;

	public ushort[][] TriangleCache;

	private Vector3[] verts;

	private Vector2[] uvs;

	private List<int> tris = new List<int>();

	private Mesh mesh;

	private int MaxVerts;

	private Dictionary<int, Vector2[]> VertCache = new Dictionary<int, Vector2[]>();

	private Dictionary<int, Vector2[]> UvCache = new Dictionary<int, Vector2[]>();

	public void OnDrawGizmos()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (Particles != null && Sprites != null && Sprites.Length != 0)
		{
			Sprite val = Sprites[0];
			for (int i = 0; i < Particles.Length; i++)
			{
				ParticleInfo particleInfo = Particles[i];
				Vector3 val2 = particleInfo.Position + ((Component)this).transform.position;
				Bounds bounds = val.bounds;
				float num = bounds.size.x * particleInfo.Scale;
				bounds = val.bounds;
				float num2 = bounds.size.y * particleInfo.Scale;
				bounds = val.bounds;
				Gizmos.DrawCube(val2, new Vector3(num, num2, bounds.size.x * particleInfo.Scale));
			}
		}
	}

	public void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
		mesh = new Mesh();
		mesh.MarkDynamic();
		component.mesh = mesh;
		TriangleCache = new ushort[Sprites.Length][];
		for (int i = 0; i < Sprites.Length; i++)
		{
			Sprite val = Sprites[i];
			Vector2[] vertices = val.vertices;
			TriangleCache[i] = val.triangles;
			VertCache[i] = vertices;
			UvCache[i] = val.uv;
			if (MaxVerts < vertices.Length)
			{
				MaxVerts = vertices.Length;
			}
		}
		verts = new Vector3[Particles.Length * MaxVerts];
		uvs = new Vector2[verts.Length];
		for (int j = 0; j < Particles.Length; j++)
		{
			int num = j * MaxVerts;
			int num2 = (int)Particles[j].Timer;
			Sprite val2 = Sprites[num2];
			Vector2[] vertices2 = val2.vertices;
			Vector2[] uv = val2.uv;
			for (int k = 0; k < val2.vertices.Length; k++)
			{
				int num3 = num + k;
				verts[num3].x = vertices2[k].x * Particles[j].Scale + Particles[j].Position.x;
				verts[num3].y = vertices2[k].y * Particles[j].Scale + Particles[j].Position.y;
				uvs[num3] = uv[k];
			}
			ushort[] triangles = val2.triangles;
			for (int l = 0; l < triangles.Length; l++)
			{
				tris.Add(triangles[l]);
			}
		}
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.SetTriangles(tris, 0);
	}

	public void Update()
	{
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.deltaTime * 24f;
		tris.Clear();
		for (int i = 0; i < Particles.Length; i++)
		{
			float num2 = Particles[i].Timer + num;
			if (num2 >= (float)Sprites.Length)
			{
				num2 %= 24f;
			}
			Particles[i].Timer = num2;
			int num3 = i * MaxVerts;
			int num4 = (int)Particles[i].Timer;
			Vector2[] array = VertCache[num4];
			Vector2[] array2 = UvCache[num4];
			for (int j = 0; j < array.Length; j++)
			{
				int num5 = num3 + j;
				verts[num5].x = array[j].x * Particles[i].Scale + Particles[i].Position.x;
				verts[num5].y = array[j].y * Particles[i].Scale + Particles[i].Position.y;
				uvs[num5] = array2[j];
			}
			ushort[] array3 = TriangleCache[num4];
			for (int k = 0; k < array3.Length; k++)
			{
				tris.Add(array3[k] + num3);
			}
		}
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.SetTriangles(tris, 0);
	}
}
