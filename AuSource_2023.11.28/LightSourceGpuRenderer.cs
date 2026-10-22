using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightSourceGpuRenderer : LightSourceRenderer
{
	public delegate void AddEdgeDelegate(Vector3 a, Vector3 b);

	private const float TWO_PI = (float)Math.PI * 2f;

	private Mesh occluderMesh;

	private List<Vector3> occVerts = new List<Vector3>();

	private List<Vector3> occNorms = new List<Vector3>();

	private List<Vector2> occUVs = new List<Vector2>();

	private List<int> occTris = new List<int>();

	private Collider2D[] hits = new Collider2D[100];

	private Material shadowCasterMaterial;

	private CommandBuffer cb;

	private bool noGeomSupport;

	private RenderTexture shadowTexture;

	private AddEdgeDelegate AddEdge;

	public override LightSourceRendererType Type => LightSourceRendererType.GPU;

	public LightSourceGpuRenderer(LightSource lightSource)
		: base(lightSource)
	{
	}

	public override void Initialize()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected Obj, but got Unknown
		if (cb == null)
		{
			cb = new CommandBuffer();
			cb.name = "Draw occlusion";
		}
		GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
		if ((int)graphicsDeviceType == 8 || (int)graphicsDeviceType == 16)
		{
			noGeomSupport = true;
			AddEdge = AddEdgeNoGeom;
			Logger.GlobalInstance.Info("Graphics API has no geometry shader support, switching to CPU workaround");
		}
		else
		{
			noGeomSupport = false;
			AddEdge = AddEdgeDefault;
			Logger.GlobalInstance.Info("Graphics API has geometry shader support");
		}
		if (!noGeomSupport && !SystemInfo.supportsGeometryShaders)
		{
			noGeomSupport = true;
			AddEdge = AddEdgeNoGeom;
			Logger.GlobalInstance.Info("Graphics hardware has no geometry shader support, switching to CPU workaround");
		}
		lightSource.LightChildMeshFilter.sharedMesh = lightSource.LightChildMesh;
		lightSource.LightCutawayMaterial.EnableKeyword("GPU_SHADOW");
		occluderMesh = new Mesh();
		occluderMesh.MarkDynamic();
		((Object)occluderMesh).name = "Occluder Mesh";
		GenerateShadowmap();
	}

	public override void Render(Vector2 origin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GPUShadows(origin);
	}

	public override void Dispose()
	{
		if (Object.op_Implicit((Object)(object)shadowTexture))
		{
			shadowTexture.Release();
			shadowTexture = null;
		}
	}

	private void GenerateShadowmap()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected Obj, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected Obj, but got Unknown
		if (SystemInfo.SupportsRenderTextureFormat(lightSource.GpuPreferredRTFormat))
		{
			shadowTexture = new RenderTexture(3, lightSource.GpuShadowmapResolution, 16, lightSource.GpuPreferredRTFormat);
		}
		else
		{
			shadowTexture = new RenderTexture(3, lightSource.GpuShadowmapResolution, 16, (RenderTextureFormat)16);
		}
		((Texture)shadowTexture).wrapModeU = (TextureWrapMode)1;
		((Texture)shadowTexture).wrapModeV = (TextureWrapMode)0;
		((Texture)shadowTexture).filterMode = (FilterMode)1;
		shadowTexture.Create();
		shadowCasterMaterial = new Material(lightSource.GpuShadowCasterMaterial);
	}

	private void GPUShadows(Vector2 origin)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		ClearEdges();
		lightSource.LightChild.transform.localScale = new Vector3(lightSource.ViewDistance * 3f, lightSource.ViewDistance * 3f, 1f);
		Camera main = Camera.main;
		Vector2 val = Vector2.op_Implicit(((Component)main).transform.position - ((Component)lightSource).transform.position);
		Vector2 val2 = new Vector2(main.orthographicSize * main.aspect, main.orthographicSize);
		float num = val2.magnitude + val.magnitude;
		float num2 = Mathf.Min(lightSource.ViewDistance, num);
		foreach (NoShadowBehaviour value3 in LightSource.NoShadows.Values)
		{
			value3.CheckHit(num2, origin);
		}
		int num3 = Physics2D.OverlapCircleNonAlloc(origin, num2, hits, Constants.ShadowMask);
		for (int i = 0; i < num3; i++)
		{
			Collider2D val3 = hits[i];
			if (val3.isTrigger || (LightSource.NoShadows.TryGetValue(((Component)val3).gameObject, out var value) && (Object)(object)value.hitOverride == (Object)(object)val3) || (LightSource.OneWayShadows.TryGetValue(((Component)val3).gameObject, out var value2) && value2.IsIgnored(lightSource)))
			{
				continue;
			}
			EdgeCollider2D val4 = (EdgeCollider2D)(object)((val3 is EdgeCollider2D) ? val3 : null);
			if (Object.op_Implicit((Object)(object)val4))
			{
				Vector2[] points = val4.points;
				for (int j = 0; j < points.Length - 1; j++)
				{
					Vector3 a = ((Component)val4).transform.TransformPoint(Vector2.op_Implicit(points[j]));
					Vector3 b = ((Component)val4).transform.TransformPoint(Vector2.op_Implicit(points[j + 1]));
					AddEdge(a, b);
				}
				continue;
			}
			PolygonCollider2D val5 = (PolygonCollider2D)(object)((val3 is PolygonCollider2D) ? val3 : null);
			if (Object.op_Implicit((Object)(object)val5))
			{
				Vector2[] points2 = val5.points;
				for (int k = 0; k < points2.Length; k++)
				{
					int num4 = k + 1;
					if (num4 == points2.Length)
					{
						num4 = 0;
					}
					Vector3 a2 = ((Component)val5).transform.TransformPoint(Vector2.op_Implicit(points2[k]));
					Vector3 b2 = ((Component)val5).transform.TransformPoint(Vector2.op_Implicit(points2[num4]));
					AddEdge(a2, b2);
				}
				continue;
			}
			BoxCollider2D val6 = (BoxCollider2D)(object)((val3 is BoxCollider2D) ? val3 : null);
			if (Object.op_Implicit((Object)(object)val6))
			{
				Vector2 val7 = val6.size / 2f;
				Vector2 val8 = Vector2.op_Implicit(((Component)val6).transform.TransformPoint(Vector2.op_Implicit(((Collider2D)val6).offset - val7)));
				Vector2 val9 = Vector2.op_Implicit(((Component)val6).transform.TransformPoint(Vector2.op_Implicit(((Collider2D)val6).offset + val7)));
				Vector3 a3 = Vector2.op_Implicit(val8);
				Vector3 b3 = Vector2.op_Implicit(val8);
				b3.y = val9.y;
				AddEdge(a3, b3);
				a3.y = val9.y;
				b3.x = val9.x;
				AddEdge(a3, b3);
				a3 = Vector2.op_Implicit(val9);
				b3.y = val8.y;
				AddEdge(a3, b3);
				a3.y = val8.y;
				b3 = Vector2.op_Implicit(val8);
				AddEdge(a3, b3);
			}
		}
		UpdateOccMesh();
		DrawOcclusion(num2);
	}

	private void ClearEdges()
	{
		occluderMesh.Clear();
		occVerts.Clear();
		occNorms.Clear();
		if (noGeomSupport)
		{
			occUVs.Clear();
		}
		occTris.Clear();
	}

	private void UpdateOccMesh()
	{
		occluderMesh.SetVertices(occVerts);
		occluderMesh.SetNormals(occNorms);
		if (noGeomSupport)
		{
			occluderMesh.SetUVs(0, occUVs);
		}
		occluderMesh.SetTriangles(occTris, 0, occTris.Count, 0, true, 0);
	}

	private void DrawOcclusion(float effectiveRadius)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)shadowTexture) && Object.op_Implicit((Object)(object)shadowCasterMaterial))
		{
			float num = ((Texture)shadowTexture).width;
			shadowCasterMaterial.SetFloat("_DepthCompressionValue", effectiveRadius);
			cb.Clear();
			cb.SetRenderTarget(RenderTargetIdentifier.op_Implicit((Texture)(object)shadowTexture));
			cb.ClearRenderTarget(true, true, new Color(1f, 1f, 1f, 1f));
			cb.SetGlobalTexture("_ShmapTexture", RenderTargetIdentifier.op_Implicit((Texture)(object)shadowTexture));
			cb.SetGlobalFloat("_Radius", lightSource.ViewDistance);
			cb.SetGlobalFloat("_Column", 0f);
			cb.SetGlobalVector("_LightPosition", Vector4.op_Implicit(((Component)lightSource).transform.position));
			cb.SetGlobalVector("_TexelSize", new Vector4(1f / num, 1f / num, num, num));
			cb.SetGlobalFloat("_DepthCompressionValue", effectiveRadius);
			cb.DrawMesh(occluderMesh, Matrix4x4.identity, shadowCasterMaterial);
			Graphics.ExecuteCommandBuffer(cb);
		}
	}

	private void DebugDrawRadius(Vector2 center, float radius)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(20, (int)(20f * radius));
		Vector2 val = center + Vector2.right * radius;
		for (int i = 1; i <= num; i++)
		{
			float num2 = (float)i / (float)num * (float)Math.PI * 2f;
			Vector2 val2 = center + new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)) * radius;
			Debug.DrawLine(Vector2.op_Implicit(val), Vector2.op_Implicit(val2), Color.yellow);
			val = val2;
		}
	}

	private void AddEdgeNoGeom(Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(a - ((Component)lightSource).transform.position);
		Vector2 val2 = Vector2.op_Implicit(b - ((Component)lightSource).transform.position);
		float num = Mathf.Atan2(val.y, val.x);
		float num2 = Mathf.Atan2(val2.y, val2.x);
		float num3 = num + ((num < 0f) ? ((float)Math.PI * 2f) : 0f);
		num2 += ((num2 < 0f) ? ((float)Math.PI * 2f) : 0f);
		if (Mathf.Abs(num3 - num2) > (float)Math.PI)
		{
			AddEdgeInternal(a, b, new Vector2(0f, 0f));
			AddEdgeInternal(a, b, new Vector2(0f, (float)Math.PI * 2f));
		}
		else
		{
			AddEdgeInternal(a, b, new Vector2(1f, 0f));
		}
	}

	private void AddEdgeInternal(Vector3 a, Vector3 b, Vector2 uv)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		a.z = 0f;
		b.z = 0f;
		Vector3 val = b - a;
		val = new Vector3(val.y, 0f - val.x, 0f);
		Vector3 item = a + Vector3.forward * 0.5f;
		Vector3 item2 = a - Vector3.forward * 0.5f;
		Vector3 item3 = b + Vector3.forward * 0.5f;
		Vector3 item4 = b - Vector3.forward * 0.5f;
		int count;
		int num = (count = occVerts.Count);
		int item5 = num + 1;
		int item6 = num + 2;
		int item7 = num + 3;
		occVerts.Add(item);
		occVerts.Add(item2);
		occVerts.Add(item3);
		occVerts.Add(item4);
		occNorms.Add(val);
		occNorms.Add(val);
		occNorms.Add(val);
		occNorms.Add(val);
		occUVs.Add(uv);
		occUVs.Add(uv);
		occUVs.Add(uv);
		occUVs.Add(uv);
		occTris.Add(count);
		occTris.Add(item5);
		occTris.Add(item6);
		occTris.Add(item5);
		occTris.Add(item7);
		occTris.Add(item6);
	}

	private void AddEdgeDefault(Vector3 a, Vector3 b)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		a.z = 0f;
		b.z = 0f;
		Vector3 val = b - a;
		val = new Vector3(val.y, 0f - val.x, 0f);
		Vector3 item = a + Vector3.forward * 0.5f;
		Vector3 item2 = a - Vector3.forward * 0.5f;
		Vector3 item3 = b + Vector3.forward * 0.5f;
		Vector3 item4 = b - Vector3.forward * 0.5f;
		int count;
		int num = (count = occVerts.Count);
		int item5 = num + 1;
		int item6 = num + 2;
		int item7 = num + 3;
		occVerts.Add(item);
		occVerts.Add(item2);
		occVerts.Add(item3);
		occVerts.Add(item4);
		occNorms.Add(val);
		occNorms.Add(val);
		occNorms.Add(val);
		occNorms.Add(val);
		occTris.Add(count);
		occTris.Add(item5);
		occTris.Add(item6);
		occTris.Add(item5);
		occTris.Add(item7);
		occTris.Add(item6);
	}
}
