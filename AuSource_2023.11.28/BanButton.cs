using System.Collections;
using TMPro;
using UnityEngine;

public class BanButton : MonoBehaviour
{
	public TextMeshPro NameText;

	public SpriteRenderer Background;

	public int TargetClientId;

	public int numVotes;

	public BanMenu Parent { get; set; }

	public void Start()
	{
		Background.SetCooldownNormalizedUvs();
	}

	public void Select()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Parent.CanSelect(TargetClientId))
		{
			Background.color = Palette.White;
			Parent.Select(TargetClientId);
		}
	}

	public void Unselect()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Background.color = Palette.DisabledGrey;
	}

	public void SetVotes(int newVotes)
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(CoSetVotes(numVotes, newVotes));
		numVotes = newVotes;
	}

	private IEnumerator CoSetVotes(int oldNum, int newNum)
	{
		_ = (float)oldNum / 3f;
		float end = (float)newNum / 3f;
		for (float timer = 0f; timer < 0.2f; timer += Time.deltaTime)
		{
			((Renderer)Background).material.SetFloat("_Percent", Mathf.SmoothStep(end, end, timer / 0.2f));
			yield return null;
		}
		((Renderer)Background).material.SetFloat("_Percent", end);
	}
}
