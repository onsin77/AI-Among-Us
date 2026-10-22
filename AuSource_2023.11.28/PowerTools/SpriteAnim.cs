using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerTools;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class SpriteAnim : SpriteAnimEventHandler
{
	private static readonly string STATE_NAME = "a";

	private static readonly string CONTROLLER_PATH = "SpriteAnimController";

	[SerializeField]
	private AnimationClip m_defaultAnim;

	private static RuntimeAnimatorController m_sharedAnimatorController = null;

	private Animator m_animator;

	private AnimatorOverrideController m_controller;

	private SpriteAnimNodes m_nodes;

	private List<KeyValuePair<AnimationClip, AnimationClip>> m_clipPairList = new List<KeyValuePair<AnimationClip, AnimationClip>>(1);

	private AnimationClip m_currAnim;

	private float m_speed = 1f;

	private bool m_initialized;

	public bool Playing => IsPlaying();

	public bool Paused
	{
		get
		{
			return IsPaused();
		}
		set
		{
			if (value)
			{
				Pause();
			}
			else
			{
				Resume();
			}
		}
	}

	public float Speed
	{
		get
		{
			return m_speed;
		}
		set
		{
			SetSpeed(value);
		}
	}

	public int FrameTime
	{
		get
		{
			return (int)(GetTime() * 24f);
		}
		set
		{
			SetTime(value / 24);
		}
	}

	public float Time
	{
		get
		{
			return GetTime();
		}
		set
		{
			SetTime(value);
		}
	}

	public float NormalizedTime
	{
		get
		{
			return GetNormalisedTime();
		}
		set
		{
			SetNormalizedTime(value);
		}
	}

	public AnimationClip Clip => m_currAnim;

	public string ClipName
	{
		get
		{
			if (!((Object)(object)m_currAnim != (Object)null))
			{
				return string.Empty;
			}
			return ((Object)m_currAnim).name;
		}
	}

	public void Play(AnimationClip anim = null, float speed = 1f)
	{
		if ((Object)(object)anim == (Object)null)
		{
			anim = (Object.op_Implicit((Object)(object)m_currAnim) ? m_currAnim : m_defaultAnim);
			if ((Object)(object)anim == (Object)null)
			{
				return;
			}
		}
		if (!((Object)(object)m_animator == (Object)null))
		{
			if (!((Behaviour)m_animator).enabled)
			{
				((Behaviour)m_animator).enabled = true;
			}
			if ((Object)(object)m_nodes != (Object)null)
			{
				m_nodes.Reset();
			}
			m_clipPairList[0] = new KeyValuePair<AnimationClip, AnimationClip>(m_clipPairList[0].Key, anim);
			m_controller.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>)m_clipPairList);
			m_animator.Update(0f);
			m_animator.Play(STATE_NAME, 0, 0f);
			m_speed = Mathf.Max(0f, speed);
			m_animator.speed = m_speed;
			m_currAnim = anim;
			m_animator.Update(0f);
		}
	}

	public void Stop()
	{
		((Behaviour)m_animator).enabled = false;
	}

	public void Pause()
	{
		m_animator.speed = 0f;
	}

	public void Resume()
	{
		m_animator.speed = m_speed;
	}

	public AnimationClip GetCurrentAnimation()
	{
		return m_currAnim;
	}

	public bool IsPlaying(AnimationClip clip = null)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)clip == (Object)null || (Object)(object)m_currAnim == (Object)(object)clip)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			return currentAnimatorStateInfo.normalizedTime < 1f;
		}
		return false;
	}

	public bool IsPlaying(string animName)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_currAnim == (Object)null)
		{
			return false;
		}
		if (((Object)m_currAnim).name == animName)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			return currentAnimatorStateInfo.normalizedTime < 1f;
		}
		return false;
	}

	public bool IsPaused()
	{
		if ((Object)(object)m_currAnim == (Object)null)
		{
			return false;
		}
		return m_animator.speed == 0f;
	}

	public void SetSpeed(float speed)
	{
		m_speed = Mathf.Max(0f, speed);
		m_animator.speed = m_speed;
	}

	public float GetSpeed()
	{
		return m_speed;
	}

	public float GetTime()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_currAnim != (Object)null)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			return currentAnimatorStateInfo.normalizedTime * m_currAnim.length;
		}
		return 0f;
	}

	public void SetTime(float time)
	{
		if (!((Object)(object)m_currAnim == (Object)null) && !(m_currAnim.length <= 0f))
		{
			SetNormalizedTime(time / m_currAnim.length);
		}
	}

	public float GetNormalisedTime()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_currAnim != (Object)null)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			return currentAnimatorStateInfo.normalizedTime;
		}
		return 0f;
	}

	public void SetNormalizedTime(float ratio)
	{
		if (!((Object)(object)m_currAnim == (Object)null))
		{
			m_animator.Play(STATE_NAME, 0, ((Motion)m_currAnim).isLooping ? Mathf.Repeat(ratio, 1f) : Mathf.Clamp01(ratio));
		}
	}

	public void Initialize()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		if (!m_initialized)
		{
			m_controller = new AnimatorOverrideController();
			if ((Object)(object)m_sharedAnimatorController == (Object)null)
			{
				m_sharedAnimatorController = Resources.Load<RuntimeAnimatorController>(CONTROLLER_PATH);
			}
			m_controller.runtimeAnimatorController = m_sharedAnimatorController;
			m_animator = ((Component)this).GetComponent<Animator>();
			m_animator.runtimeAnimatorController = (RuntimeAnimatorController)(object)m_controller;
			m_controller.GetOverrides(m_clipPairList);
			m_nodes = ((Component)this).GetComponent<SpriteAnimNodes>();
			m_initialized = true;
		}
	}

	private void Awake()
	{
		Initialize();
		Play(m_defaultAnim);
	}

	private void Reset()
	{
		if ((Object)(object)((Component)this).GetComponent<RectTransform>() == (Object)null)
		{
			if ((Object)(object)((Component)this).GetComponent<Sprite>() == (Object)null)
			{
				((Component)this).gameObject.AddComponent<SpriteRenderer>();
			}
		}
		else if ((Object)(object)((Component)this).GetComponent<Image>() == (Object)null)
		{
			((Component)this).gameObject.AddComponent<Image>();
		}
	}
}
