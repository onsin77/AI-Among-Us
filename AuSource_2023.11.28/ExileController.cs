using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public abstract class ExileController : MonoBehaviour
{
	public static ExileController Instance;

	public TextMeshPro ImpostorText;

	public TextMeshPro Text;

	public PoolablePlayer Player;

	public AnimationCurve LerpCurve;

	public float Duration = 7f;

	public AudioClip TextSound;

	public AudioClip EjectSound;

	[SerializeField]
	private bool useIdleAnim;

	[SerializeField]
	private Vector3 exileHatPosition = new Vector3(-0.216f, 0.528f);

	[SerializeField]
	private Vector3 exileVisorPosition = new Vector3(-0.148f, 0.647f, -0.002f);

	protected string completeString = "TestPlayer was not The Impostor";

	protected GameData.PlayerInfo exiled;

	private SpecialInputHandler specialInputHandler;

	private void Awake()
	{
		specialInputHandler = ((Component)this).GetComponent<SpecialInputHandler>();
	}

	protected abstract IEnumerator Animate();

	protected virtual IEnumerator HandleText(float firstWaitTime, float animDuration)
	{
		yield return Effects.Wait(firstWaitTime);
		for (float t = 0f; t <= animDuration; t += Time.deltaTime)
		{
			int num = (int)(t / animDuration * (float)completeString.Length);
			if (num > ((TMP_Text)Text).text.Length)
			{
				((TMP_Text)Text).text = completeString.Substring(0, num);
				((Component)Text).gameObject.SetActive(true);
				if (completeString[num - 1] != ' ')
				{
					SoundManager.Instance.PlaySoundImmediate(TextSound, loop: false, 0.8f);
				}
			}
			yield return null;
		}
		((TMP_Text)Text).text = completeString;
	}

	public void Begin(GameData.PlayerInfo exiled, bool tie)
	{
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)specialInputHandler != (Object)null)
		{
			specialInputHandler.disableVirtualCursor = true;
		}
		Instance = this;
		ControllerManager.Instance.CloseAndResetAll();
		this.exiled = exiled;
		((Component)Text).gameObject.SetActive(false);
		((TMP_Text)Text).text = string.Empty;
		int num = GameData.Instance.AllPlayers.Count((GameData.PlayerInfo p) => p.Role.IsImpostor && !p.IsDead && !p.Disconnected);
		if (exiled != null)
		{
			int num2 = GameData.Instance.AllPlayers.Count((GameData.PlayerInfo p) => p.Role.IsImpostor);
			if (!GameManager.Instance.LogicOptions.GetConfirmImpostor())
			{
				completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextNonConfirm, exiled.PlayerName);
			}
			else if (exiled.Role.IsImpostor)
			{
				if (num2 > 1)
				{
					completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP, exiled.PlayerName);
				}
				else
				{
					completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, exiled.PlayerName);
				}
			}
			else if (num2 > 1)
			{
				completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPN, exiled.PlayerName);
			}
			else
			{
				completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSN, exiled.PlayerName);
			}
			Player.UpdateFromEitherPlayerDataOrCache(exiled, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, includePet: false, () =>
			{
				SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(exiled.Outfits[PlayerOutfitType.Default].SkinId);
				if (useIdleAnim)
				{
					Player.FixSkinSprite(skin.IdleFrame);
				}
				else
				{
					Player.FixSkinSprite(skin.EjectFrame);
				}
			});
			Player.ToggleName(active: false);
			if (!useIdleAnim)
			{
				Player.SetCustomHatPosition(exileHatPosition);
				Player.SetCustomVisorPosition(exileVisorPosition);
			}
			if (exiled.Role.IsImpostor)
			{
				num--;
			}
		}
		else
		{
			if (tie)
			{
				completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileTie);
			}
			else
			{
				completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileSkip);
			}
			((Component)Player).gameObject.SetActive(false);
		}
		if (num == 1)
		{
			((TMP_Text)ImpostorText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainS, num);
		}
		else
		{
			((TMP_Text)ImpostorText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainP, num);
		}
		((MonoBehaviour)this).StartCoroutine(Animate());
	}

	protected void WrapUp()
	{
		if (exiled != null)
		{
			PlayerControl playerControl = exiled.Object;
			if (Object.op_Implicit((Object)(object)playerControl))
			{
				playerControl.Exiled();
			}
			exiled.IsDead = true;
		}
		if (DestroyableSingleton<TutorialManager>.InstanceExists || !GameManager.Instance.LogicFlow.IsGameOverDueToDeath())
		{
			ReEnableGameplay();
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	protected void ReEnableGameplay()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear));
		PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
		ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
		((Component)Camera.main).GetComponent<FollowerCamera>().Locked = false;
		DestroyableSingleton<HudManager>.Instance.SetHudActive(isActive: true);
		ControllerManager.Instance.CloseAndResetAll();
	}

	protected ExileController()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
	}
}
