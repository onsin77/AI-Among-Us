using System.Collections;
using UnityEngine;

public class CuttableVegetable : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer[] uncutPieces;

	[SerializeField]
	private SpriteRenderer[] partiallyCutPieces;

	[SerializeField]
	private Sprite[] cutSprites;

	private int cutPieces;

	public int NextPieceIndex => uncutPieces.Length - cutPieces - 1;

	public SpriteRenderer NextPiece => uncutPieces[NextPieceIndex];

	public bool IsFullyCut => CheckComplete(cutPieces);

	private void Awake()
	{
		SpriteRenderer[] array = partiallyCutPieces;
		foreach (SpriteRenderer val in array)
		{
			if ((Object)(object)val != (Object)null)
			{
				((Component)val).gameObject.SetActive(false);
			}
		}
	}

	public IEnumerator CoCutNextPiece()
	{
		int nextPieceIndex = NextPieceIndex;
		int num = nextPieceIndex - 1;
		uncutPieces[nextPieceIndex].sprite = cutSprites[nextPieceIndex];
		if ((Object)(object)partiallyCutPieces[nextPieceIndex] != (Object)null)
		{
			((Component)uncutPieces[nextPieceIndex]).gameObject.SetActive(true);
			((Component)partiallyCutPieces[nextPieceIndex]).gameObject.SetActive(false);
		}
		if ((Object)(object)partiallyCutPieces[num] != (Object)null)
		{
			((Component)uncutPieces[num]).gameObject.SetActive(false);
			((Component)partiallyCutPieces[num]).gameObject.SetActive(true);
		}
		float num2 = 0.6f - 0.6f / (float)(uncutPieces.Length - 1) * (float)cutPieces;
		if (CheckComplete(cutPieces + 1))
		{
			uncutPieces[num].sprite = cutSprites[num];
		}
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		yield return Effects.Slide2D(((Component)NextPiece).transform, Vector2.op_Implicit(((Component)NextPiece).transform.localPosition), Vector2.op_Implicit(((Component)NextPiece).transform.localPosition) + new Vector2(num2, 0f), 0.25f);
		cutPieces++;
	}

	private bool CheckComplete(int cutPieces)
	{
		return cutPieces >= uncutPieces.Length - 1;
	}
}
