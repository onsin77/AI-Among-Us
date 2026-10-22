using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AspectScaledAsset : MonoBehaviour
{
	private class ScaledAsset
	{
		public float OriginalWidth;

		public ScaledAsset(float originalWidth)
		{
			OriginalWidth = originalWidth;
		}
	}

	private class ScaledSprite : ScaledAsset
	{
		public SpriteRenderer Sprite;

		public ScaledSprite(float originalWidth, SpriteRenderer sprite)
			: base(originalWidth)
		{
			Sprite = sprite;
		}
	}

	private class ScaledCollider : ScaledAsset
	{
		public BoxCollider2D Collider;

		public ScaledCollider(float originalWidth, BoxCollider2D collider)
			: base(originalWidth)
		{
			Collider = collider;
		}
	}

	private class ScaledText : ScaledAsset
	{
		public TextMeshPro Text;

		public ScaledText(float originalWidth, TextMeshPro text)
			: base(originalWidth)
		{
			Text = text;
		}
	}

	private class ScaledMask : ScaledAsset
	{
		public SpriteMask Mask;

		public ScaledMask(float originalWidth, SpriteMask mask)
			: base(originalWidth)
		{
			Mask = mask;
		}
	}

	[SerializeField]
	[Header("Required")]
	private List<SpriteRenderer> spritesToScale;

	[SerializeField]
	[Header("Optional")]
	private AspectPosition aspectPosition;

	[SerializeField]
	private int positionDirection = 1;

	[SerializeField]
	private List<BoxCollider2D> collidersToScale;

	[SerializeField]
	private List<TextMeshPro> textToScale;

	[SerializeField]
	private List<SpriteMask> masksToScale;

	private float originalDistanceFromEdge;

	private List<ScaledSprite> allSprites;

	private List<ScaledCollider> allColliders;

	private List<ScaledText> allText;

	private List<ScaledMask> allMasks;

	private bool initialized;

	private void Awake()
	{
		allSprites = new List<ScaledSprite>();
		allColliders = new List<ScaledCollider>();
		allText = new List<ScaledText>();
		allMasks = new List<ScaledMask>();
		spritesToScale.ForEach((SpriteRenderer s) =>
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			allSprites.Add(new ScaledSprite(s.size.x, s));
		});
		collidersToScale.ForEach((BoxCollider2D c) =>
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			allColliders.Add(new ScaledCollider(c.size.x, c));
		});
		textToScale.ForEach((TextMeshPro t) =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			allText.Add(new ScaledText(((TMP_Text)t).rectTransform.sizeDelta.x, t));
		});
		masksToScale.ForEach((SpriteMask m) =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			allMasks.Add(new ScaledMask(((Component)m).transform.localScale.x, m));
		});
		if (Object.op_Implicit((Object)(object)aspectPosition))
		{
			originalDistanceFromEdge = aspectPosition.DistanceFromEdge.x;
		}
		initialized = true;
	}

	public void ScaleObject(float aspectDiff)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (initialized)
		{
			allSprites.ForEach((ScaledSprite s) =>
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				s.Sprite.size = new Vector2(s.OriginalWidth * aspectDiff, s.Sprite.size.y);
			});
			allColliders.ForEach((ScaledCollider s) =>
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				s.Collider.size = new Vector2(s.OriginalWidth * aspectDiff, s.Collider.size.y);
			});
			allText.ForEach((ScaledText s) =>
			{
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				((TMP_Text)s.Text).rectTransform.sizeDelta = new Vector2(s.OriginalWidth * aspectDiff, ((TMP_Text)s.Text).rectTransform.sizeDelta.y);
			});
			allMasks.ForEach((ScaledMask s) =>
			{
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				((Component)s.Mask).transform.localScale = Vector2.op_Implicit(new Vector2(s.OriginalWidth * aspectDiff, ((Component)s.Mask).transform.localScale.y));
			});
			if (Object.op_Implicit((Object)(object)aspectPosition))
			{
				aspectPosition.DistanceFromEdge.x = originalDistanceFromEdge + (float)positionDirection * ((allSprites[0].OriginalWidth - allSprites[0].Sprite.size.x) / 2f);
			}
		}
	}
}
