using PowerTools;
using Rewired;
using UnityEngine;

public class LeafMinigame : Minigame
{
	public LeafBehaviour LeafPrefab;

	public Vector2Range ValidArea;

	public SpriteAnim[] Arrows;

	public AnimationClip[] Inactive;

	public AnimationClip[] Active;

	public AnimationClip[] Complete;

	private Collider2D[] Leaves;

	public AudioClip[] LeaveSounds;

	public AudioClip[] SuckSounds;

	private Controller myController = new Controller();

	public Transform interactionCursor;

	public Transform interactionCursorCenterDot;

	private Collider2D[] overlapResults;

	private bool prevHadOverlaps;

	private bool prevRightStickInput;

	public override void Begin(PlayerTask task)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		Leaves = new Collider2D[MyNormTask.MaxStep - MyNormTask.taskStep];
		for (int i = 0; i < Leaves.Length; i++)
		{
			LeafBehaviour leafBehaviour = Object.Instantiate<LeafBehaviour>(LeafPrefab);
			((Component)leafBehaviour).transform.SetParent(((Component)this).transform);
			leafBehaviour.Parent = this;
			Vector3 localPosition = Vector2.op_Implicit(ValidArea.Next());
			localPosition.z = -1f;
			((Component)leafBehaviour).transform.localPosition = localPosition;
			Leaves[i] = ((Component)leafBehaviour).GetComponent<Collider2D>();
		}
		overlapResults = new Collider2D[Leaves.Length * 3];
		SetupInput(disableCursor: true);
	}

	public void FixedUpdate()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		myController.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 val = new Vector2(player.GetAxis(13), player.GetAxis(14));
			Vector2 val2 = new Vector2(player.GetAxis(16), player.GetAxis(17));
			if (val2.sqrMagnitude > 1f)
			{
				val2 = val2.normalized;
			}
			Vector3 localPosition = interactionCursor.localPosition;
			Vector3 localPosition2 = interactionCursorCenterDot.localPosition;
			localPosition.x += val.x * Time.deltaTime * 5f;
			localPosition.y += val.y * Time.deltaTime * 5f;
			localPosition.x = Mathf.Clamp(localPosition.x, ValidArea.min.x, ValidArea.max.x);
			localPosition.y = Mathf.Clamp(localPosition.y, ValidArea.min.y, ValidArea.max.y);
			localPosition2.x = val2.x;
			localPosition2.y = val2.y;
			interactionCursor.localPosition = localPosition;
			interactionCursorCenterDot.localPosition = localPosition2;
			int num = Physics2D.OverlapCircleNonAlloc(Vector2.op_Implicit(interactionCursor.position), ((Component)interactionCursor).transform.localScale.x, overlapResults, LayerMask.GetMask(new string[1] { "UI" }));
			int num2 = 0;
			bool flag = val2.sqrMagnitude > 0.5f;
			bool flag2 = flag && !prevRightStickInput;
			for (int i = 0; i < num; i++)
			{
				LeafBehaviour component = ((Component)overlapResults[i]).GetComponent<LeafBehaviour>();
				if (!Object.op_Implicit((Object)(object)component))
				{
					continue;
				}
				num2++;
				if (flag2)
				{
					flag2 = false;
					component.body.velocity = val2.normalized * Mathf.Min(3f, val2.magnitude * 3f);
					VibrationManager.Vibrate(0.15f, 0.15f, 0.15f);
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(LeaveSounds.Random(), loop: false);
					}
				}
			}
			prevRightStickInput = flag;
			if (num2 > 0)
			{
				if (!prevHadOverlaps)
				{
					for (int j = 0; j < Arrows.Length; j++)
					{
						Arrows[j].Play(Active[j]);
					}
				}
				prevHadOverlaps = true;
				return;
			}
			if (prevHadOverlaps)
			{
				for (int k = 0; k < Arrows.Length; k++)
				{
					Arrows[k].Play(Inactive[k]);
					((Component)Arrows[k]).GetComponent<SpriteRenderer>().sprite = null;
				}
			}
			prevHadOverlaps = false;
			return;
		}
		for (int l = 0; l < Leaves.Length; l++)
		{
			Collider2D val3 = Leaves[l];
			if (!Object.op_Implicit((Object)(object)val3))
			{
				continue;
			}
			LeafBehaviour component2 = ((Component)val3).GetComponent<LeafBehaviour>();
			switch (myController.CheckDrag(val3))
			{
			case DragState.TouchStart:
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(LeaveSounds.Random(), loop: false);
				}
				for (int n = 0; n < Arrows.Length; n++)
				{
					Arrows[n].Play(Active[n]);
				}
				component2.Held = true;
				break;
			}
			case DragState.Dragging:
			{
				Vector2 val4 = myController.DragPosition - component2.body.position;
				component2.body.velocity = val4.normalized * Mathf.Min(3f, val4.magnitude * 3f);
				break;
			}
			case DragState.Released:
			{
				component2.Held = false;
				for (int m = 0; m < Arrows.Length; m++)
				{
					Arrows[m].Play(Inactive[m]);
					((Component)Arrows[m]).GetComponent<SpriteRenderer>().sprite = null;
				}
				break;
			}
			}
		}
	}

	public void LeafDone(LeafBehaviour leaf)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(SuckSounds.Random(), loop: false);
		}
		VibrationManager.Vibrate(0.15f, 0f, 0.25f);
		Object.Destroy((Object)(object)((Component)leaf).gameObject);
		if (!Object.op_Implicit((Object)(object)MyNormTask))
		{
			return;
		}
		MyNormTask.NextStep();
		if (MyNormTask.IsComplete)
		{
			for (int i = 0; i < Arrows.Length; i++)
			{
				Arrows[i].Play(Complete[i]);
			}
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
	}
}
