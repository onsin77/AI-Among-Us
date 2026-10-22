using System.Linq;
using TMPro;
using UnityEngine;

public class HauntMenuMinigame : Minigame
{
	private enum HauntFilters
	{
		None,
		Impostor,
		Crewmate,
		Ghost
	}

	private const float MinVelMultiplier = 0.75f;

	private const float MaxVelMultiplier = 4f;

	private const float MinDistance = 0.05f;

	private const float MinOffset = 0.2f;

	private const float MaxOffset = 0.5f;

	private const float Damping = 0.7f;

	private const float Easing = 0.2f;

	[SerializeField]
	private TextMeshPro NameText;

	[SerializeField]
	private TextMeshPro FilterText;

	[SerializeField]
	private TextMeshPro HauntingText;

	private HauntFilters filterMode;

	[SerializeField]
	private PassiveButton[] FilterButtons;

	[SerializeField]
	private PassiveButton[] Arrows;

	private PlayerControl HauntTarget;

	private Vector2 Offset;

	private void Start()
	{
		if (!GameManager.Instance.RevealTeams)
		{
			((Component)FilterButtons[0]).gameObject.SetActive(false);
		}
		int num = 0;
		int num2 = FilterButtons.Count((PassiveButton s) => ((Behaviour)s).isActiveAndEnabled);
		float num3 = 1.2f * (float)num2 / 2f;
		for (int num4 = 0; num4 < FilterButtons.Length; num4++)
		{
			PassiveButton passiveButton = FilterButtons[num4];
			if (((Behaviour)passiveButton).isActiveAndEnabled)
			{
				((Component)passiveButton).transform.SetLocalX(FloatRange.SpreadToEdges(0f - num3, num3, num, num2));
				num++;
			}
		}
	}

	public override void Begin(PlayerTask task)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		DestroyableSingleton<HudManager>.Instance.ToggleMapButton(isActive: false);
		Offset = Random.insideUnitCircle;
		if (Offset.magnitude < 0.2f)
		{
			Offset = Offset.normalized * 0.2f;
		}
		else if (Offset.magnitude > 0.5f)
		{
			Offset = Offset.normalized * 0.5f;
		}
		base.Begin(task);
		Vector2 myPos = Vector2.op_Implicit(((Component)PlayerControl.LocalPlayer).transform.position);
		PlayerControl hauntTarget = PlayerControl.AllPlayerControls.Where((PlayerControl pc) => !pc.AmOwner).OrderBy((PlayerControl pc) =>
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return Vector2.Distance(Vector2.op_Implicit(((Component)pc).transform.position), myPos);
		}).First();
		SetHauntTarget(hauntTarget);
	}

	private void FixedUpdate()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		Vector2 val = Vector2.zero;
		if (Object.op_Implicit((Object)(object)HauntTarget))
		{
			Vector2 val2 = HauntTarget.GetTruePosition() + Offset;
			Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
			Vector2 val3 = PlayerControl.LocalPlayer.MyPhysics.GetVelocity() / PlayerControl.LocalPlayer.MyPhysics.TrueSpeed;
			Vector2 val4 = val2 - truePosition;
			float magnitude = val4.magnitude;
			if (magnitude > 0.05f)
			{
				val4 = val4.normalized * Mathf.Clamp(magnitude, 0.75f, 4f);
				val = val3 * 0.8f + val4 * 0.2f;
			}
			else
			{
				val *= 0.7f;
			}
		}
		PlayerControl.LocalPlayer.MyPhysics.SetNormalizedVelocity(val);
	}

	public void SetFilter(int filterInt)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		HauntFilters hauntFilters = (HauntFilters)filterInt;
		if (hauntFilters == filterMode)
		{
			hauntFilters = HauntFilters.None;
			((Component)FilterButtons[(int)(filterMode - 1)]).GetComponent<ButtonRolloverHandler>().WaitClickUnselect();
		}
		if (filterMode != HauntFilters.None)
		{
			((Component)FilterButtons[(int)(filterMode - 1)]).GetComponent<ButtonRolloverHandler>().ChangeOutColor(Color.clear);
		}
		filterMode = hauntFilters;
		if (filterMode != HauntFilters.None)
		{
			((Component)FilterButtons[(int)(filterMode - 1)]).GetComponent<ButtonRolloverHandler>().ChangeOutColor(Color.white);
		}
		ChangePick(0);
	}

	private void SetFilterText()
	{
		if (HauntTarget.Data.IsDead)
		{
			((TMP_Text)FilterText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Ghost);
		}
		else if (HauntTarget.Data.Role.IsImpostor)
		{
			((TMP_Text)FilterText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Impostor);
		}
		else
		{
			((TMP_Text)FilterText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Crewmate);
		}
	}

	public void ChangePick(int direction)
	{
		PlayerControl[] players = (from pc in PlayerControl.AllPlayerControls
			where !pc.AmOwner
			where MatchesFilter(pc)
			orderby pc.Data.GetPlayerName(PlayerOutfitType.Default)
			select pc).ToArray();
		Arrows.ForEach((PassiveButton a) =>
		{
			((Component)a).gameObject.SetActive(players.Length > 1);
		});
		if (players.Length == 0)
		{
			SetHauntTarget(null);
			return;
		}
		int num = players.IndexOf(HauntTarget);
		if (num == -1)
		{
			num = 0;
		}
		num = (num + direction).Wrap(players.Length);
		SetHauntTarget(players[num]);
	}

	private bool MatchesFilter(PlayerControl pc)
	{
		switch (filterMode)
		{
		default:
			return true;
		case HauntFilters.Impostor:
			if (pc.Data.Role.TeamType == RoleTeamTypes.Impostor)
			{
				return !pc.Data.IsDead;
			}
			return false;
		case HauntFilters.Crewmate:
			if (pc.Data.Role.TeamType == RoleTeamTypes.Crewmate || (pc.Data.Role.TeamType == RoleTeamTypes.Impostor && GameManager.Instance.LogicOptions is LogicOptionsNormal))
			{
				return !pc.Data.IsDead;
			}
			return false;
		case HauntFilters.Ghost:
			return pc.Data.IsDead;
		}
	}

	private void SetHauntTarget(PlayerControl target)
	{
		if ((Object)(object)target == (Object)null)
		{
			HauntTarget = null;
			((TMP_Text)NameText).text = "";
			((TMP_Text)FilterText).text = "";
			((Behaviour)HauntingText).enabled = false;
		}
		else
		{
			HauntTarget = target;
			((Behaviour)HauntingText).enabled = true;
			((TMP_Text)NameText).text = target.Data?.GetPlayerName(PlayerOutfitType.Default);
			SetFilterText();
		}
	}

	public override void Close()
	{
		DestroyableSingleton<HudManager>.Instance.ToggleMapButton(isActive: true);
		DestroyableSingleton<HudManager>.Instance.AbilityButton.SetEnabled();
		base.Close();
	}
}
