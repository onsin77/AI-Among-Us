using System;
using System.Collections;
using UnityEngine;

public class CollectableOre : MonoBehaviour
{
	private const float WAIT_TIME_COLLECT = 0.3f;

	private const float WAIT_TIME_ROCK = 0.7f;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private BreakableRock[] rocks;

	[SerializeField]
	private Sprite[] oreSprites;

	[SerializeField]
	private Sprite[] rockSprites;

	[SerializeField]
	private AudioClip gemCollect;

	private int rockCount;

	private Action onCollect;

	private bool isRock;

	private Action onRockRelease;

	public void Initialize(Action collectOre, Action<CollectableOre, Vector3> powerAction, Controller control, Action rockReleased)
	{
		onCollect = (Action)Delegate.Combine(onCollect, collectOre);
		onRockRelease = (Action)Delegate.Combine(onRockRelease, rockReleased);
		BreakableRock[] array = rocks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initialize(powerAction, control);
		}
	}

	public BreakableRock[] GetRocks()
	{
		return rocks;
	}

	public bool GetIsRock()
	{
		return isRock;
	}

	public void RotateObjects()
	{
		BreakableRock[] array = rocks;
		foreach (BreakableRock breakableRock in array)
		{
			RotateObject(((Component)breakableRock).gameObject);
		}
		RotateObject(((Component)spriteRenderer).gameObject);
	}

	public void SetNumRocks(int numRocks)
	{
		rockCount = 0;
		for (int i = 0; i < numRocks; i++)
		{
			rockCount++;
			((Component)rocks[i]).gameObject.SetActive(false);
		}
	}

	public void SetAsRock()
	{
		isRock = true;
		spriteRenderer.sprite = rockSprites.Random();
	}

	public void SetOreSprite(int index)
	{
		spriteRenderer.sprite = oreSprites[index];
	}

	public void BrokenRock()
	{
		rockCount++;
		if (rockCount >= rocks.Length)
		{
			onRockRelease?.Invoke();
			((MonoBehaviour)this).StartCoroutine(ShineCollect());
		}
	}

	public int GetNumberRocksBroken()
	{
		return rockCount;
	}

	private IEnumerator ShineCollect()
	{
		if (!isRock)
		{
			onCollect?.Invoke();
			yield return Effects.Wait(0.3f);
			yield return Effects.ScaleIn(((Component)spriteRenderer).transform, 1f, 1.2f, 0.3f);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(gemCollect, loop: false);
			}
			yield return Effects.ColorFade(spriteRenderer, Color.white, Palette.ClearWhite, 0.3f);
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			yield return Effects.Wait(0.7f);
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void RotateObject(GameObject obj)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		obj.transform.eulerAngles = new Vector3(obj.transform.eulerAngles.x, obj.transform.eulerAngles.y, (float)Random.Range(0, 360));
	}
}
