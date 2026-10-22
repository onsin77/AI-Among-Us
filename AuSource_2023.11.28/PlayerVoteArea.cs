using System;
using System.Collections;
using System.Collections.Generic;
using AmongUs.Data;
using Hazel;
using Innersloth.Assets;
using TMPro;
using UnityEngine;

public class PlayerVoteArea : MonoBehaviour
{
	public byte TargetPlayerId;

	public const byte HasNotVoted = byte.MaxValue;

	public const byte MissedVote = 254;

	public const byte SkippedVote = 253;

	public const byte DeadVote = 252;

	public GameObject Buttons;

	public UiElement ConfirmButton;

	public UiElement CancelButton;

	public UiElement PlayerButton;

	public SpriteRenderer Background;

	public SpriteRenderer MaskArea;

	public SpriteRenderer Flag;

	public SpriteRenderer Megaphone;

	public SpriteRenderer Overlay;

	public SpriteRenderer XMark;

	public SpriteRenderer GAIcon;

	public SpriteRenderer ThumbsDown;

	public SpriteRenderer HighlightedFX;

	public TextMeshPro NameText;

	public TextMeshPro LevelNumberText;

	public TextMeshPro ColorBlindName;

	public bool AnimateButtonsFromLeft;

	public bool AmDead;

	public bool DidReport;

	public byte VotedFor = byte.MaxValue;

	public bool voteComplete;

	public bool resultsShowing;

	[SerializeField]
	private PoolablePlayer PlayerIcon;

	public MeetingHud Parent { get; set; }

	public bool DidVote => VotedFor != byte.MaxValue;

	public int MaskLayer { get; private set; }

	private void Start()
	{
		Buttons.SetActive(false);
	}

	public void SetTargetPlayerId(byte targetId)
	{
		TargetPlayerId = targetId;
		if (Object.op_Implicit((Object)(object)PlayerIcon))
		{
			SetMaskLayer(targetId);
		}
	}

	public void SetMaskLayer(int maskLayer)
	{
		MaskLayer = maskLayer + 2;
		SpriteRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).material.SetInt(PlayerMaterial.MaskLayer, MaskLayer);
		}
		PlayerIcon.SetMaskLayer(MaskLayer);
	}

	public void SetDead(bool didReport, bool isDead, bool isGuardian = false)
	{
		AmDead = isDead;
		DidReport = didReport;
		((Renderer)Megaphone).enabled = didReport;
		((Component)Overlay).gameObject.SetActive(isDead);
		((Component)XMark).gameObject.SetActive(isDead);
		((Component)GAIcon).gameObject.SetActive(isGuardian);
	}

	public void SetDisabled()
	{
		if (!AmDead)
		{
			if (Object.op_Implicit((Object)(object)Overlay))
			{
				((Component)Overlay).gameObject.SetActive(true);
				((Component)XMark).gameObject.SetActive(false);
			}
			else
			{
				((Component)this).gameObject.SetActive(false);
			}
		}
	}

	public void SetEnabled()
	{
		if (!AmDead)
		{
			if (Object.op_Implicit((Object)(object)Overlay))
			{
				((Component)Overlay).gameObject.SetActive(false);
			}
			else
			{
				((Component)this).gameObject.SetActive(true);
			}
		}
	}

	public void UpdateOverlay()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		((Component)Overlay).gameObject.SetActive(AmDead);
		if (AmDead)
		{
			Overlay.color = Color.white;
			((Component)XMark).transform.localScale = Vector3.one;
		}
		else if (DidReport)
		{
			((Component)Megaphone).transform.localEulerAngles = Vector3.zero;
			((Component)Megaphone).transform.localScale = Vector3.one;
		}
	}

	public IEnumerator CoAnimateOverlay()
	{
		if (DidReport)
		{
			float duration = 1f;
			for (float time = 0f; time < duration; time += Time.deltaTime)
			{
				float num = time / duration;
				float num2 = TriangleWave(num * 3f) * 2f - 1f;
				((Component)Megaphone).transform.localEulerAngles = new Vector3(0f, 0f, num2 * 30f);
				num2 = Mathf.Lerp(0.7f, 1.2f, TriangleWave(num * 2f));
				((Component)Megaphone).transform.localScale = new Vector3(num2, num2, num2);
				yield return null;
			}
			((Component)Megaphone).transform.localEulerAngles = Vector3.zero;
			((Component)Megaphone).transform.localScale = Vector3.one;
		}
	}

	private static float TriangleWave(float t)
	{
		t -= (float)(int)t;
		if (t < 0.5f)
		{
			return t * 2f;
		}
		return 1f - (t - 0.5f) * 2f;
	}

	internal void SetVote(byte suspectIdx)
	{
		VotedFor = suspectIdx;
		((Renderer)Flag).enabled = true;
	}

	public void UnsetVote()
	{
		((Renderer)Flag).enabled = false;
		VotedFor = byte.MaxValue;
	}

	public void ClearButtons()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		Buttons.SetActive(false);
	}

	public void ClearForResults()
	{
		resultsShowing = true;
		((Renderer)Flag).enabled = false;
	}

	public void VoteForMe()
	{
		if (!voteComplete)
		{
			Parent.Confirm(TargetPlayerId);
			ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		}
		if (Object.op_Implicit((Object)(object)ThumbsDown))
		{
			((Renderer)ThumbsDown).enabled = true;
		}
	}

	private bool canBeHighlighted()
	{
		if (AmDead)
		{
			return false;
		}
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			return false;
		}
		if (voteComplete)
		{
			return false;
		}
		if ((Object)(object)Parent == (Object)null)
		{
			return false;
		}
		if (Parent.CurrentState == MeetingHud.VoteStates.Discussion)
		{
			return false;
		}
		return true;
	}

	public void SetHighlighted(bool value)
	{
		if (Object.op_Implicit((Object)(object)HighlightedFX))
		{
			((Renderer)HighlightedFX).enabled = value && canBeHighlighted();
		}
	}

	public void Select()
	{
		if (!PlayerControl.LocalPlayer.Data.IsDead && !AmDead && Object.op_Implicit((Object)(object)Parent) && !voteComplete && Parent.Select(TargetPlayerId))
		{
			Buttons.SetActive(true);
			float startPos = (AnimateButtonsFromLeft ? 0.2f : 1.95f);
			((MonoBehaviour)this).StartCoroutine(Effects.All(Effects.Lerp(0.25f, (float t) =>
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				((Component)CancelButton).transform.localPosition = Vector2.op_Implicit(Vector2.Lerp(Vector2.right * startPos, Vector2.right * 1.3f, Effects.ExpOut(t)));
			}), Effects.Lerp(0.35f, (float t) =>
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				((Component)ConfirmButton).transform.localPosition = Vector2.op_Implicit(Vector2.Lerp(Vector2.right * startPos, Vector2.right * 0.65f, Effects.ExpOut(t)));
			})));
			List<UiElement> selectableElements = new List<UiElement> { CancelButton, ConfirmButton };
			ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, CancelButton, ConfirmButton, selectableElements);
		}
	}

	public void Cancel()
	{
		ClearButtons();
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void Serialize(MessageWriter writer)
	{
		writer.Write(VotedFor);
		writer.Write(DidReport);
	}

	public void Deserialize(MessageReader reader)
	{
		VotedFor = reader.ReadByte();
		DidReport = reader.ReadBoolean();
		((Renderer)Flag).enabled = DidVote && !resultsShowing;
		((Component)Overlay).gameObject.SetActive(AmDead);
		((Renderer)Megaphone).enabled = DidReport;
	}

	public void SetCosmetics(GameData.PlayerInfo playerInfo)
	{
		Background.sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate(playerInfo.DefaultOutfit.NamePlateId).Image;
		PlayerIcon.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, includePet: false);
		PlayerIcon.ToggleName(active: false);
		((TMP_Text)NameText).text = playerInfo.PlayerName;
		((TMP_Text)LevelNumberText).text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);
		PlayerMaterial.SetColors((int)DataManager.Player.Customization.Color, (Renderer)(object)ThumbsDown);
		DataManager.Settings.Accessibility.OnColorBlindModeChanged += SetColorblindText;
		SetColorblindText();
	}

	public void PreviewNameplate(string plateID)
	{
		((Component)this).gameObject.SetActive(true);
		NamePlateData namePlateById = DestroyableSingleton<HatManager>.Instance.GetNamePlateById(plateID);
		((MonoBehaviour)this).StartCoroutine(AddressableAssetExtensions.CoLoadAssetAsync<NamePlateViewData>((MonoBehaviour)(object)this, (IAddressableAssetProvider<NamePlateViewData>)namePlateById, (Action<NamePlateViewData>)((NamePlateViewData viewData) =>
		{
			Background.sprite = viewData.Image;
		})));
		((Component)PlayerIcon).gameObject.SetActive(false);
		((TMP_Text)NameText).text = DataManager.Player.Customization.Name;
		((TMP_Text)LevelNumberText).text = ProgressionManager.Instance.CurrentVisualLevel;
	}

	private void OnDestroy()
	{
		DataManager.Settings.Accessibility.OnColorBlindModeChanged -= SetColorblindText;
		if (Buttons.activeSelf)
		{
			ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		}
	}

	private void SetColorblindText()
	{
		((Behaviour)ColorBlindName).enabled = DataManager.Settings.Accessibility.ColorBlindMode;
		((TMP_Text)ColorBlindName).text = PlayerIcon.ColorBlindName;
	}
}
