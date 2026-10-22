using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonRolloverHandler : MonoBehaviour
{
	public SpriteRenderer Target;

	public SpriteRenderer Target2;

	public TextMeshPro TargetText;

	public MeshRenderer TargetMesh;

	public Color OverColor = Color.green;

	public Color OutColor = Color.white;

	public Color UnselectedColor = Color.white;

	public bool UseObjectsOutColor;

	public bool UseObjectSelected;

	public AudioClip HoverSound;

	public void Awake()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected Obj, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected Obj, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected Obj, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected Obj, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected Obj, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected Obj, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected Obj, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		PassiveButton component = ((Component)this).GetComponent<PassiveButton>();
		if ((Object)(object)component != (Object)null)
		{
			component.OnMouseOver.AddListener((UnityAction)DoMouseOver);
			component.OnMouseOut.AddListener((UnityAction)DoMouseOut);
			if (UseObjectSelected)
			{
				((UnityEvent)component.OnClick).AddListener((UnityAction)DoMouseClick);
			}
			return;
		}
		ButtonBehavior component2 = ((Component)this).GetComponent<ButtonBehavior>();
		if ((Object)(object)component2 != (Object)null)
		{
			component2.OnMouseOver.AddListener((UnityAction)DoMouseOver);
			component2.OnMouseOut.AddListener((UnityAction)DoMouseOut);
			if (UseObjectSelected)
			{
				((UnityEvent)component2.OnClick).AddListener((UnityAction)DoMouseClick);
			}
			return;
		}
		SlideBar component3 = ((Component)this).GetComponent<SlideBar>();
		if ((Object)(object)component3 != (Object)null)
		{
			component3.OnMouseOver.AddListener((UnityAction)DoMouseOver);
			component3.OnMouseOut.AddListener((UnityAction)DoMouseOut);
		}
		else if (UseObjectsOutColor)
		{
			if ((Object)(object)Target != (Object)null)
			{
				OutColor = Target.color;
			}
			if ((Object)(object)TargetText != (Object)null)
			{
				OutColor = ((Graphic)TargetText).color;
			}
			if ((Object)(object)TargetMesh != (Object)null)
			{
				OutColor = ((Renderer)TargetMesh).material.color;
			}
		}
	}

	public void DoMouseClick()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Target != (Object)null)
		{
			Target.color = OverColor;
		}
		if ((Object)(object)TargetText != (Object)null)
		{
			((Graphic)TargetText).color = OverColor;
		}
		if ((Object)(object)TargetMesh != (Object)null)
		{
			((Renderer)TargetMesh).material.SetColor("_Color", OverColor);
		}
	}

	public void DoMouseOver()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Target2 != (Object)null)
		{
			Target2.color = OverColor;
		}
		else if ((Object)(object)Target != (Object)null)
		{
			Target.color = OverColor;
		}
		if ((Object)(object)TargetText != (Object)null)
		{
			((Graphic)TargetText).color = OverColor;
		}
		if ((Object)(object)TargetMesh != (Object)null)
		{
			((Renderer)TargetMesh).material.SetColor("_Color", OverColor);
		}
		if (Object.op_Implicit((Object)(object)HoverSound))
		{
			SoundManager.Instance.PlaySound(HoverSound, loop: false);
		}
	}

	public void DoMouseOut()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Target2 != (Object)null)
		{
			Target2.color = UnselectedColor;
		}
		if ((Object)(object)Target != (Object)null)
		{
			Target.color = OutColor;
		}
		if ((Object)(object)TargetText != (Object)null)
		{
			((Graphic)TargetText).color = OutColor;
		}
		if ((Object)(object)TargetMesh != (Object)null)
		{
			((Renderer)TargetMesh).material.SetColor("_Color", OutColor);
		}
	}

	public void SetDisabledColors()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ChangeOutColor(Color.gray);
		OverColor = Color.gray;
		Target.color = Color.grey;
	}

	public void SetEnabledColors()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ChangeOutColor(Color.white);
		OverColor = Color.green;
		Target.color = Color.white;
	}

	public void ChangeOutColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		OutColor = color;
		DoMouseOut();
	}

	public void WaitClickUnselect()
	{
		((MonoBehaviour)this).StartCoroutine(WaitChangeUnselect());
	}

	private IEnumerator WaitChangeUnselect()
	{
		yield return (object)new WaitForEndOfFrame();
		DoMouseOut();
		DoMouseOver();
	}

	public ButtonRolloverHandler()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}
