using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EggCoveringLeaf : MonoBehaviour
{
	[SerializeField]
	private PassiveButton leafButton;

	[SerializeField]
	private SpriteRenderer leafSprite;

	public event Action OnCollectedEvent;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		((UnityEvent)leafButton.OnClick).AddListener((UnityAction)(() =>
		{
			((MonoBehaviour)this).StartCoroutine(CoCollectLeaf());
		}));
	}

	private IEnumerator CoCollectLeaf()
	{
		leafButton.SetButtonEnableState(enabled: false);
		OnCollectedEvent?.Invoke();
		float num = ((!BoolRange.Next()) ? 1 : (-1)) * Random.Range(30, 50);
		float z = ((Component)this).transform.localEulerAngles.z;
		yield return Effects.All(Effects.ColorFade(leafSprite, Color.white, Palette.ClearWhite, 0.5f), Effects.Rotate2D(((Component)this).transform, z, z + num, 0.5f));
	}
}
