using System;
using System.Collections;
using System.Collections.Generic;
using AmongUs.Data;
using Innersloth.Assets;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticsLayer : MonoBehaviour
{
	[SerializeField]
	[Header("Options")]
	private bool alwaysDrawNormalPlayer;

	[SerializeField]
	private bool uiPet = true;

	[SerializeField]
	private float zIndexSpacing = 1E-08f;

	[SerializeField]
	private bool showColorBlindText;

	[SerializeField]
	[Header("Cosmetics")]
	private List<PlayerBodySprite> bodySprites;

	[SerializeField]
	private TextMeshPro colorBlindText;

	[SerializeField]
	private HatParent hat;

	[SerializeField]
	private TextMeshPro nameText;

	[SerializeField]
	private GameObject nameTextContainer;

	[SerializeField]
	private Transform petParent;

	[SerializeField]
	private SkinLayer skin;

	[SerializeField]
	private VisorLayer visor;

	[SerializeField]
	private Vector3 defaultHatVisorPosition;

	private PlayerBodyTypes bodyType;

	private PlayerMaterial.Properties bodyMatProperties;

	private PlayerBodySprite currentBodySprite;

	private PetBehaviour currentPet;

	private PlayerBodySprite normalBodySprite;

	private bool initialized;

	private AddressableAsset<PetBehaviour> petAsset;

	private bool visible = true;

	private bool isNameVisible = true;

	private bool lockVisible;

	private bool localPlayer;

	public PetBehaviour CurrentPet => currentPet;

	public PlayerPettingHand PettingHand => currentBodySprite.PettingHand;

	public SpriteRenderer HatHand => currentBodySprite.HandHat;

	public PlayerBodyTypes CurrentBodyType => currentBodySprite.Type;

	public Vector3 FlippedCosmeticOffset
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return currentBodySprite.flippedCosmeticOffset;
		}
	}

	public bool AlwaysDrawNormalPlayer => alwaysDrawNormalPlayer;

	public bool FlipX => currentBodySprite.BodySprite.flipX;

	public int ColorId => bodyMatProperties.ColorId;

	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			if (!lockVisible)
			{
				visible = value;
			}
			UpdateVisibility();
		}
	}

	public void SetBodyCosmeticsVisible(bool b)
	{
		skin.Visible = b;
		hat.Visible = b;
		visor.Visible = b;
		((Behaviour)colorBlindText).enabled = b;
	}

	public void UpdateVisibility()
	{
		currentBodySprite.Visible = visible;
		skin.Visible = visible;
		hat.Visible = visible;
		visor.Visible = visible;
		((Behaviour)colorBlindText).enabled = visible;
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			currentPet.Visible = visible;
		}
		UpdateNameVisibility();
	}

	public void SetPetVisible(bool v)
	{
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			currentPet.Visible = v;
		}
	}

	public bool HasPetEquipped()
	{
		if (Object.op_Implicit((Object)(object)CurrentPet))
		{
			return CurrentPet.Data.ProductId != "pet_EmptyPet";
		}
		return false;
	}

	public void ToggleHat(bool b)
	{
		hat.Visible = b;
	}

	public void SetForcedVisible(bool isVisible)
	{
		if (isVisible)
		{
			lockVisible = false;
			Visible = isVisible;
		}
		else
		{
			Visible = isVisible;
			lockVisible = true;
		}
	}

	public static Material GetBodyMaterial(PlayerMaterial.MaskType type)
	{
		Material result = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
		if (type == PlayerMaterial.MaskType.ComplexUI || type == PlayerMaterial.MaskType.ScrollingUI)
		{
			result = DestroyableSingleton<HatManager>.Instance.MaskedPlayerMaterial;
		}
		return result;
	}

	public void EnsureInitialized(PlayerBodyTypes bt)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (initialized && bt == bodyType)
		{
			return;
		}
		if (alwaysDrawNormalPlayer && bt != PlayerBodyTypes.Normal)
		{
			Logger.GlobalInstance.Info($"Overriding body type {bt} with Normal because {((Object)this).name} always draws normal player.");
			bt = PlayerBodyTypes.Normal;
		}
		bodyType = bt;
		bodySprites.ForEach((PlayerBodySprite s) =>
		{
			if (s.Type == PlayerBodyTypes.Normal)
			{
				normalBodySprite = s;
			}
			if (s.Type == bodyType)
			{
				currentBodySprite = s;
				s.Visible = true;
			}
			else
			{
				s.Visible = false;
			}
		});
		if (Object.op_Implicit((Object)(object)hat))
		{
			((Component)hat).transform.localPosition = defaultHatVisorPosition;
			hat.Parent = currentBodySprite.BodySprite;
			hat.SetOptions(bodyType.GetHatOptions());
		}
		if ((Object)(object)skin != (Object)null && (Object)(object)skin.layer != (Object)null)
		{
			skin.layer.color = (bodyType.GetSkinOptions().Enabled ? Color.white : Color.clear);
		}
		if (Object.op_Implicit((Object)(object)visor))
		{
			((Component)visor).transform.localPosition = defaultHatVisorPosition;
			visor.SetOptions(bodyType.GetVisorOptions());
		}
		SetCosmeticZIndices();
		if (initialized)
		{
			SetColor(bodyMatProperties.ColorId);
		}
		initialized = true;
	}

	public void SetCustomHatPosition(Vector3 pos)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)hat))
		{
			((Component)hat).transform.localPosition = pos;
		}
	}

	public void SetCustomVisorPosition(Vector3 pos)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)visor))
		{
			((Component)visor).transform.localPosition = pos;
		}
	}

	public void FixSkinSprite(Sprite skinSprite)
	{
		skin.animator.Stop();
		skin.layer.sprite = skinSprite;
	}

	public PetBehaviour GetPet()
	{
		return currentPet;
	}

	public SpriteAnim GetSkinSpriteAnim()
	{
		return ((Component)skin).GetComponent<SpriteAnim>();
	}

	public void ReplacePetParent(Transform newParent, Vector3 newPosition)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)petParent == (Object)null)
		{
			Debug.LogError((object)"Trying to replace pet parent when none is set.");
			return;
		}
		petParent.SetParent(newParent);
		((Component)petParent).transform.localPosition = newPosition;
	}

	public void ResetCosmetics()
	{
		bool isLeft = false;
		if (currentBodySprite != null && (Object)(object)currentBodySprite.BodySprite != (Object)null)
		{
			isLeft = currentBodySprite.BodySprite.flipX;
		}
		skin?.SetSkin("", 0, isLeft);
		hat?.SetHat("", 0);
		visor?.SetVisor("", 0);
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			Object.Destroy((Object)(object)((Component)currentPet).gameObject);
			currentPet = null;
		}
	}

	public void SetBodyAsGhost()
	{
		currentBodySprite.BodySprite.sprite = currentBodySprite.GhostSprite;
	}

	public void SetBodyColor(int bodyColor)
	{
		bodyMatProperties.ColorId = bodyColor;
		UpdateBodyMaterial();
		SetColorBlindColor(bodyColor);
	}

	public void SetColor(int color)
	{
		SetBodyColor(color);
		SetHatColor(color);
		SetSkinColor(color);
		SetPetColor(color);
		SetVisorColor(color);
	}

	public void SetDeadFlipX(bool flipped)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		currentBodySprite.BodySprite.flipX = flipped;
		hat.FlipX = flipped;
		Vector3 localPosition = defaultHatVisorPosition;
		Vector3 localPosition2 = ((Component)visor).transform.localPosition;
		if (currentBodySprite.Type != PlayerBodyTypes.Horse)
		{
			localPosition.x = (flipped ? (-0.2f) : 0.2f);
			localPosition.y = 0.75f;
			localPosition2.x = (flipped ? (-0.17f) : 0.17f);
			localPosition2.y = 0.75f;
		}
		((Component)hat).transform.localPosition = localPosition;
		if (flipped)
		{
			Transform transform = ((Component)hat).transform;
			transform.localPosition += currentBodySprite.flippedCosmeticOffset;
			Transform transform2 = ((Component)visor).transform;
			transform2.localPosition += currentBodySprite.flippedCosmeticOffset;
		}
		visor.SetFlipX(flipped);
		((Component)visor).transform.localPosition = localPosition2;
		SetPetFlipX(flipped);
	}

	public void SetEnabledColorblind(bool isEnabled)
	{
		if (showColorBlindText)
		{
			((Component)colorBlindText).gameObject.SetActive(isEnabled);
		}
	}

	public void SetFlipX(bool flipped)
	{
		SetFlipXWithoutPet(flipped);
		SetPetFlipX(flipped);
	}

	public void SetFlipXWithoutPet(bool flipped)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		currentBodySprite.BodySprite.flipX = flipped;
		hat.FlipX = flipped;
		Vector3 val = defaultHatVisorPosition;
		val.x = (flipped ? Mathf.Abs(val.x) : (0f - Mathf.Abs(val.x)));
		((Component)hat).transform.localPosition = val;
		if (flipped)
		{
			Transform transform = ((Component)hat).transform;
			transform.localPosition += currentBodySprite.flippedCosmeticOffset;
			Transform transform2 = ((Component)visor).transform;
			transform2.localPosition += currentBodySprite.flippedCosmeticOffset;
		}
		visor.SetFlipX(flipped);
		skin.Flipped = flipped;
	}

	public void SetGhost()
	{
		skin.SetGhost();
	}

	public void SetHat(string hatId, int color)
	{
		hat?.SetHat(hatId, color);
	}

	public void SetHat(HatData hatData, int color)
	{
		hat?.SetHat(hatData, color);
	}

	public bool HasHat()
	{
		if (hat.HasHat())
		{
			return !hat.HideHat();
		}
		return false;
	}

	public void SetHatAndVisorIdle(int colorId)
	{
		hat.SetIdleAnim(colorId);
		ToggleVisor(active: true);
	}

	public void SetHatAndVisorOnFloor()
	{
		hat.SetFloorAnim();
		hat.SetFloorAnim();
	}

	public void SetHatColor(Color color)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)hat))
		{
			hat.SpriteColor = color;
		}
	}

	public void SetHatColor(int color)
	{
		hat?.SetMaterialColor(color);
	}

	public void SetVisorColor(int color)
	{
		visor?.SetMaterialColor(color);
	}

	public void SetHatWithoutChangingColor(HatData hatData)
	{
		if (Object.op_Implicit((Object)(object)hat))
		{
			hat.Hat = hatData;
		}
	}

	public void SetMaskLayer(int layer)
	{
		bodyMatProperties.MaskLayer = layer;
		UpdateBodyMaterial();
		skin.SetMaskLayer(layer);
		hat.SetMaskLayer(layer);
		visor.SetMaskLayer(layer);
	}

	public void SetMaskType(PlayerMaterial.MaskType type)
	{
		bodyMatProperties.MaskType = type;
		UpdateBodyMaterial();
		hat?.SetMaskType(type);
		visor?.SetMaskType(type);
		skin?.SetMaskType(type);
	}

	public void SetName(string nameStr, Vector3 scale, Color color, float zPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		SetNameColor(color);
		SetNameScale(scale);
		nameText.transform.SetLocalZ(zPosition);
		SetName(nameStr);
	}

	public void SetName(string nameStr)
	{
		if (string.IsNullOrWhiteSpace(nameStr))
		{
			((TMP_Text)nameText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerName);
		}
		else
		{
			((TMP_Text)nameText).text = nameStr;
		}
	}

	public void SetNameColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)nameText).color = color;
	}

	public void SetNameMask(bool alive)
	{
		int num = (alive ? 4 : 0);
		((Renderer)((Component)nameText).GetComponent<MeshRenderer>()).material.SetInt("_Mask", num);
	}

	public void SetNamePosition(Vector3 newPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		nameTextContainer.transform.localPosition = newPosition;
	}

	public void SetNameScale(Vector3 scale)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		nameTextContainer.transform.localScale = scale;
	}

	public void SetOutline(bool active, Color? color = null)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)currentBodySprite.BodySprite).material.SetFloat("_Outline", (float)(active ? 1 : 0));
		if (color.HasValue)
		{
			((Renderer)currentBodySprite.BodySprite).material.SetColor("_OutlineColor", color.Value);
		}
	}

	public void SetPetFlipX(bool flipped)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			currentPet.FlipX = flipped;
			if (Object.op_Implicit((Object)(object)petParent))
			{
				Vector3 localPosition = ((Component)petParent).transform.localPosition;
				localPosition.x = (flipped ? (0f - Mathf.Abs(localPosition.x)) : Mathf.Abs(localPosition.x));
				((Component)petParent).transform.localPosition = localPosition;
			}
			else
			{
				Vector3 localPosition2 = ((Component)currentPet).transform.localPosition;
				localPosition2.x = (flipped ? (0f - Mathf.Abs(localPosition2.x)) : Mathf.Abs(localPosition2.x));
				((Component)currentPet).transform.localPosition = localPosition2;
			}
		}
	}

	public void SetPetIdle(string petId, int colorId, Action onComplete = null)
	{
		SetPetIdle(DestroyableSingleton<HatManager>.Instance.GetPetById(petId), colorId, onComplete);
	}

	public void SetPetIdle(PetData petData, int colorId, Action onComplete = null)
	{
		((MonoBehaviour)this).StopAllCoroutines();
		if (Object.op_Implicit((Object)(object)currentPet) && currentPet.Data.ProdId == petData.ProdId)
		{
			currentPet.SetIdle();
			onComplete?.Invoke();
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(CoLoadAndSetPetIdle(petData, colorId, onComplete));
		}
	}

	public void SetPetIdle(PetBehaviour petBehaviour, int colorId, Action onComplete = null)
	{
		((MonoBehaviour)this).StopAllCoroutines();
		InstantiatePetCopy(petBehaviour, colorId);
		currentPet.SetIdle();
		currentPet.Visible = visible;
		onComplete?.Invoke();
	}

	public void SetPetPosition(Vector3 newPosition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			((Component)currentPet).transform.position = newPosition;
		}
	}

	public IEnumerator WalkPetBehindPlayer(float speed)
	{
		yield return currentPet.MoveBehindSource(speed);
	}

	public void SetPetScared(PetBehaviour petBehaviour, int colorId)
	{
		((MonoBehaviour)this).StopAllCoroutines();
		InstantiatePetCopy(petBehaviour, colorId);
		currentPet.SetScared();
	}

	public void SetPetSource(PlayerControl playerControl)
	{
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			currentPet.SetTargetPlayer(playerControl);
		}
	}

	public void SetScale(Vector3 playerScale, Vector3 cosmeticsScale)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((Component)currentBodySprite.BodySprite).transform.localScale = playerScale;
		((Component)normalBodySprite.BodySprite).transform.localScale = playerScale;
		((Component)this).transform.localScale = cosmeticsScale;
	}

	public void SetSkin(string skinId, int color, Action onLoaded = null)
	{
		SetSkin(DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId), color, onLoaded);
	}

	public void SetSkin(SkinData skin, int color, Action onLoaded = null)
	{
		if (Object.op_Implicit((Object)(object)this.skin))
		{
			this.skin.SetSkin(skin, color, currentBodySprite.BodySprite.flipX, onLoaded);
			this.skin.Flipped = currentBodySprite.BodySprite.flipX;
		}
	}

	public void SetSkin(SkinViewData skin, int color)
	{
		if (Object.op_Implicit((Object)(object)this.skin))
		{
			this.skin.SetSkin(skin, color, currentBodySprite.BodySprite.flipX);
			this.skin.Flipped = currentBodySprite.BodySprite.flipX;
		}
	}

	public void SetSkinColor(int color)
	{
		skin?.SetColor(color);
	}

	public void SetVisor(string visorId, int color)
	{
		if (DestroyableSingleton<HatManager>.InstanceExists)
		{
			SetVisor(DestroyableSingleton<HatManager>.Instance.GetVisorById(visorId), color);
		}
	}

	public void SetVisor(VisorData visorData, int color)
	{
		visor?.SetVisor(visorData, color);
	}

	public void SetVisor(VisorData visorData, VisorViewData visorViewData, int color)
	{
		if (DestroyableSingleton<HatManager>.InstanceExists && Object.op_Implicit((Object)(object)visor))
		{
			visor.SetVisor(visorData, visorViewData, color);
		}
	}

	public void SetVisorAlpha(float alpha)
	{
		if (Object.op_Implicit((Object)(object)visor))
		{
			visor.Alpha = alpha;
		}
	}

	public void ToggleName(bool active)
	{
		((Component)nameText).gameObject.SetActive(active);
	}

	public void TogglePet(bool active)
	{
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			((Component)currentPet).gameObject.SetActive(active);
		}
	}

	public void TogglePetVisible(bool visible)
	{
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			currentPet.Visible = visible;
		}
	}

	public void ToggleVisor(bool active)
	{
		((Component)visor).gameObject.SetActive(active);
	}

	public void ToggleNameVisible(bool visible)
	{
		isNameVisible = visible;
		UpdateNameVisibility();
	}

	public bool IsSkinPlayingRunAnim()
	{
		return skin.IsPlayingRunAnim();
	}

	private void UpdateNameVisibility()
	{
		((Component)nameText).gameObject.SetActive(visible && isNameVisible);
		((Component)colorBlindText).gameObject.SetActive(visible && isNameVisible && DataManager.Settings.Accessibility.ColorBlindMode);
	}

	private static SpriteMaskInteraction GetBodySpriteMaskInteraction(PlayerMaterial.MaskType type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		SpriteMaskInteraction result = (SpriteMaskInteraction)0;
		switch (type)
		{
		case PlayerMaterial.MaskType.SimpleUI:
			result = (SpriteMaskInteraction)1;
			break;
		case PlayerMaterial.MaskType.Exile:
			result = (SpriteMaskInteraction)2;
			break;
		}
		return result;
	}

	private IEnumerator CoLoadAndSetPetIdle(PetData petData, int colorId, Action onComplete = null)
	{
		((MonoBehaviour)(object)this).UnloadAddressableAsset(petAsset);
		yield return AddressableAssetExtensions.CoLoadAssetAsync<PetBehaviour>((MonoBehaviour)(object)this, (IAddressableAssetProvider<PetBehaviour>)petData, (Action<PetBehaviour>)((PetBehaviour pet) =>
		{
			SetPetIdle(pet, colorId, onComplete);
		}));
	}

	private void InstantiatePetCopy(PetBehaviour petBehaviour, int colorId)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			Object.Destroy((Object)(object)((Component)currentPet).gameObject);
			currentPet = null;
		}
		currentPet = Object.Instantiate<PetBehaviour>(petBehaviour, petParent);
		if (uiPet)
		{
			currentPet.PrepareForUI();
		}
		currentPet.SetCrewmateColor(colorId);
		((Component)currentPet).transform.localPosition = Vector3.zero;
		currentPet.SetDefaultMaterial();
		SetPetFlipX(FlipX);
		if (localPlayer)
		{
			currentPet.SetAsLocalPlayer();
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)currentPet) && (Object)(object)((Component)currentPet).transform.parent == (Object)null)
		{
			Object.Destroy((Object)(object)((Component)currentPet).gameObject);
		}
	}

	private void SetColorBlindColor(int color)
	{
		if (!((Object)(object)colorBlindText == (Object)null) && showColorBlindText)
		{
			((TMP_Text)colorBlindText).text = GetColorBlindText();
		}
	}

	public string GetColorBlindText()
	{
		char[] array = Palette.GetColorName(bodyMatProperties.ColorId).ToCharArray();
		if (array.Length != 0)
		{
			array[0] = char.ToUpper(array[0]);
			for (int i = 1; i < array.Length; i++)
			{
				array[i] = char.ToLower(array[i]);
			}
		}
		return new string(array);
	}

	private void SetCosmeticZIndices()
	{
		if (Object.op_Implicit((Object)(object)hat))
		{
			SpriteRenderer backLayer = hat.BackLayer;
			if (backLayer != null)
			{
				((Component)backLayer).transform.SetLocalZ(1f * zIndexSpacing);
			}
			SpriteRenderer frontLayer = hat.FrontLayer;
			if (frontLayer != null)
			{
				((Component)frontLayer).transform.SetLocalZ(-2f * zIndexSpacing);
			}
		}
		if (Object.op_Implicit((Object)(object)visor))
		{
			visor.ZIndexSpacing = zIndexSpacing;
			((Component)visor).transform.SetLocalZ(-3f * zIndexSpacing);
		}
		if (Object.op_Implicit((Object)(object)petParent))
		{
			((Component)petParent).transform.SetLocalZ(-4f * zIndexSpacing);
		}
		if (Object.op_Implicit((Object)(object)skin))
		{
			((Component)skin).transform.SetLocalZ(-1f * zIndexSpacing);
		}
	}

	private void SetPetColor(int color)
	{
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			currentPet.SetCrewmateColor(color);
		}
	}

	private void UpdateBodyMaterial()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		PlayerMaterial.MaskType maskType = bodyMatProperties.MaskType;
		Material bodyMaterial = GetBodyMaterial(maskType);
		SpriteMaskInteraction bodySpriteMaskInteraction = GetBodySpriteMaskInteraction(maskType);
		foreach (PlayerBodySprite bodySprite in bodySprites)
		{
			((Renderer)bodySprite.BodySprite).sharedMaterial = bodyMaterial;
			bodySprite.BodySprite.maskInteraction = bodySpriteMaskInteraction;
		}
		if (currentBodySprite != null)
		{
			PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(currentBodySprite.BodySprite, localPlayer);
			if (bodyMatProperties.MaskLayer > 0)
			{
				((Renderer)currentBodySprite.BodySprite).material.SetInt(PlayerMaterial.MaskLayer, bodyMatProperties.MaskLayer);
			}
			PlayerMaterial.SetColors(bodyMatProperties.ColorId, (Renderer)(object)currentBodySprite.BodySprite);
			if (currentBodySprite != null && (Object)(object)currentBodySprite.PettingHand != (Object)null)
			{
				PlayerMaterial.SetColors(bodyMatProperties.ColorId, (Renderer)(object)currentBodySprite.PettingHand.HandSprite);
			}
			if (currentBodySprite != null && (Object)(object)currentBodySprite.HandHat != (Object)null)
			{
				PlayerMaterial.SetColors(bodyMatProperties.ColorId, (Renderer)(object)currentBodySprite.HandHat);
			}
		}
	}

	public void HideHatAndVisor()
	{
		hat.Visible = false;
		visor.Visible = false;
	}

	public void FadeBlackCosmetics(float duration)
	{
		((MonoBehaviour)this).StartCoroutine(Effects.Lerp(duration, (float t) =>
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)hat))
			{
				SetHatColor(Color.Lerp(Color.white, Color.black, t));
			}
			if (Object.op_Implicit((Object)(object)visor))
			{
				visor.SetVisorColor(Color.Lerp(Color.white, Color.black, t));
			}
			if (Object.op_Implicit((Object)(object)skin) && bodyType != PlayerBodyTypes.Horse)
			{
				skin.layer.color = Color.Lerp(Color.white, Color.black, t);
			}
			if (Object.op_Implicit((Object)(object)currentPet))
			{
				currentPet.SetRawColor(Color.Lerp(Color.white, Color.black, t));
			}
			currentBodySprite.BodySprite.color = Color.Lerp(Color.white, Color.black, t);
		}));
	}

	public void AnimateClimb(bool down)
	{
		skin.SetClimb(down);
		hat.SetClimbAnim();
	}

	public void AnimatePetMourning()
	{
		if (Object.op_Implicit((Object)(object)currentPet))
		{
			currentPet.SetMourning();
		}
	}

	public void AnimateSkinJump()
	{
		skin.SetJump(FlipX);
	}

	public void AnimateSkinEnterVent()
	{
		skin.SetEnterVent(FlipX);
	}

	public void AnimateSkinExitVent()
	{
		skin.SetExitVent(FlipX);
	}

	public void AnimateSkinIdle()
	{
		skin.SetIdle(FlipX);
	}

	public void AnimateSkinRun()
	{
		skin.SetRun(FlipX);
	}

	public void AnimateSkinSpawn(float time = 0f)
	{
		skin.SetSpawn(FlipX, time);
	}

	public bool HasSkinLoaded()
	{
		return skin.IsLoaded;
	}

	public bool HasVisorLoaded()
	{
		return visor.IsLoaded;
	}

	public bool HaveAllCosmeticsLoaded()
	{
		if (visor.IsLoaded && skin.IsLoaded)
		{
			return hat.IsLoaded;
		}
		return false;
	}

	public void SetAsLocalPlayer()
	{
		localPlayer = true;
		PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(currentBodySprite.BodySprite, isLocalPlayer: true);
		hat.SetLocalPlayer(localPlayer: true);
		skin.SetLocalPlayer(localPlayer: true);
		visor.SetLocalPlayer(localPlayer: true);
		if ((Object)(object)currentPet != (Object)null)
		{
			currentPet.SetAsLocalPlayer();
		}
		((TMP_Text)nameText).fontMaterial.SetFloat("_StencilComp", 7f);
		((TMP_Text)nameText).fontMaterial.SetFloat("_Stencil", 2f);
		((TMP_Text)colorBlindText).fontMaterial.SetFloat("_StencilComp", 7f);
		((TMP_Text)colorBlindText).fontMaterial.SetFloat("_Stencil", 2f);
	}
}
