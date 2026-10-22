using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WeaponsMinigame : Minigame
{
	public FloatRange XSpan = new FloatRange(-1.15f, 1.15f);

	public FloatRange YSpan = new FloatRange(-1.15f, 1.15f);

	public FloatRange TimeToSpawn;

	public ObjectPoolBehavior asteroidPool;

	public TextMeshPro ScoreText;

	public SpriteRenderer TargetReticle;

	public LineRenderer TargetLines;

	private Vector3 TargetCenter;

	public Collider2D BackgroundCol;

	public SpriteRenderer Background;

	public Controller myController = new Controller();

	private float Timer;

	public AudioClip ShootSound;

	public AudioClip[] ExplodeSounds;

	public override void Begin(PlayerTask task)
	{
		SetupInput(disableCursor: false);
		base.Begin(task);
		((TMP_Text)ScoreText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AstDestroyed, MyNormTask.taskStep);
		TimeToSpawn.Next();
	}

	protected override IEnumerator CoAnimateOpen()
	{
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num = timer / 0.1f;
			((Component)this).transform.localScale = new Vector3(num, 0.1f, num);
			yield return null;
		}
		for (float timer = 0.010000001f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num2 = timer / 0.1f;
			((Component)this).transform.localScale = new Vector3(1f, num2, 1f);
			yield return null;
		}
		((Component)this).transform.localScale = new Vector3(1f, 1f, 1f);
	}

	protected override IEnumerator CoDestroySelf()
	{
		for (float timer = 0.010000001f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num = 1f - timer / 0.1f;
			((Component)this).transform.localScale = new Vector3(1f, num, 1f);
			yield return null;
		}
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num2 = 1f - timer / 0.1f;
			((Component)this).transform.localScale = new Vector3(num2, 0.1f, num2);
			yield return null;
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void FixedUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected Obj, but got Unknown
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		Background.color = Color.Lerp(Palette.ClearWhite, Color.white, Mathf.Sin(Time.time * 3f) * 0.1f + 0.79999995f);
		if (Object.op_Implicit((Object)(object)MyNormTask) && MyNormTask.IsComplete)
		{
			return;
		}
		Timer += Time.fixedDeltaTime;
		if (Timer >= TimeToSpawn.Last)
		{
			Timer = 0f;
			TimeToSpawn.Next();
			if (asteroidPool.InUse < MyNormTask.MaxStep - MyNormTask.TaskStep)
			{
				Asteroid ast = asteroidPool.Get<Asteroid>();
				((Component)ast).transform.localPosition = new Vector3(XSpan.max, YSpan.Next(), -1f);
				ast.TargetPosition = new Vector3(XSpan.min, YSpan.Next(), -1f);
				((UnityEvent)((Component)ast).GetComponent<ButtonBehavior>().OnClick).AddListener((UnityAction)(() =>
				{
					BreakApart(ast);
				}));
			}
		}
		myController.Update();
		bool flag = myController.CheckHover(BackgroundCol);
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (flag)
			{
				Vector3 val = Vector2.op_Implicit(myController.HoverPosition) - ((Component)this).transform.position;
				val.z = -2f;
				((Component)TargetReticle).transform.localPosition = val;
				val.z = 0f;
				TargetLines.SetPosition(1, val);
			}
			else
			{
				Bounds bounds = BackgroundCol.bounds;
				Vector3 val2 = Vector2.op_Implicit(myController.HoverPosition);
				val2.x = Mathf.Clamp(val2.x, bounds.min.x, bounds.max.x);
				val2.y = Mathf.Clamp(val2.y, bounds.min.y, bounds.max.y);
				VirtualCursor.instance.SetWorldPosition(Vector2.op_Implicit(val2));
				val2 -= ((Component)this).transform.position;
				val2.z = -2f;
				((Component)TargetReticle).transform.localPosition = val2;
				val2.z = 0f;
				TargetLines.SetPosition(1, val2);
			}
		}
		if (myController.CheckDrag(BackgroundCol) == DragState.TouchStart)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ShootSound, loop: false);
			}
			Vector3 val3 = Vector2.op_Implicit(myController.DragPosition) - ((Component)this).transform.position;
			val3.z = -2f;
			((Component)TargetReticle).transform.localPosition = val3;
			val3.z = 0f;
			TargetLines.SetPosition(1, val3);
			if (Object.op_Implicit((Object)(object)ShipStatus.Instance.WeaponsImage) && !ShipStatus.Instance.WeaponsImage.IsPlaying())
			{
				PlayerControl.LocalPlayer.RpcPlayAnimation(6);
			}
		}
	}

	public void BreakApart(Asteroid ast)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(ExplodeSounds.Random(), loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		if (MyNormTask.IsComplete)
		{
			return;
		}
		((MonoBehaviour)this).StartCoroutine(ast.CoBreakApart());
		if (Object.op_Implicit((Object)(object)MyNormTask))
		{
			MyNormTask.NextStep();
			((TMP_Text)ScoreText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AstDestroyed, MyNormTask.taskStep);
		}
		if (!Object.op_Implicit((Object)(object)MyNormTask) || !MyNormTask.IsComplete)
		{
			return;
		}
		((MonoBehaviour)this).StartCoroutine(CoStartClose());
		foreach (Asteroid activeChild in asteroidPool.activeChildren)
		{
			if (!((Object)(object)activeChild == (Object)(object)ast))
			{
				((MonoBehaviour)this).StartCoroutine(activeChild.CoBreakApart());
			}
		}
	}
}
