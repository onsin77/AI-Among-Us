using System.Collections;
using System.Linq;
using Rewired;
using UnityEngine;

public class EmptyGarbageMinigame : Minigame
{
	private const float GrinderVolume = 0.8f;

	public FloatRange HandleRange = new FloatRange(-0.65f, 0.65f);

	public Vector2Range SpawnRange;

	public Collider2D Blocker;

	public AreaEffector2D Popper;

	public Collider2D Handle;

	public SpriteRenderer Bars;

	public Transform VibratePivot;

	private Controller controller = new Controller();

	private bool finished;

	public int NumObjects = 15;

	private SpriteRenderer[] Objects;

	public SpriteRenderer[] GarbagePrefabs;

	public SpriteRenderer[] LeafPrefabs;

	public SpriteRenderer[] SpecialObjectPrefabs;

	public AudioClip LeverDown;

	public AudioClip LeverUp;

	public AudioClip GrinderStart;

	public AudioClip GrinderLoop;

	public AudioClip GrinderEnd;

	private TouchpadBehavior touchpad;

	private bool hadInput;

	private float leverInput;

	private Coroutine popCoroutine;

	private Coroutine animateCoroutine;

	public override void Begin(PlayerTask task)
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		int i = 0;
		Objects = new SpriteRenderer[NumObjects];
		RandomFill<SpriteRenderer> randomFill = new RandomFill<SpriteRenderer>();
		if (MyNormTask.StartAt == SystemTypes.LifeSupp)
		{
			randomFill.Set(GarbagePrefabs.Union(LeafPrefabs));
		}
		else
		{
			NormalPlayerTask myNormTask = MyNormTask;
			if (myNormTask != null && myNormTask.taskStep == 0)
			{
				if (MyNormTask.TaskType == TaskTypes.EmptyChute)
				{
					randomFill.Set(GarbagePrefabs);
				}
				else
				{
					randomFill.Set(LeafPrefabs);
				}
			}
			else
			{
				randomFill.Set(GarbagePrefabs.Union(LeafPrefabs));
				for (; i < SpecialObjectPrefabs.Length; i++)
				{
					SpriteRenderer val = (Objects[i] = Object.Instantiate<SpriteRenderer>(SpecialObjectPrefabs[i]));
					((Component)val).transform.SetParent(((Component)this).transform);
					((Component)val).transform.localPosition = Vector2.op_Implicit(SpawnRange.Next());
				}
			}
		}
		for (; i < Objects.Length; i++)
		{
			SpriteRenderer val2 = (Objects[i] = Object.Instantiate<SpriteRenderer>(randomFill.Get()));
			((Component)val2).transform.SetParent(((Component)this).transform);
			Vector3 val3 = Vector2.op_Implicit(SpawnRange.Next());
			val3.z = FloatRange.Next(-0.5f, 0.5f);
			((Component)val2).transform.localPosition = val3;
			val2.color = Color.Lerp(Color.white, Color.black, (val3.z + 0.5f) * 0.7f);
		}
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	private void StopCoroutines()
	{
		if (popCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(popCoroutine);
			popCoroutine = null;
		}
		if (animateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(animateCoroutine);
			animateCoroutine = null;
		}
	}

	public void Update()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		controller.Update();
		Vector3 localPosition = ((Component)Handle).transform.localPosition;
		float num = HandleRange.ReverseLerp(localPosition.y);
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (!finished)
			{
				Player player = ReInput.players.GetPlayer(0);
				if (touchpad.IsTouching())
				{
					leverInput = 0f - touchpad.GetTouchVector().y;
				}
				else
				{
					leverInput = Mathf.Clamp01(0f - player.GetAxis(17));
				}
				localPosition.y = HandleRange.Lerp(1f - leverInput);
				num = HandleRange.ReverseLerp(localPosition.y);
				if (leverInput >= 0.01f)
				{
					hadInput = true;
					if (num <= 0.5f && ((Behaviour)Blocker).enabled)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(LeverDown, loop: false);
							SoundManager.Instance.PlaySound(GrinderStart, loop: false, 0.8f);
							SoundManager.Instance.StopSound(GrinderEnd);
							SoundManager.Instance.StopSound(GrinderLoop);
						}
						((Behaviour)Blocker).enabled = false;
						StopCoroutines();
						popCoroutine = ((MonoBehaviour)this).StartCoroutine(PopObjects());
						animateCoroutine = ((MonoBehaviour)this).StartCoroutine(AnimateObjects());
					}
				}
				else
				{
					if (hadInput)
					{
						if (!((Behaviour)Blocker).enabled)
						{
							((Behaviour)Blocker).enabled = true;
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(LeverUp, loop: false);
								SoundManager.Instance.StopSound(GrinderStart);
								SoundManager.Instance.StopSound(GrinderLoop);
								SoundManager.Instance.PlaySound(GrinderEnd, loop: false, 0.8f);
							}
						}
						if (!finished && Objects.All((SpriteRenderer o) => !Object.op_Implicit((Object)(object)o)))
						{
							finished = true;
							MyNormTask.NextStep();
							((MonoBehaviour)this).StartCoroutine(CoStartClose());
						}
					}
					hadInput = false;
				}
			}
		}
		else
		{
			switch (controller.CheckDrag(Handle))
			{
			case DragState.Dragging:
				if (finished)
				{
					break;
				}
				if (num > 0.5f)
				{
					Vector2 val = controller.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
					float num2 = HandleRange.ReverseLerp(HandleRange.Clamp(val.y));
					localPosition.y = HandleRange.Lerp(num2 / 2f + 0.5f);
					break;
				}
				localPosition.y = Mathf.Lerp(localPosition.y, HandleRange.min, num + Time.deltaTime * 15f);
				if (((Behaviour)Blocker).enabled)
				{
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(LeverDown, loop: false);
						SoundManager.Instance.PlaySound(GrinderStart, loop: false, 0.8f);
						SoundManager.Instance.StopSound(GrinderEnd);
						SoundManager.Instance.StopSound(GrinderLoop);
					}
					((Behaviour)Blocker).enabled = false;
					StopCoroutines();
					popCoroutine = ((MonoBehaviour)this).StartCoroutine(PopObjects());
					animateCoroutine = ((MonoBehaviour)this).StartCoroutine(AnimateObjects());
				}
				break;
			case DragState.NoTouch:
				localPosition.y = Mathf.Lerp(localPosition.y, HandleRange.max, num + Time.deltaTime * 15f);
				break;
			case DragState.Released:
				if (!((Behaviour)Blocker).enabled)
				{
					((Behaviour)Blocker).enabled = true;
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(LeverUp, loop: false);
						SoundManager.Instance.StopSound(GrinderStart);
						SoundManager.Instance.StopSound(GrinderLoop);
						SoundManager.Instance.PlaySound(GrinderEnd, loop: false, 0.8f);
					}
				}
				if (!finished && Objects.All((SpriteRenderer o) => !Object.op_Implicit((Object)(object)o)))
				{
					finished = true;
					MyNormTask.NextStep();
					((MonoBehaviour)this).StartCoroutine(CoStartClose());
				}
				break;
			}
		}
		if (Constants.ShouldPlaySfx() && !((Behaviour)Blocker).enabled && !SoundManager.Instance.SoundIsPlaying(GrinderStart))
		{
			SoundManager.Instance.PlaySound(GrinderLoop, loop: true, 0.8f, SoundManager.Instance.SfxChannel);
		}
		((Component)Handle).transform.localPosition = localPosition;
		Vector3 localScale = ((Component)Bars).transform.localScale;
		localScale.y = HandleRange.ChangeRange(localPosition.y, -1f, 1f);
		((Component)Bars).transform.localScale = localScale;
	}

	private IEnumerator PopObjects()
	{
		((Behaviour)Popper).enabled = true;
		yield return (object)new WaitForSeconds(0.05f);
		((Behaviour)Popper).enabled = false;
		popCoroutine = null;
	}

	private IEnumerator AnimateObjects()
	{
		Vector3 pos = VibratePivot.localPosition;
		for (float t = 3f; t > 0f; t -= Time.deltaTime)
		{
			float num = t / 3f;
			float num2 = num * 0.1f * 3f;
			VibrationManager.Vibrate(num2, num2, 0.01f);
			VibratePivot.localPosition = pos + Vector2.op_Implicit(Vector2Range.NextEdge()) * num * 0.1f;
			yield return null;
		}
		animateCoroutine = null;
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(GrinderStart);
		SoundManager.Instance.StopSound(GrinderLoop);
		SoundManager.Instance.StopSound(GrinderEnd);
		if (Object.op_Implicit((Object)(object)MyNormTask) && MyNormTask.IsComplete)
		{
			ShipStatus.Instance.OpenHatch();
			PlayerControl.LocalPlayer.RpcPlayAnimation((byte)MyTask.TaskType);
		}
		base.Close();
	}
}
