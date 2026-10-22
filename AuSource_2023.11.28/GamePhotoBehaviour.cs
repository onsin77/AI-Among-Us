using System.Collections;
using UnityEngine;

public class GamePhotoBehaviour : MonoBehaviour
{
	public static readonly Color InWaterPink = Color.Lerp(Color.white, Color32.op_Implicit(new Color32((byte)211, (byte)106, (byte)129, byte.MaxValue)), 0.6f);

	public float zOffset;

	public SpriteRenderer Frame;

	public SpriteRenderer Image;

	public Collider2D Hitbox;

	public Color TargetColor = Palette.ClearWhite;

	public void Start()
	{
		((Component)this).transform.SetLocalZ(1f + zOffset);
	}

	internal IEnumerator Pickup()
	{
		yield return Effects.All(Effects.Lerp(0.3f, (float t) =>
		{
			((Component)this).transform.SetLocalZ(1f - t + zOffset);
		}), Effects.ScaleIn(((Component)this).transform, 1f, 1.2f, 0.3f), Effects.Sequence(Effects.Wait(0.15f), Effects.Action(() =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			TargetColor = Color.white;
		})));
	}

	internal IEnumerator Drop(bool inWater)
	{
		yield return Effects.All(Effects.Lerp(0.3f, (float t) =>
		{
			((Component)this).transform.SetLocalZ(t + zOffset);
		}), Effects.ScaleIn(((Component)this).transform, 1.2f, 1f, 0.3f), Effects.Sequence(Effects.Wait(0.15f), Effects.Action(() =>
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (inWater)
			{
				TargetColor = InWaterPink;
			}
		})));
	}

	public GamePhotoBehaviour()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}

	static GamePhotoBehaviour()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
	}
}
