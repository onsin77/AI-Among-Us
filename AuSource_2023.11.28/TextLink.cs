using UnityEngine;

public class TextLink : MonoBehaviour
{
	public BoxCollider2D boxCollider;

	public string targetUrl;

	public bool needed;

	public void Set(Vector2 from, Vector2 to, string target)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		targetUrl = target;
		Vector2 val = to + from;
		((Component)this).transform.localPosition = new Vector3(val.x / 2f, val.y / 2f, -1f);
		val = to - from;
		val.y = 0f - val.y;
		boxCollider.size = val;
	}

	public void Click()
	{
		if (targetUrl == "httpstore")
		{
			DestroyableSingleton<StoreMenu>.Instance.Open();
		}
		else
		{
			Application.OpenURL(targetUrl);
		}
	}
}
