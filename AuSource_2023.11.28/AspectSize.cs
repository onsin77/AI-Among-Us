using UnityEngine;

public class AspectSize : MonoBehaviour
{
	public Sprite Background;

	public SpriteRenderer Renderer;

	private Vector2 lastScreenSize = Vector2.zero;

	public float PercentWidth = 0.95f;

	private Vector3? originalScale;

	private void OnValidate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.localScale != Vector3.one)
		{
			Debug.LogWarning((object)("Warning! AspectSize should be placed on an object with a local scale of (1,1). Otherwise it will not work correctly. Gameobject name: " + ((Object)this).name));
		}
	}

	public static float CalculateSize(Vector3 parentPos, Sprite sprite, float percentWidth)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		float num = CameraSafeArea.GetSafeOrthographicSize(main) * main.aspect + parentPos.x;
		Bounds bounds = sprite.bounds;
		float x = bounds.size.x;
		float num2 = num / x * percentWidth;
		return Mathf.Min(1f, num2);
	}

	private void Update()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (lastScreenSize.x != (float)Screen.width || lastScreenSize.y != (float)Screen.height)
		{
			lastScreenSize = new Vector2((float)Screen.width, (float)Screen.height);
			DoSetUp();
		}
	}

	private void DoSetUp()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!originalScale.HasValue)
		{
			originalScale = ((Component)this).transform.localScale;
		}
		Camera main = Camera.main;
		AdjustScale(main.aspect);
		ResolutionManager.ResolutionChanged += OnResolutionChanged;
	}

	private void OnEnable()
	{
		DoSetUp();
	}

	private void OnDisable()
	{
		ResolutionManager.ResolutionChanged -= OnResolutionChanged;
	}

	private void OnDestroy()
	{
		ResolutionManager.ResolutionChanged -= OnResolutionChanged;
	}

	private void OnResolutionChanged(float aspectRatio, int width, int height, bool fullscreen)
	{
		AdjustScale(aspectRatio);
	}

	private void AdjustScale(float aspect)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null)
		{
			ResolutionManager.ResolutionChanged -= OnResolutionChanged;
			return;
		}
		((Component)this).transform.localScale = originalScale.Value;
		Camera main = Camera.main;
		float scale = GetScale(main.orthographicSize, main.aspect);
		if (scale < 1f)
		{
			((Component)this).transform.localScale = new Vector3(scale, scale, scale);
		}
	}

	private float GetScale(float orthographicSize, float aspect)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		float num = orthographicSize * aspect;
		Bounds bounds;
		Vector3 val;
		if (Object.op_Implicit((Object)(object)Background))
		{
			bounds = Background.bounds;
			val = bounds.size;
		}
		else
		{
			if ((int)Renderer.drawMode != 0)
			{
				val = Vector2.op_Implicit(Renderer.size);
			}
			else
			{
				bounds = Renderer.sprite.bounds;
				val = bounds.size;
			}
			val = ((Component)Renderer).transform.TransformVector(val);
		}
		float num2 = val.x / 2f;
		return num / num2 * PercentWidth;
	}

	public AspectSize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
