using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
	private struct Cloud
	{
		public int CloudIdx;

		public float Rate;

		public float Size;

		public float FlipX;

		public float PositionX;

		public float PositionY;

		public float PositionZ;
	}

	public Sprite[] CloudImages;

	public int NumClouds = 500;

	public float Length = 25f;

	public float Width = 25f;

	public Vector2 Direction = new Vector2(1f, 0f);

	public FloatRange Rates = new FloatRange(0.25f, 1f);

	public FloatRange Sizes = new FloatRange(0.75f, 1.25f);

	public bool Depth;

	public float MaxDepth = 4f;

	public float ParallaxOffset;

	public float ParallaxStrength = 1f;

	[HideInInspector]
	private Cloud[] clouds;

	private Vector2[] ExtentCache;

	[HideInInspector]
	private Mesh mesh;

	private Vector2 NormDir = new Vector2(1f, 0f);

	private Vector2 Tangent = new Vector2(0f, 1f);

	private float tanLen;

	[HideInInspector]
	private Vector3[] verts;

	public void Start()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected Obj, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		Vector2[][] array = new Vector2[CloudImages.Length][];
		ExtentCache = new Vector2[CloudImages.Length];
		for (int i = 0; i < CloudImages.Length; i++)
		{
			Sprite val = CloudImages[i];
			array[i] = val.uv;
			Vector2[] extentCache = ExtentCache;
			int num = i;
			Bounds bounds = val.bounds;
			extentCache[num] = Vector2.op_Implicit(bounds.extents);
		}
		clouds = new Cloud[NumClouds];
		verts = new Vector3[NumClouds * 4];
		Vector2[] array2 = new Vector2[NumClouds * 4];
		int[] array3 = new int[NumClouds * 6];
		SetDirection(Direction);
		MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
		mesh = new Mesh();
		mesh.MarkDynamic();
		component.mesh = mesh;
		Vector3 val2 = default;
		for (int j = 0; j < clouds.Length; j++)
		{
			Cloud cloud = clouds[j];
			int num2 = (cloud.CloudIdx = CloudImages.RandomIdx());
			Vector2 val3 = ExtentCache[num2];
			Vector2[] array4 = array[num2];
			float num3 = FloatRange.Next(-1f, 1f) * Length;
			float num4 = FloatRange.Next(-1f, 1f) * Width;
			float num5 = (cloud.PositionX = num3 * NormDir.x + num4 * Tangent.x);
			float num6 = (cloud.PositionY = num3 * NormDir.y + num4 * Tangent.y);
			cloud.Rate = Rates.Next();
			cloud.Size = Sizes.Next();
			cloud.FlipX = ((!BoolRange.Next()) ? 1 : (-1));
			if (Depth)
			{
				cloud.PositionZ = FloatRange.Next(0f, MaxDepth);
			}
			val3 *= cloud.Size;
			clouds[j] = cloud;
			int num7 = j * 4;
			val2.x = num5 - val3.x * cloud.FlipX;
			val2.y = num6 + val3.y;
			val2.z = cloud.PositionZ;
			verts[num7] = val2;
			val2.x = num5 + val3.x * cloud.FlipX;
			verts[num7 + 1] = val2;
			val2.x = num5 - val3.x * cloud.FlipX;
			val2.y = num6 - val3.y;
			verts[num7 + 2] = val2;
			val2.x = num5 + val3.x * cloud.FlipX;
			verts[num7 + 3] = val2;
			array2[num7] = array4[0];
			array2[num7 + 1] = array4[1];
			array2[num7 + 2] = array4[2];
			array2[num7 + 3] = array4[3];
			int num8 = j * 6;
			array3[num8] = num7;
			array3[num8 + 1] = num7 + 1;
			array3[num8 + 2] = num7 + 2;
			array3[num8 + 3] = num7 + 2;
			array3[num8 + 4] = num7 + 1;
			array3[num8 + 5] = num7 + 3;
		}
		mesh.vertices = verts;
		mesh.uv = array2;
		mesh.SetIndices(array3, (MeshTopology)0, 0);
	}

	public void SetDirection(Vector2 dir)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Direction = dir;
		NormDir = Direction.normalized;
		Tangent = new Vector2(0f - NormDir.y, NormDir.x);
		tanLen = Mathf.Sqrt(Tangent.y * Tangent.y + Tangent.x * Tangent.x);
	}

	private void Update()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		float num = -0.99f * Length;
		Vector2 val = Direction * Time.deltaTime;
		Vector3 val2 = default;
		for (int i = 0; i < clouds.Length; i++)
		{
			int num2 = i * 4;
			Cloud cloud = clouds[i];
			float positionX = cloud.PositionX;
			float positionY = cloud.PositionY;
			Vector2 val3 = ExtentCache[cloud.CloudIdx];
			val3 *= cloud.Size;
			float rate = cloud.Rate;
			positionX += rate * val.x;
			positionY += rate * val.y;
			if (OrthoDistance(positionX, positionY) > Length)
			{
				float num3 = FloatRange.Next(-1f, 1f) * Width;
				positionX = num * NormDir.x + num3 * Tangent.x;
				positionY = num * NormDir.y + num3 * Tangent.y;
				cloud.Rate = Rates.Next();
				cloud.Size = Sizes.Next();
				cloud.FlipX = ((!BoolRange.Next()) ? 1 : (-1));
				if (Depth)
				{
					cloud.PositionZ = FloatRange.Next(0f, MaxDepth);
				}
			}
			cloud.PositionX = positionX;
			cloud.PositionY = positionY;
			if (Depth)
			{
				positionY += (((Component)this).transform.position.y + ParallaxOffset) / (cloud.PositionZ * ParallaxStrength + 0.0001f);
				val2.z = cloud.PositionZ;
			}
			clouds[i] = cloud;
			val2.x = positionX - val3.x * cloud.FlipX;
			val2.y = positionY + val3.y;
			verts[num2] = val2;
			val2.x = positionX + val3.x * cloud.FlipX;
			verts[num2 + 1] = val2;
			val2.x = positionX - val3.x * cloud.FlipX;
			val2.y = positionY - val3.y;
			verts[num2 + 2] = val2;
			val2.x = positionX + val3.x * cloud.FlipX;
			verts[num2 + 3] = val2;
		}
		mesh.vertices = verts;
	}

	private float OrthoDistance(float pointx, float pointy)
	{
		return (Tangent.y * pointx - Tangent.x * pointy) / tanLen;
	}

	public CloudGenerator()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
	}
}
