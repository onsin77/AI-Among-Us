using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroCutscene : MonoBehaviour
{
	private const float BaseY = -1f;

	private const float ScaleAll = 1f;

	private const float OffsetWidth = 0.9f;

	private const float OffsetHeight = 0.15f;

	public static IntroCutscene Instance;

	public TextMeshPro TeamTitle;

	public TextMeshPro ImpostorText;

	public TextMeshPro YouAreText;

	public TextMeshPro RoleText;

	public TextMeshPro RoleBlurbText;

	public TextMeshPro HideAndSeekTimerText;

	public PoolablePlayer PlayerPrefab;

	public PoolablePlayer OurCrewmate;

	public GameObject HideAndSeekPanels;

	public GameObject CrewmateRules;

	public GameObject ImpostorRules;

	public MeshRenderer BackgroundBar;

	public MeshRenderer Foreground;

	public FloatRange ForegroundRadius;

	public SpriteRenderer FrontMost;

	public AudioClip IntroStinger;

	[Header("HnS Temp")]
	public TextMeshPro ImpostorName;

	public TextMeshPro ImpostorTitle;

	private Vector3 impostorPos = new Vector3(-1.23f, -1.46f, -24f);

	private float impostorScale = 1f;

	[SerializeField]
	private AnimationClip HnSSeekerSpawnAnim;

	[SerializeField]
	private AnimationClip HnSSeekerSpawnHorseAnim;

	[SerializeField]
	private AnimationClip HnSSeekerSpawnHorseInGameAnim;

	public PoolablePlayer HideAndSeekPlayerVisual;

	public PoolablePlayer HorseWrangleVisualSuit;

	public PoolablePlayer HorseWrangleVisualPlayer;

	private PoolablePlayer ourCrewmate;

	private DualshockLightManager.LightOverlayHandle overlayHandle;

	public IEnumerator CoBegin()
	{
		Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Starting intro cutscene");
		SoundManager.Instance.PlaySound(IntroStinger, loop: false);
		if (GameManager.Instance.IsNormal())
		{
			Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Game Mode: Normal");
			LogPlayerRoleData();
			HideAndSeekPanels.SetActive(false);
			CrewmateRules.SetActive(false);
			ImpostorRules.SetActive(false);
			((Component)ImpostorName).gameObject.SetActive(false);
			((Component)ImpostorTitle).gameObject.SetActive(false);
			List<PlayerControl> list = SelectTeamToShow((GameData.PlayerInfo pcd) => !PlayerControl.LocalPlayer.Data.Role.IsImpostor || pcd.Role.TeamType == PlayerControl.LocalPlayer.Data.Role.TeamType);
			if (list == null || list.Count < 1)
			{
				Logger.GlobalInstance.Error("IntroCutscene :: CoBegin() :: teamToShow is EMPTY or NULL");
			}
			if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
			{
				((Component)ImpostorText).gameObject.SetActive(false);
			}
			else
			{
				int adjustedNumImpostors = GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
				if (adjustedNumImpostors == 1)
				{
					((TMP_Text)ImpostorText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsS);
				}
				else
				{
					((TMP_Text)ImpostorText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, adjustedNumImpostors);
				}
				((TMP_Text)ImpostorText).text = ((TMP_Text)ImpostorText).text.Replace("[FF1919FF]", "<color=#FF1919FF>");
				((TMP_Text)ImpostorText).text = ((TMP_Text)ImpostorText).text.Replace("[]", "</color>");
			}
			yield return ShowTeam(list, 3f);
			yield return ShowRole();
		}
		else
		{
			Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Game Mode: Hide and Seek");
			LogPlayerRoleData();
			HideAndSeekPanels.SetActive(true);
			if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
			{
				CrewmateRules.SetActive(false);
				ImpostorRules.SetActive(true);
			}
			else
			{
				CrewmateRules.SetActive(true);
				ImpostorRules.SetActive(false);
			}
			List<PlayerControl> list2 = SelectTeamToShow((GameData.PlayerInfo pcd) => PlayerControl.LocalPlayer.Data.Role.IsImpostor != pcd.Role.IsImpostor);
			if (list2 == null || list2.Count < 1)
			{
				Logger.GlobalInstance.Error("IntroCutscene :: CoBegin() :: teamToShow is EMPTY or NULL");
			}
			PlayerControl impostor = PlayerControl.AllPlayerControls.Find((PlayerControl pc) => pc.Data.Role.IsImpostor);
			if ((Object)(object)impostor == (Object)null)
			{
				Logger.GlobalInstance.Error("IntroCutscene :: CoBegin() :: impostor is NULL");
			}
			GameManager.Instance.SetSpecialCosmetics(impostor);
			((Component)ImpostorName).gameObject.SetActive(true);
			((Component)ImpostorTitle).gameObject.SetActive(true);
			((Renderer)BackgroundBar).enabled = false;
			((Component)TeamTitle).gameObject.SetActive(false);
			if ((Object)(object)impostor != (Object)null)
			{
				((TMP_Text)ImpostorName).text = impostor.Data.PlayerName;
			}
			else
			{
				((TMP_Text)ImpostorName).text = "???";
			}
			yield return (object)new WaitForSecondsRealtime(0.1f);
			PoolablePlayer playerSlot = null;
			if ((Object)(object)impostor != (Object)null)
			{
				playerSlot = CreatePlayer(1, 1, impostor.Data, impostorPositioning: false);
				playerSlot.SetBodyType(PlayerBodyTypes.Normal);
				playerSlot.SetFlipX(flipped: false);
				((Component)playerSlot).transform.localPosition = impostorPos;
				((Component)playerSlot).transform.localScale = Vector3.one * impostorScale;
			}
			yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
			yield return (object)new WaitForSecondsRealtime(6f);
			if ((Object)(object)playerSlot != (Object)null)
			{
				((Component)playerSlot).gameObject.SetActive(false);
			}
			HideAndSeekPanels.SetActive(false);
			CrewmateRules.SetActive(false);
			ImpostorRules.SetActive(false);
			LogicOptionsHnS logicOptionsHnS = GameManager.Instance.LogicOptions as LogicOptionsHnS;
			(GameManager.Instance.GetLogicComponent<LogicHnSMusic>() as LogicHnSMusic)?.StartMusicWithIntro();
			if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
			{
				float crewmateLeadTime = logicOptionsHnS.GetCrewmateLeadTime();
				((Component)HideAndSeekTimerText).gameObject.SetActive(true);
				PoolablePlayer poolablePlayer;
				AnimationClip anim;
				if (Constants.ShouldHorseAround())
				{
					poolablePlayer = HorseWrangleVisualSuit;
					((Component)poolablePlayer).gameObject.SetActive(true);
					poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
					anim = HnSSeekerSpawnHorseAnim;
					HorseWrangleVisualPlayer.SetBodyType(PlayerBodyTypes.Normal);
					HorseWrangleVisualPlayer.UpdateFromPlayerData(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.CurrentOutfitType, PlayerMaterial.MaskType.None, includePet: false);
				}
				else
				{
					poolablePlayer = HideAndSeekPlayerVisual;
					((Component)poolablePlayer).gameObject.SetActive(true);
					poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
					anim = HnSSeekerSpawnAnim;
				}
				poolablePlayer.UpdateFromPlayerData(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.CurrentOutfitType, PlayerMaterial.MaskType.None, includePet: false);
				SpriteAnim component = ((Component)poolablePlayer).GetComponent<SpriteAnim>();
				((Component)poolablePlayer).gameObject.SetActive(true);
				poolablePlayer.SetBodyCosmeticsVisible(b: false);
				poolablePlayer.ToggleName(active: false);
				component.Play(anim);
				while (crewmateLeadTime > 0f)
				{
					((TMP_Text)HideAndSeekTimerText).text = Mathf.RoundToInt(crewmateLeadTime).ToString();
					crewmateLeadTime -= Time.deltaTime;
					yield return null;
				}
			}
			else
			{
				ShipStatus.Instance.HideCountdown = logicOptionsHnS.GetCrewmateLeadTime();
				if (Constants.ShouldHorseAround())
				{
					if ((Object)(object)impostor != (Object)null)
					{
						impostor.AnimateCustom(HnSSeekerSpawnHorseInGameAnim);
					}
				}
				else if ((Object)(object)impostor != (Object)null)
				{
					impostor.AnimateCustom(HnSSeekerSpawnAnim);
					impostor.cosmetics.SetBodyCosmeticsVisible(b: false);
				}
			}
		}
		ShipStatus.Instance.StartSFX();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void CheckSeekerHand_AnimEvent()
	{
		if (HideAndSeekPlayerVisual.HasHat())
		{
			((Component)HideAndSeekPlayerVisual.GetHandHat()).gameObject.SetActive(true);
			HideAndSeekPlayerVisual.ToggleHat(b: true);
		}
	}

	private static List<PlayerControl> SelectTeamToShow(Func<GameData.PlayerInfo, bool> roleFilter)
	{
		return (from pcd in GameData.Instance.AllPlayers
			where !pcd.Disconnected && roleFilter(pcd)
			select pcd.Object into pc
			orderby (!((Object)(object)pc == (Object)(object)PlayerControl.LocalPlayer)) ? 1 : 0
			select pc).ToList();
	}

	private IEnumerator ShowTeam(List<PlayerControl> teamToShow, float duration)
	{
		if (overlayHandle == null)
		{
			overlayHandle = DestroyableSingleton<DualshockLightManager>.Instance.AllocateLight();
		}
		yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
		if (!teamToShow.TrueForAll((PlayerControl p) => p.Data.Role.IsImpostor))
		{
			BeginCrewmate(teamToShow);
			overlayHandle.color = Palette.CrewmateBlue;
		}
		else
		{
			BeginImpostor(teamToShow);
			overlayHandle.color = Palette.ImpostorRed;
		}
		Color c = ((Graphic)TeamTitle).color;
		Color fade = Color.black;
		Color impColor = Color.white;
		Vector3 titlePos = TeamTitle.transform.localPosition;
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float num = Mathf.Min(1f, timer / duration);
			((Renderer)Foreground).material.SetFloat("_Rad", ForegroundRadius.ExpOutLerp(num * 2f));
			fade.a = Mathf.Lerp(1f, 0f, num * 3f);
			FrontMost.color = fade;
			c.a = Mathf.Clamp(FloatRange.ExpOutLerp(num, 0f, 1f), 0f, 1f);
			((Graphic)TeamTitle).color = c;
			((Graphic)RoleText).color = c;
			impColor.a = Mathf.Lerp(0f, 1f, (num - 0.3f) * 3f);
			((Graphic)ImpostorText).color = impColor;
			titlePos.y = 2.7f - num * 0.3f;
			TeamTitle.transform.localPosition = titlePos;
			overlayHandle.color.a = Mathf.Min(1f, timer * 2f);
			yield return null;
		}
		timer = 0f;
		while (timer < 1f)
		{
			timer += Time.deltaTime;
			float num2 = timer / 1f;
			fade.a = Mathf.Lerp(0f, 1f, num2 * 3f);
			FrontMost.color = fade;
			overlayHandle.color.a = 1f - fade.a;
			yield return null;
		}
	}

	private void BeginCrewmate(List<PlayerControl> teamToDisplay)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)BackgroundBar).transform.position;
		position.y -= 0.25f;
		((Component)BackgroundBar).transform.position = position;
		((Renderer)BackgroundBar).material.SetColor("_Color", Palette.CrewmateBlue);
		((TMP_Text)TeamTitle).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Crewmate);
		((Graphic)TeamTitle).color = Palette.CrewmateBlue;
		int maxDepth = Mathf.CeilToInt(7.5f);
		for (int i = 0; i < teamToDisplay.Count; i++)
		{
			PlayerControl playerControl = teamToDisplay[i];
			if (!Object.op_Implicit((Object)(object)playerControl))
			{
				continue;
			}
			GameData.PlayerInfo data = playerControl.Data;
			if (data != null)
			{
				PoolablePlayer poolablePlayer = CreatePlayer(i, maxDepth, data, impostorPositioning: false);
				if (i == 0 && data.PlayerId == PlayerControl.LocalPlayer.PlayerId)
				{
					ourCrewmate = poolablePlayer;
				}
			}
		}
	}

	private void BeginImpostor(List<PlayerControl> yourTeam)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)TeamTitle).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Impostor);
		((Graphic)TeamTitle).color = Palette.ImpostorRed;
		for (int i = 0; i < yourTeam.Count; i++)
		{
			PlayerControl playerControl = yourTeam[i];
			if (!Object.op_Implicit((Object)(object)playerControl))
			{
				continue;
			}
			GameData.PlayerInfo data = playerControl.Data;
			if (data != null)
			{
				PoolablePlayer poolablePlayer = CreatePlayer(i, 1, data, impostorPositioning: true);
				if (i == 0 && data.PlayerId == PlayerControl.LocalPlayer.PlayerId)
				{
					ourCrewmate = poolablePlayer;
				}
			}
		}
	}

	private IEnumerator ShowRole()
	{
		RoleBehaviour role = PlayerControl.LocalPlayer.Data.Role;
		((TMP_Text)RoleText).text = DestroyableSingleton<TranslationController>.Instance.GetString(role.StringName);
		((TMP_Text)RoleBlurbText).text = role.Blurb;
		((Graphic)RoleText).color = role.TeamColor;
		((Graphic)YouAreText).color = role.TeamColor;
		((Graphic)RoleBlurbText).color = role.TeamColor;
		SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.Data.Role.IntroSound, loop: false);
		((Component)YouAreText).gameObject.SetActive(true);
		((Component)RoleText).gameObject.SetActive(true);
		((Component)RoleBlurbText).gameObject.SetActive(true);
		if ((Object)(object)ourCrewmate == (Object)null)
		{
			ourCrewmate = CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, impostorPositioning: false);
			((Component)ourCrewmate).gameObject.SetActive(false);
		}
		((Component)ourCrewmate).gameObject.SetActive(true);
		((Component)ourCrewmate).transform.localPosition = new Vector3(0f, -1.05f, -18f);
		((Component)ourCrewmate).transform.localScale = new Vector3(1f, 1f, 1f);
		ourCrewmate.ToggleName(active: false);
		yield return (object)new WaitForSeconds(2.5f);
		((Component)YouAreText).gameObject.SetActive(false);
		((Component)RoleText).gameObject.SetActive(false);
		((Component)RoleBlurbText).gameObject.SetActive(false);
		((Component)ourCrewmate).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (overlayHandle != null)
		{
			overlayHandle.Dispose();
			overlayHandle = null;
		}
	}

	private PoolablePlayer CreatePlayer(int i, int maxDepth, GameData.PlayerInfo pData, bool impostorPositioning)
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		int num = ((i % 2 != 0) ? 1 : (-1));
		int num2 = (i + 1) / 2;
		float num3 = ((i == 0) ? (-8) : (-1));
		PoolablePlayer poolablePlayer = Object.Instantiate<PoolablePlayer>(PlayerPrefab, ((Component)this).transform);
		((Object)poolablePlayer).name = pData.PlayerName + "Dummy";
		poolablePlayer.SetFlipX(i % 2 == 0);
		if (impostorPositioning)
		{
			((Component)poolablePlayer).transform.localPosition = new Vector3((float)(num * num2) * 1.5f, -1f + (float)num2 * 0.15f, num3 + (float)num2 * 0.01f) * 1f;
			float num4 = (1f - (float)num2 * 0.075f) * 1f;
			Vector3 val = new Vector3(num4, num4, 1f);
			((Component)poolablePlayer).transform.localScale = val;
			poolablePlayer.SetNameScale(val.Inv());
		}
		else
		{
			float num5 = (float)num2 / (float)maxDepth;
			float num6 = Mathf.Lerp(1f, 0.75f, num5);
			((Component)poolablePlayer).transform.localPosition = new Vector3(0.9f * (float)num * (float)num2 * num6, FloatRange.SpreadToEdges(-1.125f, 0f, num2, maxDepth), num3 + (float)num2 * 0.01f) * 1f;
			float num7 = Mathf.Lerp((i == 0) ? 1.2f : 1f, 0.65f, num5) * 1f;
			((Component)poolablePlayer).transform.localScale = new Vector3(num7, num7, 1f);
		}
		poolablePlayer.UpdateFromEitherPlayerDataOrCache(pData, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, includePet: true);
		if (impostorPositioning)
		{
			Vector3 namePosition = new Vector3(0f, -1.31f, -0.5f);
			poolablePlayer.SetNamePosition(namePosition);
			poolablePlayer.SetName(pData.PlayerName);
			poolablePlayer.SetNameColor(pData.Role.NameColor);
		}
		poolablePlayer.ToggleName(impostorPositioning);
		return poolablePlayer;
	}

	private void LogPlayerRoleData()
	{
		string text = "IntroCutscene :: CoBegin() :: PlayerControl.LocalPlayer";
		if ((Object)(object)PlayerControl.LocalPlayer == (Object)null)
		{
			Logger.GlobalInstance.Error(text + " is NULL");
			return;
		}
		text += ".Data";
		if (PlayerControl.LocalPlayer.Data == null)
		{
			Logger.GlobalInstance.Error(text + " is NULL");
			return;
		}
		text += ".Role";
		if ((Object)(object)PlayerControl.LocalPlayer.Data.Role == (Object)null)
		{
			Logger.GlobalInstance.Error(text + " is NULL");
		}
		else
		{
			Logger.GlobalInstance.Info(text + " data is valid");
		}
	}

	public IntroCutscene()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
