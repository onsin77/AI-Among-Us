using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeatherControl : MonoBehaviour
{
	public Sprite backgroundLight;

	public Sprite backgroundDark;

	public Sprite lightOff;

	public Sprite lightOn;

	public SpriteRenderer Background;

	public SpriteRenderer Switch;

	public SpriteRenderer Light;

	public TextMeshPro Label;

	internal void SetInactive()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Light.sprite = lightOff;
		((Graphic)Label).color = Color32.op_Implicit(new Color32((byte)146, (byte)135, (byte)163, byte.MaxValue));
		((MonoBehaviour)this).StartCoroutine(Run());
		Switch.flipX = true;
	}

	public void SetActive()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)this).StopAllCoroutines();
		((Graphic)Label).color = Color32.op_Implicit(new Color32((byte)81, (byte)53, (byte)115, byte.MaxValue));
		Background.sprite = backgroundLight;
		Light.sprite = lightOn;
		Switch.flipX = false;
	}

	private IEnumerator Run()
	{
		while (true)
		{
			Background.sprite = backgroundDark;
			yield return Effects.Wait(0.5f);
			Background.sprite = backgroundLight;
			yield return Effects.Wait(0.5f);
		}
	}
}
