using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class CosmicubeBuildHelper : MonoBehaviour
{
	public enum CubeSide
	{
		East,
		West,
		North,
		South
	}

	public CosmicubeCell cell;

	public CubeSide cubeSide;

	public GameObject connector;

	private bool childOwned;

	private bool animating;

	public void AnimateUnlock(float duration)
	{
		((MonoBehaviour)this).StartCoroutine(CoAnimateUnlock(duration));
	}

	private IEnumerator CoAnimateUnlock(float duration)
	{
		animating = true;
		bool flag = cubeSide == CubeSide.East || cubeSide == CubeSide.South;
		Vector3 startPosition = new Vector3(0f, flag ? 0.25f : (-0.25f), -0.01f);
		Vector3 startScale = new Vector3(1f, 0f, 1f);
		Vector3 endPosition = new Vector3(0f, 0f, -0.01f);
		Vector3 endScale = Vector3.one;
		Transform green = connector.transform.Find("Green");
		((Component)green).gameObject.SetActive(true);
		green.localScale = startScale;
		green.localPosition = startPosition;
		yield return Effects.Lerp(duration, (float t) =>
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			green.localScale = Vector3.Lerp(startScale, endScale, t);
			green.localPosition = Vector3.Lerp(startPosition, endPosition, t);
		});
		animating = false;
	}

	private void Update()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		if (animating)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)cell.CosmeticData) && !Application.isPlaying)
		{
			cell.qualitySprite.color = CosmicubeManager.qualityColorMap[cell.CosmeticData.quality];
		}
		switch (cubeSide)
		{
		case CubeSide.East:
			if ((Object)(object)cell.eastChild != (Object)null)
			{
				connector.SetActive(true);
				if (cell.eastChild.unlocked)
				{
					childOwned = true;
				}
			}
			else
			{
				connector.SetActive(false);
			}
			break;
		case CubeSide.West:
			if ((Object)(object)cell.westChild != (Object)null)
			{
				connector.SetActive(true);
				if (cell.westChild.unlocked)
				{
					childOwned = true;
				}
			}
			else
			{
				connector.SetActive(false);
			}
			break;
		case CubeSide.North:
			if ((Object)(object)cell.northChild != (Object)null)
			{
				connector.SetActive(true);
				if (cell.northChild.unlocked)
				{
					childOwned = true;
				}
			}
			else
			{
				connector.SetActive(false);
			}
			break;
		case CubeSide.South:
			if ((Object)(object)cell.southChild != (Object)null)
			{
				connector.SetActive(true);
				if (cell.southChild.unlocked)
				{
					childOwned = true;
				}
			}
			else
			{
				connector.SetActive(false);
			}
			break;
		}
		if (cell.unlocked)
		{
			if (!childOwned)
			{
				connector.GetComponent<SpriteRenderer>().color = Palette.CosmicubeCellUnlockedColor;
			}
			else
			{
				connector.GetComponent<SpriteRenderer>().color = Palette.White;
			}
		}
	}

	public void Show()
	{
		connector.SetActive(true);
	}
}
