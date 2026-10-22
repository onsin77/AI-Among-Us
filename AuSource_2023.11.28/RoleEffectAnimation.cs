using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class RoleEffectAnimation : MonoBehaviour
{
	public enum SoundType
	{
		None,
		Local,
		Global
	}

	public enum EffectType
	{
		Default,
		ProtectLoop,
		Shapeshift
	}

	public EffectType effectType;

	public AnimationClip Clip;

	public SpriteAnim Animator;

	public Action MidAnimCB;

	public SpriteRenderer Renderer;

	public AudioClip UseSound;

	public AudioSource AudioSource;

	private PlayerControl parent;

	public void Play(PlayerControl parent, Action onClipEnded, bool flipX, SoundType soundType, float duration = 0f)
	{
		((MonoBehaviour)this).StartCoroutine(CoPlay(parent, onClipEnded, flipX, soundType, duration));
	}

	private IEnumerator CoPlay(PlayerControl parent, Action onClipEnded, bool flipX, SoundType soundType, float duration = 0f)
	{
		this.parent = parent;
		parent.currentRoleAnimations.Add(this);
		Renderer.flipX = flipX;
		if (Object.op_Implicit((Object)(object)UseSound))
		{
			switch (soundType)
			{
			case SoundType.Global:
				SoundManager.Instance.PlaySound(UseSound, loop: false);
				break;
			case SoundType.Local:
				PlaySound();
				break;
			}
		}
		if (duration > 0f)
		{
			yield return (object)new WaitForSeconds(duration);
		}
		else
		{
			yield return new WaitForAnimationFinish(Animator, Clip);
		}
		if ((Object)(object)MeetingHud.Instance == (Object)null)
		{
			onClipEnded?.Invoke();
		}
		parent.currentRoleAnimations.Remove(this);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void MidAnimationCallback()
	{
		MidAnimCB();
	}

	public void SetMaskLayerBasedOnWhoShouldSee(bool shouldBeVisible)
	{
		PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(Renderer, shouldBeVisible);
	}

	public void SetMaterialColor(int colorId)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (colorId >= 0 && colorId < Palette.PlayerColors.Length)
		{
			((Renderer)Renderer).material.SetColor("_BackColor", Color32.op_Implicit(Palette.ShadowColors[colorId]));
			((Renderer)Renderer).material.SetColor("_BodyColor", Color32.op_Implicit(Palette.PlayerColors[colorId]));
			((Renderer)Renderer).material.SetColor("_VisorColor", Color32.op_Implicit(Palette.VisorColor));
		}
	}

	public void PlaySound()
	{
		if (Constants.ShouldPlaySfx() && !((Object)(object)PlayerControl.LocalPlayer != (Object)(object)parent))
		{
			AudioSource.clip = UseSound;
			AudioSource.Play();
		}
	}

	public void ToggleRenderer(bool on)
	{
		((Renderer)Renderer).enabled = on;
	}
}
