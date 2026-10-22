using System;
using System.Collections;
using UnityEngine;

public abstract class Minigame : MonoBehaviour
{
	protected enum CloseState
	{
		None,
		Waiting,
		Closing
	}

	public static Minigame Instance;

	public const float Depth = -50f;

	public TransitionType TransType;

	public Vector2 TargetPosition;

	protected PlayerTask MyTask;

	protected NormalPlayerTask MyNormTask;

	protected CloseState amClosing;

	protected bool amOpening;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	[NonSerialized]
	public SpecialInputHandler inputHandler;

	private bool multistageMinigameChecked;

	private MultistageMinigame multistageMinigameParent;

	private readonly Logger logger = new Logger("Minigame", Logger.Level.Info, Logger.Category.Gameplay);

	private float timeOpened;

	public TaskTypes TaskType
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)MyTask))
			{
				return TaskTypes.None;
			}
			return MyTask.TaskType;
		}
	}

	public Console Console { get; set; }

	protected int ConsoleId
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)Console))
			{
				return 0;
			}
			return Console.ConsoleId;
		}
	}

	public bool HasMultistageMinigameParent => (Object)(object)GetMultistageMinigameParentOnce() != (Object)null;

	public MultistageMinigame MultistageMinigameParent => GetMultistageMinigameParentOnce();

	public virtual bool SkipMultistageOverlayMenuSetup => false;

	public virtual void Begin(PlayerTask task)
	{
		Instance = this;
		MyTask = task;
		MyNormTask = task as NormalPlayerTask;
		timeOpened = Time.realtimeSinceStartup;
		if (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			if (Object.op_Implicit((Object)(object)MapBehaviour.Instance))
			{
				MapBehaviour.Instance.Close();
			}
			PlayerControl.LocalPlayer.NetTransform.Halt();
		}
		logger.Info("Opening minigame " + ((object)this).GetType().Name);
		((MonoBehaviour)this).StartCoroutine(CoAnimateOpen());
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MinigameOpened(PlayerControl.LocalPlayer.Data, TaskType);
	}

	protected IEnumerator CoStartClose(float duration = 0.75f)
	{
		if (amClosing == CloseState.None)
		{
			amClosing = CloseState.Waiting;
			yield return Effects.Wait(duration);
			Close();
		}
	}

	[Obsolete("Don't use, I just don't want to reselect the close button event handlers", true)]
	public void Close(bool allowMovement)
	{
		Close();
	}

	public void ForceClose()
	{
		Close();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public virtual void Close()
	{
		if (amClosing != CloseState.Closing)
		{
			if (Object.op_Implicit((Object)(object)CloseSound) && Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(CloseSound, loop: false);
			}
			if (PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Crewmate)
			{
				GameManager.Instance.LogicMinigame.OnMinigameClose();
			}
			if (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
			{
				PlayerControl.HideCursorTemporarily();
			}
			amClosing = CloseState.Closing;
			logger.Info("Closing minigame " + ((object)this).GetType().Name);
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MinigameClosed(PlayerControl.LocalPlayer.Data, TaskType, Time.realtimeSinceStartup - timeOpened, MyTask?.IsComplete ?? false);
			((MonoBehaviour)this).StartCoroutine(CoDestroySelf());
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	protected virtual IEnumerator CoAnimateOpen()
	{
		amOpening = true;
		if (Object.op_Implicit((Object)(object)OpenSound) && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(OpenSound, loop: false);
		}
		float depth = ((Component)this).transform.localPosition.z;
		switch (TransType)
		{
		case TransitionType.None:
			((Component)this).transform.localPosition = new Vector3(TargetPosition.x, TargetPosition.y, depth);
			break;
		case TransitionType.SlideBottom:
		{
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float num2 = timer / 0.25f;
				((Component)this).transform.localPosition = new Vector3(TargetPosition.x, Mathf.SmoothStep(-8f, TargetPosition.y, num2), depth);
				yield return null;
			}
			((Component)this).transform.localPosition = new Vector3(TargetPosition.x, TargetPosition.y, depth);
			break;
		}
		case TransitionType.Alpha:
		{
			SpriteRenderer[] rends = ((Component)this).GetComponentsInChildren<SpriteRenderer>();
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float num = timer / 0.25f;
				for (int i = 0; i < rends.Length; i++)
				{
					rends[i].color = Color.Lerp(Palette.ClearWhite, Color.white, num);
				}
				yield return null;
			}
			for (int j = 0; j < rends.Length; j++)
			{
				rends[j].color = Color.white;
			}
			break;
		}
		}
		if (PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Crewmate)
		{
			GameManager.Instance.LogicMinigame.OnMinigameOpen();
		}
		amOpening = false;
	}

	protected virtual IEnumerator CoDestroySelf()
	{
		switch (TransType)
		{
		case TransitionType.SlideBottom:
		{
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float num2 = timer / 0.25f;
				((Component)this).transform.localPosition = new Vector3(0f, Mathf.SmoothStep(0f, -8f, num2), -50f);
				yield return null;
			}
			break;
		}
		case TransitionType.Alpha:
		{
			SpriteRenderer[] rends = ((Component)this).GetComponentsInChildren<SpriteRenderer>();
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float num = timer / 0.25f;
				for (int i = 0; i < rends.Length; i++)
				{
					rends[i].color = Color.Lerp(Color.white, Palette.ClearWhite, num);
				}
				yield return null;
			}
			break;
		}
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void SetupInput(bool disableCursor, bool suppressInGamePlayerList = false)
	{
		inputHandler = ((Component)this).GetComponentInChildren<SpecialInputHandler>(true);
		if ((Object)(object)inputHandler == (Object)null)
		{
			inputHandler = ((Component)this).gameObject.AddComponent<SpecialInputHandler>();
		}
		if (disableCursor)
		{
			inputHandler.disableVirtualCursor = true;
		}
		if (suppressInGamePlayerList)
		{
			inputHandler.suppressInGamePlayerList = true;
		}
		ConsoleJoystick.SetMode_Task();
	}

	private MultistageMinigame GetMultistageMinigameParentOnce()
	{
		if (!multistageMinigameChecked)
		{
			multistageMinigameChecked = true;
			multistageMinigameParent = ((Component)this).GetComponentInParent<MultistageMinigame>();
		}
		return multistageMinigameParent;
	}
}
