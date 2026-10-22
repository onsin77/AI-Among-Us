using System.Collections;
using System.Linq;
using PowerTools;
using UnityEngine;

public class DripBehaviour : MonoBehaviour
{
	public Vector2Range SpawnArea;

	public FloatRange FirstWait = new FloatRange(0f, 3f);

	public FloatRange Frequency = new FloatRange(0.75f, 3f);

	public SpriteAnim myAnim;

	public Collider2D[] IgnoreAreas;

	public bool FixDepth = true;

	public void Start()
	{
		((MonoBehaviour)this).StartCoroutine(Run());
	}

	private IEnumerator Run()
	{
		yield return Effects.Wait(FirstWait.Next());
		while (true)
		{
			Vector3 pos = Vector2.op_Implicit(SpawnArea.Next());
			((Component)this).transform.localPosition = pos;
			if (FixDepth)
			{
				pos = ((Component)this).transform.position;
				pos.z = pos.y / 1000f;
				((Component)this).transform.position = pos;
			}
			if (!IgnoreAreas.Any((Collider2D i) =>
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return i.OverlapPoint(Vector2.op_Implicit(pos));
			}))
			{
				myAnim.Play();
				while (myAnim.IsPlaying())
				{
					yield return null;
				}
				yield return Effects.Wait(Frequency.Next());
			}
		}
	}
}
