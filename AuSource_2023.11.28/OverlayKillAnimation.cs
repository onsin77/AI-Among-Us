using System.Collections;
using System.Collections.Generic;
using PowerTools;
using UnityEngine;

public class OverlayKillAnimation : OverlayAnimation
{
	[SerializeField]
	private KillAnimType KillType;

	[SerializeField]
	protected PoolablePlayer killerParts;

	[SerializeField]
	private Vector3 KillerPetPosition = new Vector3(-2.068f, -0.37f, 0.2f);

	[SerializeField]
	protected PoolablePlayer victimParts;

	[SerializeField]
	private Vector3 VictimPetPosition = new Vector3(0.282f, -0.37f, 0.1f);

	[SerializeField]
	private AudioClip Stinger;

	[SerializeField]
	private AudioClip Sfx;

	[SerializeField]
	private float StingerVolume = 0.6f;

	private string victimHat;

	private HashSet<GameObject> petObjects;

	public virtual void Initialize(GameData.PlayerInfo kInfo, GameData.PlayerInfo vInfo)
	{
		if (Object.op_Implicit((Object)(object)killerParts))
		{
			PlayerControl playerControl = PlayerControl.AllPlayerControls.Find((PlayerControl p) => p.PlayerId == kInfo.PlayerId);
			GameData.PlayerOutfit killerOutfit = playerControl.CurrentOutfit;
			killerParts.SetBodyType(playerControl.BodyType);
			killerParts.UpdateFromPlayerData(kInfo, playerControl.CurrentOutfitType, PlayerMaterial.MaskType.None, includePet: false, () =>
			{
				LoadKillerSkin(killerOutfit);
			});
			killerParts.ToggleName(active: false);
			LoadKillerPet(killerOutfit);
		}
		if (vInfo != null && Object.op_Implicit((Object)(object)victimParts))
		{
			PlayerControl playerControl2 = PlayerControl.AllPlayerControls.Find((PlayerControl p) => p.PlayerId == vInfo.PlayerId);
			GameData.PlayerOutfit victimOutfit = playerControl2.CurrentOutfit;
			victimHat = victimOutfit.HatId;
			victimParts.SetBodyType(playerControl2.BodyType);
			victimParts.UpdateFromPlayerData(vInfo, playerControl2.CurrentOutfitType, PlayerMaterial.MaskType.None, includePet: false, () =>
			{
				LoadVictimSkin(victimOutfit);
			});
			victimParts.ToggleName(active: false);
			LoadVictimPet(victimOutfit);
		}
	}

	public void SetHatFloor()
	{
		HatData hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(victimHat);
		if (Object.op_Implicit((Object)(object)hatById))
		{
			victimParts.SetHatWithoutChangingColor(hatById);
			victimParts.SetHatAndVisorOnFloor();
		}
	}

	public void PlayKillSound()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(Sfx, loop: false).volume = 0.8f;
			VibrationManager.Vibrate(3f, 3f, 0f, VibrationManager.VibrationFalloff.None, Sfx);
			DualshockLightManager.Flash(Color.red, 2f, Sfx);
		}
	}

	public override IEnumerator CoShow(KillOverlay parent)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(Stinger, loop: false).volume = StingerVolume;
		}
		petObjects = new HashSet<GameObject>();
		SpriteAnim[] componentsInChildren = ((Component)this).GetComponentsInChildren<SpriteAnim>();
		foreach (SpriteAnim spriteAnim in componentsInChildren)
		{
			if ((Object)(object)((Component)spriteAnim).GetComponent<PetBehaviour>() != (Object)null)
			{
				petObjects.Add(((Component)spriteAnim).gameObject);
			}
		}
		((Renderer)parent.background).enabled = true;
		yield return Effects.Wait(1f / 12f);
		((Renderer)parent.background).enabled = false;
		parent.flameParent.SetActive(true);
		parent.flameParent.transform.localScale = new Vector3(1f, 0.3f, 1f);
		parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 25f);
		yield return Effects.Wait(1f / 12f);
		parent.flameParent.transform.localScale = new Vector3(1f, 0.5f, 1f);
		parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, -15f);
		yield return Effects.Wait(1f / 12f);
		parent.flameParent.transform.localScale = new Vector3(1f, 1f, 1f);
		parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		((Component)this).gameObject.SetActive(true);
		yield return WaitForFinish();
		((Component)this).gameObject.SetActive(false);
		yield return new WaitForLerp(1f / 6f, (float t) =>
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			parent.flameParent.transform.localScale = new Vector3(1f, 1f - t, 1f);
		});
		parent.flameParent.SetActive(false);
	}

	private IEnumerator WaitForFinish()
	{
		SpriteAnim[] anims = ((Component)this).GetComponentsInChildren<SpriteAnim>();
		if (anims.Length == 0)
		{
			yield return (object)new WaitForSeconds(1f);
			yield break;
		}
		while (true)
		{
			bool flag = false;
			for (int i = 0; i < anims.Length; i++)
			{
				if (!petObjects.Contains(((Component)anims[i]).gameObject) && anims[i].IsPlaying())
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private void LoadVictimSkin(GameData.PlayerOutfit victimOutfit)
	{
		SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(victimOutfit.SkinId);
		SpriteAnim skinSpriteAnim = victimParts.GetSkinSpriteAnim();
		switch (KillType)
		{
		case KillAnimType.RHM:
			skinSpriteAnim.Play(skin.KillRHMVictim);
			break;
		case KillAnimType.Tongue:
			skinSpriteAnim.Play(skin.KillTongueVictim);
			break;
		case KillAnimType.Stab:
			skinSpriteAnim.Play(skin.KillStabVictim);
			break;
		case KillAnimType.Neck:
			skinSpriteAnim.Play(skin.KillNeckVictim);
			break;
		case KillAnimType.Shoot:
			skinSpriteAnim.Play(skin.KillShootVictim);
			break;
		}
	}

	private void LoadKillerSkin(GameData.PlayerOutfit killerOutfit)
	{
		SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(killerOutfit.SkinId);
		switch (KillType)
		{
		case KillAnimType.Tongue:
			killerParts.GetSkinSpriteAnim().Play(skin.KillTongueImpostor);
			break;
		case KillAnimType.Shoot:
			killerParts.GetSkinSpriteAnim().Play(skin.KillShootImpostor);
			break;
		case KillAnimType.Neck:
			killerParts.GetSkinSpriteAnim().Play(skin.KillNeckImpostor);
			break;
		case KillAnimType.Stab:
			killerParts.GetSkinSpriteAnim().Play(skin.KillStabImpostor);
			break;
		}
	}

	private void LoadKillerPet(GameData.PlayerOutfit killerOutfit)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)DestroyableSingleton<HatManager>.Instance.GetPetById(killerOutfit.PetId)))
		{
			PetBehaviour pet = ShipStatus.Instance.CosmeticsCache.GetPet(killerOutfit.PetId);
			killerParts.SetPetIdle(pet, killerOutfit.ColorId);
			killerParts.ReplacePetParent(((Component)this).transform, KillerPetPosition);
		}
	}

	private void LoadVictimPet(GameData.PlayerOutfit victimOutfit)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)DestroyableSingleton<HatManager>.Instance.GetPetById(victimOutfit.PetId)))
		{
			PetBehaviour pet = ShipStatus.Instance.CosmeticsCache.GetPet(victimOutfit.PetId);
			victimParts.SetPetScared(pet, victimOutfit.ColorId);
			victimParts.SetPetFlipX(flipped: true);
			victimParts.ReplacePetParent(((Component)this).transform, VictimPetPosition);
		}
	}

	private void OnDestroy()
	{
		petObjects = null;
	}

	public OverlayKillAnimation()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
	}
}
