using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class SurveillanceMinigame : Minigame
{
	public Camera CameraPrefab;

	public GameObject Viewables;

	public MeshRenderer[] ViewPorts;

	public TextMeshPro[] SabText;

	private PlainShipRoom[] FilteredRooms;

	private RenderTexture[] textures;

	public MeshRenderer FillQuad;

	public Material DefaultMaterial;

	public Material StaticMaterial;

	private bool isStatic;

	public override void Begin(PlayerTask task)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		DestroyableSingleton<HudManager>.Instance.PlayerCam.Locked = true;
		FilteredRooms = ShipStatus.Instance.AllRooms.Where((PlainShipRoom i) => Object.op_Implicit((Object)(object)i.survCamera)).ToArray();
		textures = new RenderTexture[FilteredRooms.Length];
		for (int num = 0; num < FilteredRooms.Length; num++)
		{
			PlainShipRoom plainShipRoom = FilteredRooms[num];
			Camera val = Object.Instantiate<Camera>(CameraPrefab);
			((Component)val).transform.SetParent(((Component)this).transform);
			((Component)val).transform.position = ((Component)plainShipRoom).transform.position + plainShipRoom.survCamera.Offset - new Vector3(0f, 0f, 0f - ((Component)plainShipRoom.survCamera).transform.localPosition.z);
			val.orthographicSize = plainShipRoom.survCamera.CamSize;
			RenderTexture temporary = RenderTexture.GetTemporary((int)(256f * plainShipRoom.survCamera.CamAspect), 256, 16, (RenderTextureFormat)0);
			textures[num] = temporary;
			val.targetTexture = temporary;
			((Renderer)ViewPorts[num]).material.SetTexture("_MainTex", (Texture)(object)temporary);
		}
		if (!PlayerControl.LocalPlayer.Data.IsDead)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, (byte)1);
		}
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		if (isStatic && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			isStatic = false;
			for (int i = 0; i < ViewPorts.Length; i++)
			{
				((Renderer)ViewPorts[i]).sharedMaterial = DefaultMaterial;
				((Renderer)ViewPorts[i]).material.SetTexture("_MainTex", (Texture)(object)textures[i]);
				((Component)SabText[i]).gameObject.SetActive(false);
			}
		}
		else if (!isStatic && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			isStatic = true;
			for (int j = 0; j < ViewPorts.Length; j++)
			{
				((Renderer)ViewPorts[j]).sharedMaterial = StaticMaterial;
				((Component)SabText[j]).gameObject.SetActive(true);
			}
		}
	}

	protected override IEnumerator CoAnimateOpen()
	{
		Viewables.SetActive(false);
		((Renderer)FillQuad).material.SetFloat("_Center", -5f);
		((Renderer)FillQuad).material.SetColor("_Color2", Color.clear);
		for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
		{
			((Renderer)FillQuad).material.SetColor("_Color2", Color.Lerp(Color.clear, Color.black, timer / 0.25f));
			yield return null;
		}
		((Renderer)FillQuad).material.SetColor("_Color2", Color.black);
		Viewables.SetActive(true);
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			((Renderer)FillQuad).material.SetFloat("_Center", Mathf.Lerp(-5f, 0f, timer / 0.1f));
			yield return null;
		}
		for (float timer = 0f; timer < 0.15f; timer += Time.deltaTime)
		{
			((Renderer)FillQuad).material.SetFloat("_Center", Mathf.Lerp(-3f, 0.4f, timer / 0.15f));
			yield return null;
		}
		((Renderer)FillQuad).material.SetFloat("_Center", 0.4f);
	}

	private IEnumerator CoAnimateClose()
	{
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			((Renderer)FillQuad).material.SetFloat("_Center", Mathf.Lerp(0.4f, -5f, timer / 0.1f));
			yield return null;
		}
		Viewables.SetActive(false);
		for (float timer = 0f; timer < 0.3f; timer += Time.deltaTime)
		{
			((Renderer)FillQuad).material.SetColor("_Color2", Color.Lerp(Color.black, Color.clear, timer / 0.3f));
			yield return null;
		}
		((Renderer)FillQuad).material.SetColor("_Color2", Color.clear);
	}

	protected override IEnumerator CoDestroySelf()
	{
		DestroyableSingleton<HudManager>.Instance.PlayerCam.Locked = false;
		yield return CoAnimateClose();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public override void Close()
	{
		ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, (byte)2);
		base.Close();
	}

	public void OnDestroy()
	{
		for (int i = 0; i < textures.Length; i++)
		{
			textures[i].Release();
		}
	}
}
