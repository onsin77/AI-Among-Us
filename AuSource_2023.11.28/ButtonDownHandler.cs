using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ButtonDownHandler : MonoBehaviour
{
	private Coroutine downState;

	public SpriteRenderer Target;

	public Sprite UpSprite;

	public Sprite DownSprite;

	public void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		((UnityEvent)((Component)this).GetComponent<PassiveButton>().OnClick).AddListener((UnityAction)StartDown);
	}

	public void OnDisable()
	{
		if (downState != null)
		{
			((MonoBehaviour)this).StopCoroutine(downState);
			downState = null;
			Target.sprite = UpSprite;
		}
	}

	private void StartDown()
	{
		if (downState == null)
		{
			downState = ((MonoBehaviour)this).StartCoroutine(CoRunDown());
		}
	}

	private IEnumerator CoRunDown()
	{
		Target.sprite = DownSprite;
		while (DestroyableSingleton<PassiveButtonManager>.Instance.controller.AnyTouch)
		{
			yield return null;
		}
		Target.sprite = UpSprite;
		downState = null;
	}
}
