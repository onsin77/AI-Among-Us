using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class StarGen : MonoBehaviour
{
	[Serializable]
	private struct Stars
	{
		public float Size;

		public float Rate;

		public float PositionX;

		public float PositionY;
	}

	public int NumStars = 500;

	public float Length = 25f;

	public float Width = 25f;

	public float MaxStarRadius = 0.05f;

	public Vector2 Direction = new Vector2(1f, 0f);

	private Vector2 NormDir = new Vector2(1f, 0f);

	private Vector2 Tangent = new Vector2(0f, 1f);

	private float tanLen;

	public FloatRange Rates = new FloatRange(0.25f, 1f);

	[HideInInspector]
	private Stars[] stars;

	[HideInInspector]
	private Vector3[] verts;

	[HideInInspector]
	private Mesh mesh;

	public void Start()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected Obj, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		this.stars = new Stars[NumStars];
		verts = new Vector3[NumStars * 4];
		Vector2[] array = new Vector2[NumStars * 4];
		int[] array2 = new int[NumStars * 6];
		SetDirection(Direction);
		MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
		mesh = new Mesh();
		mesh.MarkDynamic();
		component.mesh = mesh;
		Vector3 val = default;
		Vector2 val2 = default;
		for (int i = 0; i < this.stars.Length; i++)
		{
			Stars stars = this.stars[i];
			float num = FloatRange.Next(-1f, 1f) * Length;
			float num2 = FloatRange.Next(-1f, 1f) * Width;
			float num3 = (stars.PositionX = num * NormDir.x + num2 * Tangent.x);
			float num4 = (stars.PositionY = num * NormDir.y + num2 * Tangent.y);
			float num5 = (stars.Size = FloatRange.Next(0.01f, MaxStarRadius));
			stars.Rate = Rates.Next();
			this.stars[i] = stars;
			int num6 = i * 4;
			val.x = num3 - num5;
			val.y = num4 + num5;
			verts[num6] = val;
			val.y = num4 - num5;
			verts[num6 + 1] = val;
			val.x = num3 + num5;
			verts[num6 + 2] = val;
			val.y = num4 + num5;
			verts[num6 + 3] = val;
			val2.x = -1f;
			val2.y = 1f;
			array[num6] = val2;
			val2.y = -1f;
			array[num6 + 1] = val2;
			val2.x = 1f;
			array[num6 + 2] = val2;
			val2.y = 1f;
			array[num6 + 3] = val2;
			int num7 = i * 6;
			array2[num7] = num6;
			array2[num7 + 1] = num6 + 1;
			array2[num7 + 2] = num6 + 2;
			array2[num7 + 3] = num6 + 2;
			array2[num7 + 4] = num6;
			array2[num7 + 5] = num6 + 3;
		}
		mesh.vertices = verts;
		mesh.uv = array;
		mesh.SetIndices(array2, (MeshTopology)0, 0);
	}

	private void Update()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		float num = -0.99f * Length;
		Vector2 val = Direction * Time.deltaTime;
		for (int i = 0; i < this.stars.Length; i++)
		{
			Stars stars = this.stars[i];
			float size = stars.Size;
			float positionX = stars.PositionX;
			float positionY = stars.PositionY;
			float num2 = stars.Rate * (size / MaxStarRadius);
			positionX += num2 * val.x;
			positionY += num2 * val.y;
			if (OrthoDistance(positionX, positionY) > Length)
			{
				float num3 = FloatRange.Next(-1f, 1f) * Width;
				positionX = num * NormDir.x + num3 * Tangent.x;
				positionY = num * NormDir.y + num3 * Tangent.y;
				this.stars[i].Rate = Rates.Next();
			}
			stars.PositionX = positionX;
			stars.PositionY = positionY;
			this.stars[i] = stars;
			int num4 = i * 4;
			float x = positionX - size;
			float x2 = positionX + size;
			float y = positionY + size;
			float y2 = positionY - size;
			verts[num4].x = x;
			verts[num4].y = y;
			verts[num4 + 1].x = x;
			verts[num4 + 1].y = y2;
			verts[num4 + 2].x = x2;
			verts[num4 + 2].y = y2;
			verts[num4 + 3].x = x2;
			verts[num4 + 3].y = y;
		}
		mesh.vertices = verts;
	}

	public void SetDirection(Vector2 dir)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Direction = dir;
		if (Direction.sqrMagnitude < 0.0001f)
		{
			NormDir = Vector2.left;
			Tangent = Vector2.up;
			tanLen = 1f;
		}
		else
		{
			NormDir = Direction.normalized;
			Tangent = new Vector2(0f - NormDir.y, NormDir.x);
			tanLen = Mathf.Sqrt(Tangent.y * Tangent.y + Tangent.x * Tangent.x);
		}
	}

	public void RegenPositions()
	{
		if (stars != null)
		{
			for (int i = 0; i < stars.Length; i++)
			{
				float num = FloatRange.Next(-1f, 1f) * Length;
				float num2 = FloatRange.Next(-1f, 1f) * Width;
				stars[i].PositionX = num * NormDir.x + num2 * Tangent.x;
				stars[i].PositionY = num * NormDir.y + num2 * Tangent.y;
			}
		}
	}

	private float OrthoDistance(float pointx, float pointy)
	{
		return (Tangent.y * pointx - Tangent.x * pointy) / tanLen;
	}

	public StarGen()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
	}
}
