using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeetingIntroAnimation : MonoBehaviour
{
	public Transform VoteButtonParent;

	public Transform OverlayParent;

	public Transform DeadParent;

	public GameObject ProtectedRecently;

	public TextMeshPro DeadBodiesText;

	public SpriteRenderer BloodSplat;

	public Vector3 VoteButtonParentPos = new Vector3(0.25f, 0f, 0f);

	public Vector3 ReporterPos;

	public SpriteRenderer background;

	public AnimationCurve SlamCurve;

	public PlayerVoteArea VoteAreaPrefab;

	public AudioClip PlayerDeadSound;

	public AudioClip ProtectedRecentlySound;

	private List<PlayerVoteArea> deadCards = new List<PlayerVoteArea>();

	public SpriteRenderer[] OutsideMasks;

	public void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		VoteButtonParent.localPosition = Vector3.right * 20f;
		DeadParent.localPosition = Vector3.right * 20f;
		SpriteRenderer[] outsideMasks = OutsideMasks;
		for (int i = 0; i < outsideMasks.Length; i++)
		{
			((Renderer)outsideMasks[i]).material.SetInt(PlayerMaterial.MaskLayer, 254);
		}
	}

	public void Init(GameData.PlayerInfo reporter, GameData.PlayerInfo[] deadBodies)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		PlayerVoteArea playerVoteArea = Object.Instantiate<PlayerVoteArea>(VoteAreaPrefab, OverlayParent);
		((Component)playerVoteArea).transform.localPosition = ReporterPos;
		playerVoteArea.SetMaskLayer(0);
		playerVoteArea.SetCosmetics(reporter);
		playerVoteArea.SetDead(didReport: true, isDead: false);
		float num = background.size.x / 2f;
		Bounds bounds = ((Renderer)VoteAreaPrefab.MaskArea).bounds;
		float num2 = bounds.size.y + 0.2f;
		bounds = ((Renderer)VoteAreaPrefab.MaskArea).bounds;
		float x = bounds.extents.x;
		int num3 = Mathf.CeilToInt((float)deadBodies.Length / 3f);
		for (int i = 0; i < deadBodies.Length; i += 3)
		{
			float num4 = (float)(num3 / 2 - i / 3) * num2;
			int num5 = Mathf.Min(deadBodies.Length - i, 3);
			for (int j = 0; j < num5; j++)
			{
				int num6 = i + j;
				float num7 = FloatRange.SpreadEvenly(0f - num - x, num + x, j, num5);
				PlayerVoteArea playerVoteArea2 = Object.Instantiate<PlayerVoteArea>(VoteAreaPrefab, DeadParent);
				((Component)playerVoteArea2).transform.localPosition = new Vector3(num7, num4, 0f);
				playerVoteArea2.SetMaskLayer(num6);
				playerVoteArea2.SetCosmetics(deadBodies[num6]);
				playerVoteArea2.SetDead(didReport: false, isDead: true);
				deadCards.Add(playerVoteArea2);
			}
		}
		if (deadBodies.Length == 0)
		{
			((TMP_Text)DeadBodiesText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoDeadBodiesFound);
			DeadBodiesText.transform.localPosition = Vector3.zero;
			((Renderer)BloodSplat).enabled = false;
		}
		bool someoneWasProtected = false;
		PlayerControl.AllPlayerControls.ForEach((PlayerControl pc) =>
		{
			if (pc.protectedByGuardianThisRound)
			{
				pc.protectedByGuardianThisRound = false;
				if (pc.Data != null && !pc.Data.IsDead)
				{
					someoneWasProtected = true;
				}
			}
		});
		if (someoneWasProtected)
		{
			SoundManager.Instance.PlaySound(ProtectedRecentlySound, loop: false);
		}
		ProtectedRecently.SetActive(someoneWasProtected);
	}

	public IEnumerator CoRun()
	{
		((Component)this).gameObject.SetActive(true);
		foreach (PlayerVoteArea deadCard in deadCards)
		{
			((MonoBehaviour)this).StartCoroutine(deadCard.CoAnimateOverlay());
		}
		DeadParent.localPosition = Vector3.zero;
		yield return Effects.Lerp(0.2f, (float t) =>
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			((Component)DeadParent).transform.localScale = Vector3.one * Mathf.LerpUnclamped(8f, 1f, SlamCurve.Evaluate(t));
		});
		if (deadCards.Count > 0)
		{
			SoundManager.Instance.PlaySound(PlayerDeadSound, loop: false);
		}
		yield return Effects.Wait(3f);
		yield return Effects.All(Effects.Slide2D(VoteButtonParent, Vector2.right * 20f, Vector2.op_Implicit(VoteButtonParentPos)), Effects.Slide2D(OverlayParent, Vector2.zero, Vector2.left * 20f));
		((Component)this).gameObject.SetActive(false);
	}

	public MeetingIntroAnimation()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
