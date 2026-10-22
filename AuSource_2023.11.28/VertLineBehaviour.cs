using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class VertLineBehaviour : MonoBehaviour
{
	public int NumPoints = 128;

	public FloatRange Width;

	public FloatRange Height;

	private Mesh mesh;

	private MeshRenderer rend;

	private Vector3[] vecs;

	public float Duration = 4f;

	private float timer;

	public int Offset = 25;

	public int beats = 4;

	public int beatPadding = 5;

	public Color color
	{
		set
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			((Renderer)rend).material.SetColor("_Color", value);
		}
	}

	public void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected Obj, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		rend = ((Component)this).GetComponent<MeshRenderer>();
		mesh = new Mesh();
		((Component)this).GetComponent<MeshFilter>().mesh = mesh;
		mesh.MarkDynamic();
		vecs = new Vector3[NumPoints];
		Vector2[] array = new Vector2[NumPoints];
		int[] array2 = new int[NumPoints];
		float num = vecs.Length - 1;
		for (int i = 0; i < vecs.Length; i++)
		{
			vecs[i].y = Height.Lerp((float)i / num);
			array2[i] = i;
			array[i] = new Vector2(0.5f, (float)i / num);
		}
		mesh.vertices = vecs;
		mesh.uv = array;
		mesh.SetIndices(array2, (MeshTopology)4, 0);
	}

	public void Update()
	{
		timer += Time.deltaTime;
		if (timer >= Duration)
		{
			timer = 0f;
		}
		float num = timer / Duration;
		mesh.vertices = vecs;
		((Renderer)rend).material.SetFloat("_Fade", num);
	}

	public void SetAlive()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		color = Color.white;
		_ = vecs.Length;
		int num = vecs.Length / (beats + beatPadding);
		for (int i = 0; i < beats; i++)
		{
			int num2 = (int)((float)i * ((float)vecs.Length / (float)beats)) + Offset;
			for (int j = 0; j < num; j++)
			{
				int num3 = (j + num2) % vecs.Length;
				Vector3 val = vecs[num3];
				float num4 = (float)j / (float)num;
				if ((double)num4 < 0.3)
				{
					float num5 = num4 / 0.3f;
					val.x = Width.Lerp(Mathf.Lerp(0.5f, 0f, num5));
				}
				else if (num4 < 0.6f)
				{
					float num6 = (num4 - 0.3f) / 0.3f;
					val.x = Width.Lerp(Mathf.Lerp(0f, 1f, num6));
				}
				else
				{
					float num7 = (num4 - 0.6f) / 0.3f;
					val.x = Width.Lerp(Mathf.Lerp(1f, 0.5f, num7));
				}
				vecs[num3] = val;
			}
		}
		mesh.vertices = vecs;
	}

	public void SetDead()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		color = Color.red;
		float num = vecs.Length - 1;
		for (int i = 0; i < vecs.Length; i++)
		{
			Vector3 val = vecs[i];
			val.x = Width.Lerp(0.5f);
			val.y = Height.Lerp((float)i / num);
			vecs[i] = val;
		}
		mesh.vertices = vecs;
	}
}
