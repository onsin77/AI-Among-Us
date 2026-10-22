using System.Collections.Generic;
using UnityEngine;

public class LightSourceRaycastRenderer : LightSourceRenderer
{
	private class VertInfo
	{
		public float Angle;

		public Vector3 Position;

		internal void Complete(float x, float y)
		{
			Position.x = x;
			Position.y = y;
			Angle = PseudoAngle(y, x);
		}

		internal void Complete(Vector2 point)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			Position.x = point.x;
			Position.y = point.y;
			Angle = PseudoAngle(point.y, point.x);
		}
	}

	private class AngleComparer : IComparer<VertInfo>
	{
		public static readonly AngleComparer Instance = new AngleComparer();

		public int Compare(VertInfo x, VertInfo y)
		{
			if (!(x.Angle > y.Angle))
			{
				if (!(x.Angle < y.Angle))
				{
					return 0;
				}
				return -1;
			}
			return 1;
		}
	}

	private class HitDepthComparer : IComparer<RaycastHit2D>
	{
		public static readonly HitDepthComparer Instance = new HitDepthComparer();

		public int Compare(RaycastHit2D x, RaycastHit2D y)
		{
			if (!(x.fraction > y.fraction))
			{
				return -1;
			}
			return 1;
		}
	}

	private Camera camera;

	private List<VertInfo> verts = new List<VertInfo>(256);

	private int vertCount;

	private Vector3[] vec;

	private Vector2[] uvs;

	private int[] triangles = new int[1800];

	private RaycastHit2D[] buffer = new RaycastHit2D[50];

	private ContactFilter2D filter;

	private Collider2D[] hits = new Collider2D[100];

	private Vector2[] requiredDels;

	private Mesh myMesh;

	private Vector2 del;

	private Vector2 tan;

	private Vector2 side;

	private List<RaycastHit2D> lightHits = new List<RaycastHit2D>();

	public override LightSourceRendererType Type => LightSourceRendererType.Raycast;

	public LightSourceRaycastRenderer(LightSource lightSource)
		: base(lightSource)
	{
	}

	public override void Initialize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		myMesh = new Mesh();
		myMesh.MarkDynamic();
		((Object)myMesh).name = "ShadowMesh";
		filter.useTriggers = true;
		filter.layerMask = LayerMask.op_Implicit(Constants.ShadowMask);
		filter.useLayerMask = true;
		lightSource.LightChildMeshFilter.sharedMesh = myMesh;
		lightSource.LightCutawayMaterial.DisableKeyword("GPU_SHADOW");
		requiredDels = new Vector2[lightSource.RaycastMinimumRayCount];
		for (int i = 0; i < requiredDels.Length; i++)
		{
			requiredDels[i] = Vector2.left.Rotate((float)i / (float)requiredDels.Length * 360f);
		}
	}

	public override void Render(Vector2 origin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RaycastShadows(origin);
	}

	public override void Dispose()
	{
		lightSource.LightChildMeshFilter.sharedMesh = null;
		myMesh = null;
	}

	private float GetValidViewDistance()
	{
		if ((Object)(object)camera == (Object)null)
		{
			camera = Camera.main;
		}
		return Mathf.Min(lightSource.ViewDistance, camera.orthographicSize * camera.aspect);
	}

	private void RaycastShadows(Vector2 origin)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		vertCount = 0;
		lightSource.LightChild.transform.localScale = Vector3.one;
		int num = Physics2D.OverlapCircleNonAlloc(origin, GetValidViewDistance(), hits, Constants.ShadowMask);
		for (int i = 0; i < num; i++)
		{
			Collider2D val = hits[i];
			if (val.isTrigger)
			{
				continue;
			}
			EdgeCollider2D val2 = (EdgeCollider2D)(object)((val is EdgeCollider2D) ? val : null);
			if (Object.op_Implicit((Object)(object)val2))
			{
				Vector2[] points = val2.points;
				for (int j = 0; j < points.Length; j++)
				{
					Vector2 val3 = Vector2.op_Implicit(((Component)val2).transform.TransformPoint(Vector2.op_Implicit(points[j])));
					del.x = val3.x - origin.x;
					del.y = val3.y - origin.y;
					TestBothSides(origin);
				}
				continue;
			}
			PolygonCollider2D val4 = (PolygonCollider2D)(object)((val is PolygonCollider2D) ? val : null);
			if (Object.op_Implicit((Object)(object)val4))
			{
				Vector2[] points2 = val4.points;
				for (int k = 0; k < points2.Length; k++)
				{
					Vector2 val5 = Vector2.op_Implicit(((Component)val4).transform.TransformPoint(Vector2.op_Implicit(points2[k])));
					del.x = val5.x - origin.x;
					del.y = val5.y - origin.y;
					TestBothSides(origin);
				}
				continue;
			}
			BoxCollider2D val6 = (BoxCollider2D)(object)((val is BoxCollider2D) ? val : null);
			if (Object.op_Implicit((Object)(object)val6))
			{
				Vector2 val7 = val6.size / 2f;
				Vector2 val8 = Vector2.op_Implicit(((Component)val6).transform.TransformPoint(Vector2.op_Implicit(((Collider2D)val6).offset - val7))) - origin;
				Vector2 val9 = Vector2.op_Implicit(((Component)val6).transform.TransformPoint(Vector2.op_Implicit(((Collider2D)val6).offset + val7))) - origin;
				del.x = val8.x;
				del.y = val8.y;
				TestBothSides(origin);
				del.x = val9.x;
				TestBothSides(origin);
				del.y = val9.y;
				TestBothSides(origin);
				del.x = val8.x;
				TestBothSides(origin);
			}
		}
		float num2 = GetValidViewDistance() * 1.05f;
		for (int l = 0; l < requiredDels.Length; l++)
		{
			Vector2 val10 = num2 * requiredDels[l];
			CreateVert(origin, ref val10);
		}
		verts.Sort(0, vertCount, AngleComparer.Instance);
		myMesh.Clear();
		if (vec == null || vec.Length < vertCount + 1)
		{
			vec = new Vector3[vertCount + 1];
			uvs = new Vector2[vec.Length];
		}
		vec[0] = Vector3.zero;
		uvs[0] = new Vector2(vec[0].x, vec[0].y);
		for (int m = 0; m < vertCount; m++)
		{
			int num3 = m + 1;
			vec[num3] = verts[m].Position;
			uvs[num3] = new Vector2(vec[num3].x, vec[num3].y);
		}
		int num4 = vertCount * 3;
		if (num4 > triangles.Length)
		{
			triangles = new int[num4];
			Debug.LogWarning((object)("Resized triangles to: " + num4));
		}
		int num5 = 0;
		for (int n = 0; n < triangles.Length; n += 3)
		{
			if (n < num4)
			{
				triangles[n] = 0;
				triangles[n + 1] = num5 + 1;
				if (n == num4 - 3)
				{
					triangles[n + 2] = 1;
				}
				else
				{
					triangles[n + 2] = num5 + 2;
				}
				num5++;
			}
			else
			{
				triangles[n] = 0;
				triangles[n + 1] = 0;
				triangles[n + 2] = 0;
			}
		}
		myMesh.vertices = vec;
		myMesh.uv = uvs;
		myMesh.SetIndices(triangles, (MeshTopology)0, 0);
	}

	private void TestBothSides(Vector2 origin)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		float num = length(del.x, del.x);
		tan.x = (0f - del.y) / num * lightSource.RaycastTolerance;
		tan.y = del.x / num * lightSource.RaycastTolerance;
		side.x = del.x + tan.x;
		side.y = del.y + tan.y;
		CreateVert(origin, ref side);
		side.x = del.x - tan.x;
		side.y = del.y - tan.y;
		CreateVert(origin, ref side);
	}

	private void CreateVert(Vector2 origin, ref Vector2 del)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		float num = GetValidViewDistance() * 1.5f;
		int num2 = Physics2D.Raycast(origin, del, filter, buffer, num);
		if (num2 > 0)
		{
			lightHits.Clear();
			RaycastHit2D val = default;
			Collider2D val2 = null;
			for (int i = 0; i < num2; i++)
			{
				RaycastHit2D val3 = buffer[i];
				Collider2D collider = val3.collider;
				if (!LightSource.OneWayShadows.TryGetValue(((Component)collider).gameObject, out var value) || !value.IsIgnored(lightSource))
				{
					lightHits.Add(val3);
					if (!collider.isTrigger)
					{
						val = val3;
						val2 = collider;
						break;
					}
				}
			}
			for (int j = 0; j < lightHits.Count; j++)
			{
				RaycastHit2D val4 = lightHits[j];
				if (!(val4.distance > GetValidViewDistance()) && LightSource.NoShadows.TryGetValue(((Component)val4.collider).gameObject, out var value2))
				{
					value2.didHit = true;
				}
			}
			if (Object.op_Implicit((Object)(object)val2) && !val2.isTrigger)
			{
				Vector2 point = val.point;
				GetEmptyVert().Complete(point.x - origin.x, point.y - origin.y);
				return;
			}
		}
		Vector2 normalized = del.normalized;
		GetEmptyVert().Complete(normalized.x * num, normalized.y * num);
	}

	private VertInfo GetEmptyVert()
	{
		if (vertCount < verts.Count)
		{
			return verts[vertCount++];
		}
		VertInfo vertInfo = new VertInfo();
		verts.Add(vertInfo);
		vertCount = verts.Count;
		return vertInfo;
	}

	private static float length(float x, float y)
	{
		return Mathf.Sqrt(x * x + y * y);
	}

	public static float PseudoAngle(float dx, float dy)
	{
		if (dx < 0f)
		{
			float num = 0f - dx;
			float num2 = ((dy > 0f) ? dy : (0f - dy));
			return 2f - dy / (num + num2);
		}
		float num3 = ((dy > 0f) ? dy : (0f - dy));
		return dy / (dx + num3);
	}
}
