using System;
using System.Collections;
using Innersloth.Assets;
using PowerTools;
using UnityEngine;

public class SkinLayer : MonoBehaviour
{
	private readonly int JUMP_ZIPLINE_FRAME_STOP = 5;

	public SpriteRenderer layer;

	public SpriteAnim animator;

	public SkinViewData skin;

	private PlayerMaterial.Properties matProperties;

	public bool IsLoaded => (Object)(object)skin != (Object)null;

	public bool Flipped
	{
		set
		{
			if (!Object.op_Implicit((Object)(object)skin))
			{
				layer.flipX = value;
			}
			else
			{
				layer.flipX = value && (Object)(object)animator.Clip != (Object)(object)skin.IdleLeftAnim && (Object)(object)animator.Clip != (Object)(object)skin.RunLeftAnim && (Object)(object)animator.Clip != (Object)(object)skin.EnterLeftVentAnim && (Object)(object)animator.Clip != (Object)(object)skin.ExitLeftVentAnim && (Object)(object)animator.Clip != (Object)(object)skin.SpawnLeftAnim;
			}
		}
	}

	public bool Visible
	{
		set
		{
			((Renderer)layer).enabled = value;
		}
	}

	public void SetMaskType(PlayerMaterial.MaskType maskType)
	{
		matProperties.MaskType = maskType;
		UpdateMaterial();
	}

	public void SetMaskLayer(int layer)
	{
		matProperties.MaskLayer = layer;
		UpdateMaterial();
	}

	public void SetLocalPlayer(bool localPlayer)
	{
		matProperties.IsLocalPlayer = localPlayer;
		UpdateMaterial();
	}

	public void SetRun(bool isLeft)
	{
		if (!Object.op_Implicit((Object)(object)skin) || !Object.op_Implicit((Object)(object)animator))
		{
			SetGhost();
			return;
		}
		if (isLeft && Object.op_Implicit((Object)(object)skin.RunLeftAnim))
		{
			if (!animator.IsPlaying(skin.RunLeftAnim))
			{
				animator.Play(skin.RunLeftAnim);
				animator.Time = 11f / 24f;
			}
		}
		else if (!animator.IsPlaying(skin.RunAnim))
		{
			animator.Play(skin.RunAnim);
			animator.Time = 11f / 24f;
		}
		Flipped = isLeft;
	}

	public void SetSpawn(bool isLeft, float time = 0f)
	{
		if (!Object.op_Implicit((Object)(object)skin) || !Object.op_Implicit((Object)(object)animator))
		{
			SetGhost();
			return;
		}
		if (isLeft && Object.op_Implicit((Object)(object)skin.SpawnLeftAnim))
		{
			animator.Play(skin.SpawnLeftAnim);
		}
		else
		{
			animator.Play(skin.SpawnAnim);
		}
		Flipped = isLeft;
		animator.Time = time;
	}

	public void SetClimb(bool down)
	{
		if (!Object.op_Implicit((Object)(object)skin) || !Object.op_Implicit((Object)(object)animator))
		{
			SetGhost();
			return;
		}
		animator.Play(down ? skin.ClimbDownAnim : skin.ClimbAnim);
		animator.Time = 0f;
	}

	public void SetExitVent(bool isLeft)
	{
		if (!Object.op_Implicit((Object)(object)skin) || !Object.op_Implicit((Object)(object)animator))
		{
			SetGhost();
			return;
		}
		if (isLeft && Object.op_Implicit((Object)(object)skin.ExitLeftVentAnim))
		{
			animator.Play(skin.ExitLeftVentAnim);
		}
		else
		{
			animator.Play(skin.ExitVentAnim);
		}
		Flipped = isLeft;
		animator.Time = 0f;
	}

	public void SetJump(bool isLeft)
	{
		if (!Object.op_Implicit((Object)(object)skin) || !Object.op_Implicit((Object)(object)animator))
		{
			SetGhost();
			return;
		}
		if (isLeft && Object.op_Implicit((Object)(object)skin.EnterLeftVentAnim))
		{
			animator.Play(skin.EnterLeftVentAnim);
		}
		else
		{
			animator.Play(skin.EnterVentAnim);
		}
		Flipped = isLeft;
		animator.Time = 0f;
		((MonoBehaviour)this).StartCoroutine(CoCheckJumpAnim());
	}

	private IEnumerator CoCheckJumpAnim()
	{
		while (animator.FrameTime < JUMP_ZIPLINE_FRAME_STOP)
		{
			yield return null;
		}
		animator.Stop();
	}

	public void SetEnterVent(bool isLeft)
	{
		if (!Object.op_Implicit((Object)(object)skin) || !Object.op_Implicit((Object)(object)animator))
		{
			SetGhost();
			return;
		}
		if (isLeft && Object.op_Implicit((Object)(object)skin.EnterLeftVentAnim))
		{
			animator.Play(skin.EnterLeftVentAnim);
		}
		else
		{
			animator.Play(skin.EnterVentAnim);
		}
		Flipped = isLeft;
		animator.Time = 0f;
	}

	public void SetIdle(bool isLeft)
	{
		if (!Object.op_Implicit((Object)(object)skin) || !Object.op_Implicit((Object)(object)animator))
		{
			SetGhost();
			return;
		}
		if (isLeft && Object.op_Implicit((Object)(object)skin.IdleLeftAnim))
		{
			if (!animator.IsPlaying(skin.IdleLeftAnim))
			{
				animator.Play(skin.IdleLeftAnim);
			}
		}
		else if (!animator.IsPlaying(skin.IdleAnim))
		{
			animator.Play(skin.IdleAnim);
		}
		Flipped = isLeft;
	}

	public void SetGhost()
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.Stop();
			layer.sprite = null;
		}
	}

	public void SetSkin(string skinId, int color, bool isLeft, Action onLoaded = null)
	{
		SetSkin(DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId), color, isLeft, onLoaded);
	}

	public void SetSkin(SkinData skinData, int color, bool isLeft, Action onLoaded = null)
	{
		AddressableAssetExtensions.LoadAssetAsync<SkinViewData>((MonoBehaviour)(object)this, (IAddressableAssetProvider<SkinViewData>)skinData, (Action<SkinViewData>)((SkinViewData skinView) =>
		{
			if (!((MonoBehaviour)(object)this).IsDestroyedOrNull() && !((Component)this).gameObject.IsDestroyedOrNull())
			{
				SetSkin(skinView, color, isLeft);
				onLoaded?.Invoke();
			}
		}), (Action)null);
	}

	public void SetSkin(SkinViewData skin, int color, bool isLeft)
	{
		this.skin = skin;
		matProperties.ColorId = color;
		UpdateMaterial();
		SetIdle(isLeft);
	}

	public void SetColor(int color)
	{
		matProperties.ColorId = color;
		UpdateMaterial();
	}

	public bool IsPlayingRunAnim()
	{
		if (!((Object)(object)animator.GetCurrentAnimation() == (Object)(object)skin.RunAnim))
		{
			return (Object)(object)animator.GetCurrentAnimation() == (Object)(object)skin.RunLeftAnim;
		}
		return true;
	}

	private void UpdateMaterial()
	{
		PlayerMaterial.MaskType maskType = matProperties.MaskType;
		if (Object.op_Implicit((Object)(object)skin) && skin.MatchPlayerColor)
		{
			if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
			{
				((Renderer)layer).sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedPlayerMaterial;
			}
			else
			{
				((Renderer)layer).sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
			}
		}
		else if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
		{
			((Renderer)layer).sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedMaterial;
		}
		else
		{
			((Renderer)layer).sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
		}
		switch (maskType)
		{
		case PlayerMaterial.MaskType.SimpleUI:
			layer.maskInteraction = (SpriteMaskInteraction)1;
			break;
		case PlayerMaterial.MaskType.Exile:
			layer.maskInteraction = (SpriteMaskInteraction)2;
			break;
		default:
			layer.maskInteraction = (SpriteMaskInteraction)0;
			break;
		}
		((Renderer)layer).material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
		if (Object.op_Implicit((Object)(object)skin) && skin.MatchPlayerColor)
		{
			PlayerMaterial.SetColors(matProperties.ColorId, (Renderer)(object)layer);
		}
		if (matProperties.MaskLayer <= 0)
		{
			PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(layer, matProperties.IsLocalPlayer);
		}
	}
}
