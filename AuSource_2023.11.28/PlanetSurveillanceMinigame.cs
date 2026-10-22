using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlanetSurveillanceMinigame : Minigame
{
	public Camera Camera;

	public GameObject Viewables;

	public MeshRenderer ViewPort;

	public TextMeshPro LocationName;

	public TextMeshPro SabText;

	private RenderTexture texture;

	public MeshRenderer FillQuad;

	public Material DefaultMaterial;

	public Material StaticMaterial;

	private bool isStatic;

	private SurvCamera[] survCameras;

	public Transform DotParent;

	private SpriteRenderer[] Dots;

	public Sprite DotEnabled;

	public Sprite DotDisabled;

	private int currentCamera;

	public AudioClip ChangeSound;

	public override void Begin(PlayerTask task)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		DestroyableSingleton<HudManager>.Instance.PlayerCam.Locked = true;
		RenderTexture val = (texture = RenderTexture.GetTemporary(330, 230, 16, (RenderTextureFormat)0));
		Camera.targetTexture = val;
		((Renderer)ViewPort).material.SetTexture("_MainTex", (Texture)(object)val);
		survCameras = ShipStatus.Instance.AllCameras;
		Dots = new SpriteRenderer[survCameras.Length];
		for (int i = 0; i < Dots.Length; i++)
		{
			GameObject val2 = new GameObject("Dot" + i, new Type[1] { typeof(SpriteRenderer) })
			{
				layer = ((Component)this).gameObject.layer
			};
			val2.transform.SetParent(DotParent);
			SpriteRenderer component = val2.GetComponent<SpriteRenderer>();
			component.sprite = DotDisabled;
			Dots[i] = component;
		}
		DotAligner.Align(DotParent, 0.25f, even: true);
		NextCamera(0);
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
			((Renderer)ViewPort).sharedMaterial = DefaultMaterial;
			((Renderer)ViewPort).material.SetTexture("_MainTex", (Texture)(object)texture);
			((Component)SabText).gameObject.SetActive(false);
		}
		else if (!isStatic && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			isStatic = true;
			((Renderer)ViewPort).sharedMaterial = StaticMaterial;
			((Component)SabText).gameObject.SetActive(true);
		}
	}

	public void NextCamera(int direction)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (direction != 0 && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(ChangeSound, loop: false);
		}
		Dots[currentCamera].sprite = DotDisabled;
		currentCamera = (currentCamera + direction).Wrap(survCameras.Length);
		Dots[currentCamera].sprite = DotEnabled;
		SurvCamera survCamera = survCameras[currentCamera];
		((Component)Camera).transform.position = ((Component)survCamera).transform.position + survCameras[currentCamera].Offset;
		((TMP_Text)LocationName).text = ((survCamera.NewName > StringNames.None) ? DestroyableSingleton<TranslationController>.Instance.GetString(survCamera.NewName) : survCamera.CamName);
		if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			((MonoBehaviour)this).StartCoroutine(PulseStatic());
		}
	}

	private IEnumerator PulseStatic()
	{
		((Renderer)ViewPort).sharedMaterial = StaticMaterial;
		((Renderer)ViewPort).material.SetTexture("_MainTex", (Texture)null);
		yield return Effects.Wait(0.2f);
		((Renderer)ViewPort).sharedMaterial = DefaultMaterial;
		((Renderer)ViewPort).material.SetTexture("_MainTex", (Texture)(object)texture);
		isStatic = false;
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
		texture.Release();
	}
}
