using System.Collections;
using System.Text;
using PowerTools;
using TMPro;
using UnityEngine;

public class UploadDataGame : Minigame
{
	public SpriteAnim LeftFolder;

	public SpriteAnim RightFolder;

	public AnimationClip FolderOpen;

	public AnimationClip FolderClose;

	public SpriteRenderer Runner;

	public HorizontalGauge Gauge;

	public TextMeshPro PercentText;

	public TextMeshPro EstimatedText;

	public TextMeshPro SourceText;

	public TextMeshPro TargetText;

	public SpriteRenderer Button;

	public TextTranslatorTMP translator;

	public GameObject Status;

	public GameObject Tower;

	private int count;

	private float timer;

	public const float RandomChunks = 5f;

	public const float ConstantTime = 3f;

	private bool running = true;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public override void Begin(PlayerTask task)
	{
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)Runner);
		base.Begin(task);
		if (MyNormTask.taskStep == 0)
		{
			translator.TargetText = StringNames.Download;
			Tower.SetActive(false);
			((TMP_Text)SourceText).text = DestroyableSingleton<TranslationController>.Instance.GetString(MyTask.StartAt);
			((TMP_Text)TargetText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MyTablet);
		}
		else
		{
			((TMP_Text)SourceText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MyTablet);
			((TMP_Text)TargetText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Headquarters);
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected);
	}

	public void Click()
	{
		((MonoBehaviour)this).StartCoroutine(Transition());
	}

	private IEnumerator Transition()
	{
		((Component)Button).gameObject.SetActive(false);
		Status.SetActive(true);
		float target = ((Component)Gauge).transform.localScale.x;
		for (float t = 0f; t < 0.15f; t += Time.deltaTime)
		{
			((Component)Gauge).transform.localScale = new Vector3(t / 0.15f * target, 1f, 1f);
			yield return null;
		}
		((MonoBehaviour)this).StartCoroutine(PulseText());
		((MonoBehaviour)this).StartCoroutine(DoRun());
		((MonoBehaviour)this).StartCoroutine(DoText());
		((MonoBehaviour)this).StartCoroutine(DoPercent());
	}

	private IEnumerator PulseText()
	{
		MeshRenderer rend2 = ((Component)PercentText).GetComponent<MeshRenderer>();
		MeshRenderer rend3 = ((Component)EstimatedText).GetComponent<MeshRenderer>();
		Color gray = new Color(0.3f, 0.3f, 0.3f, 1f);
		while (running)
		{
			yield return new WaitForLerp(0.4f, (float t) =>
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				Color val = Color.Lerp(Color.black, gray, t);
				((Renderer)rend2).material.SetColor("_OutlineColor", val);
				((Renderer)rend3).material.SetColor("_OutlineColor", val);
			});
			yield return new WaitForLerp(0.4f, (float t) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				Color val = Color.Lerp(gray, Color.black, t);
				((Renderer)rend2).material.SetColor("_OutlineColor", val);
				((Renderer)rend3).material.SetColor("_OutlineColor", val);
			});
		}
		((Renderer)rend2).material.SetColor("_OutlineColor", Color.black);
		((Renderer)rend3).material.SetColor("_OutlineColor", Color.black);
	}

	private IEnumerator DoPercent()
	{
		while (running)
		{
			float num = (float)count / 5f * 0.7f + timer / 3f * 0.3f;
			if (num >= 1f)
			{
				running = false;
			}
			num = Mathf.Clamp(num, 0f, 1f);
			Gauge.Value = num;
			((TMP_Text)PercentText).text = Mathf.RoundToInt(num * 100f) + "%";
			yield return null;
		}
	}

	private IEnumerator DoText()
	{
		StringBuilder txt = new StringBuilder("Estimated Time: ");
		int baselen = txt.Length;
		int max = 604800;
		count = 0;
		while ((float)count < 5f)
		{
			txt.Length = baselen;
			int num = IntRange.Next(max / 6, max);
			int num2 = num / 86400;
			int num3 = num / 3600 % 24;
			int num4 = num / 60 % 60;
			int num5 = num % 60;
			string text;
			if (num2 > 0)
			{
				text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeDHMS, num2, num3, num4, num5);
			}
			else if (num3 > 0)
			{
				text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeHMS, num3, num4, num5);
			}
			else
			{
				text = ((num4 <= 0) ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeS, num5) : DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeMS, num4, num5));
			}
			((TMP_Text)EstimatedText).text = text;
			max /= 4;
			yield return (object)new WaitForSeconds(FloatRange.Next(0.6f, 1.2f));
			count++;
		}
		for (timer = 0f; timer < 3f; timer += Time.deltaTime)
		{
			txt.Length = baselen;
			int num6 = Mathf.RoundToInt(3f - timer);
			((TMP_Text)EstimatedText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeS, num6);
			yield return null;
		}
	}

	private IEnumerator DoRun()
	{
		while (running)
		{
			LeftFolder.Play(FolderOpen);
			Vector3 pos = ((Component)Runner).transform.localPosition;
			yield return new WaitForLerp(1.125f, (float t) =>
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				pos.x = Mathf.Lerp(-1.25f, 0.5625f, t);
				((Component)Runner).transform.localPosition = pos;
			});
			LeftFolder.Play(FolderClose);
			RightFolder.Play(FolderOpen);
			yield return new WaitForLerp(1.375f, (float t) =>
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				pos.x = Mathf.Lerp(0.5625f, 1.25f, t);
				((Component)Runner).transform.localPosition = pos;
			});
			yield return new WaitForAnimationFinish(RightFolder, FolderClose);
		}
		((TMP_Text)EstimatedText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadComplete);
		MyNormTask.NextStep();
		((MonoBehaviour)this).StartCoroutine(CoStartClose());
	}
}
