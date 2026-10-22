using UnityEngine;

public class DangerMeter : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer[] barSprites;

	[SerializeField]
	private SpriteRenderer crewmateSeekerSprite;

	[SerializeField]
	private Color emptyColor;

	[SerializeField]
	private Color safeColor;

	[SerializeField]
	private Color cautionColor;

	[SerializeField]
	private Color dangerColor;

	private Vector3 originalLocalPosition;

	private Coroutine shakeCoroutine;

	public void SetDangerValue(float dangerLevel1, float dangerLevel2)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (dangerLevel1 <= 0f)
		{
			SetFirstNBarColors(5, emptyColor);
		}
		else if (dangerLevel1 <= 0.25f)
		{
			SetFirstNBarColors(1, safeColor);
		}
		else if (dangerLevel1 > 0.25f && dangerLevel1 < 0.5f)
		{
			SetFirstNBarColors(2, safeColor);
		}
		else if (dangerLevel1 > 0.5f && dangerLevel2 <= 0f)
		{
			SetFirstNBarColors(3, cautionColor);
		}
		else if (dangerLevel2 > 0f && dangerLevel2 < 0.5f)
		{
			SetFirstNBarColors(4, cautionColor);
		}
		else
		{
			SetFirstNBarColors(5, dangerColor);
		}
		if (shakeCoroutine == null && dangerLevel2 > 0.5f)
		{
			originalLocalPosition = ((Component)this).transform.localPosition;
			if (((Behaviour)this).isActiveAndEnabled)
			{
				shakeCoroutine = ((MonoBehaviour)this).StartCoroutine(Effects.ShakeForever(((Component)this).transform, 0.02f));
			}
		}
		else if (dangerLevel2 < 0.5f && shakeCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(shakeCoroutine);
			((Component)this).transform.localPosition = originalLocalPosition;
			shakeCoroutine = null;
		}
	}

	private void SetFirstNBarColors(int numBars, Color color)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < barSprites.Length; i++)
		{
			if (i < numBars)
			{
				barSprites[i].color = color;
			}
			else
			{
				barSprites[i].color = emptyColor;
			}
		}
		if (color == Color.clear)
		{
			crewmateSeekerSprite.color = emptyColor;
		}
		else
		{
			crewmateSeekerSprite.color = color;
		}
	}
}
