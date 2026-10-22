using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationPopper : MonoBehaviour
{
	public TextMeshPro TextArea;

	public float zPos = -350f;

	private float alphaTimer;

	public float ShowDuration = 5f;

	public float FadeDuration = 1f;

	public Color textColor = Color.white;

	public AudioClip NotificationSound;

	private Camera mainCamera;

	private readonly StringBuilder builder = new StringBuilder();

	public void Update()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (alphaTimer > 0f)
		{
			if ((Object)(object)mainCamera == (Object)null)
			{
				mainCamera = Camera.main;
			}
			float num = mainCamera.orthographicSize * mainCamera.aspect;
			Bounds bounds = ((TMP_Text)DestroyableSingleton<HudManager>.Instance.GameSettings).bounds;
			float y = bounds.size.y;
			Transform transform = DestroyableSingleton<HudManager>.Instance.GameSettings.transform;
			((Component)this).transform.localPosition = new Vector3(0f - num + 0.1f, transform.localPosition.y - y, zPos);
			alphaTimer -= Time.deltaTime;
			textColor.a = Mathf.Clamp(alphaTimer / FadeDuration, 0f, 1f);
			((Graphic)TextArea).color = textColor;
			if (alphaTimer <= 0f)
			{
				builder.Clear();
				((TMP_Text)TextArea).text = string.Empty;
			}
		}
	}

	public void AddItem(string item)
	{
		builder.AppendLine(item);
		((TMP_Text)TextArea).text = builder.ToString();
		alphaTimer = ShowDuration;
		SoundManager.Instance.PlaySound(NotificationSound, loop: false);
	}

	public NotificationPopper()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
	}
}
