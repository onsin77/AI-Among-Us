using System.Collections;
using Rewired;
using UnityEngine;

public class AcceptDivertPowerGame : Minigame
{
	private LineRenderer[] LeftWires;

	private LineRenderer[] RightWires;

	public GameObject RightWireParent;

	public GameObject LeftWireParent;

	public SpriteRenderer Switch;

	public AudioClip SwitchSound;

	private bool done;

	private bool prevHadInput;

	private float rotateAngle;

	private Vector2 prevStickDir;

	public void Start()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		LeftWires = LeftWireParent.GetComponentsInChildren<LineRenderer>();
		RightWires = RightWireParent.GetComponentsInChildren<LineRenderer>();
		for (int i = 0; i < LeftWires.Length; i++)
		{
			((Renderer)LeftWires[i]).material.SetColor("_Color", Color.yellow);
		}
		SetupInput(disableCursor: true);
	}

	public void DoSwitch()
	{
		if (!done)
		{
			done = true;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(SwitchSound, loop: false);
			}
			((MonoBehaviour)this).StartCoroutine(CoDoSwitch());
		}
	}

	private IEnumerator CoDoSwitch()
	{
		yield return new WaitForLerp(0.25f, (float t) =>
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			((Component)Switch).transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, 90f, t));
		});
		LeftWires[0].SetPosition(1, new Vector3(1.265f, 0f, 0f));
		for (int num = 0; num < RightWires.Length; num++)
		{
			((Renderer)RightWires[num]).enabled = true;
			((Renderer)RightWires[num]).material.SetColor("_Color", Color.yellow);
		}
		for (int num2 = 0; num2 < LeftWires.Length; num2++)
		{
			((Renderer)LeftWires[num2]).material.SetColor("_Color", Color.yellow);
		}
		if (Object.op_Implicit((Object)(object)MyNormTask))
		{
			MyNormTask.NextStep();
		}
		((MonoBehaviour)this).StartCoroutine(CoStartClose());
	}

	public void Update()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		if (!done)
		{
			Vector2 axis2DRaw = ReInput.players.GetPlayer(0).GetAxis2DRaw(13, 14);
			if (axis2DRaw.sqrMagnitude > 0.9f)
			{
				Vector2 normalized = axis2DRaw.normalized;
				if (prevHadInput)
				{
					float num = Vector2.SignedAngle(prevStickDir, normalized);
					rotateAngle += num;
					if (Mathf.Abs(rotateAngle) > 45f)
					{
						DoSwitch();
					}
				}
				prevStickDir = normalized;
				prevHadInput = true;
			}
			else
			{
				rotateAngle = 0f;
				prevHadInput = false;
			}
		}
		for (int i = 0; i < LeftWires.Length; i++)
		{
			Vector2 textureOffset = ((Renderer)LeftWires[i]).material.GetTextureOffset("_MainTex");
			textureOffset.x -= Time.deltaTime * 3f;
			((Renderer)LeftWires[i]).material.SetTextureOffset("_MainTex", textureOffset);
		}
		for (int j = 0; j < RightWires.Length; j++)
		{
			Vector2 textureOffset2 = ((Renderer)RightWires[j]).material.GetTextureOffset("_MainTex");
			textureOffset2.x += Time.deltaTime * 3f;
			((Renderer)RightWires[j]).material.SetTextureOffset("_MainTex", textureOffset2);
		}
	}
}
