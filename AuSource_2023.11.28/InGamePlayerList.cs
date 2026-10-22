using System;
using System.Collections;
using UnityEngine;

public class InGamePlayerList : MonoBehaviour
{
	private enum ListState
	{
		Closed,
		Open,
		Opening,
		Closing
	}

	public Vector3 offsetFromEdge;

	private float windowWidth = 1f;

	private Vector3 openPosition;

	private Vector3 closedPosition;

	public float openTime = 0.5f;

	public float buttonHeight = 0.35f;

	public ObjectPoolBehavior buttonPool;

	public ControllerNavMenu controllerNavMenu;

	public SpriteRenderer backgroundSprite;

	public SpriteRenderer backgroundSpriteMask;

	public GameObject ChatScreen;

	public ControllerButtonBehavior OpenChatButton;

	private ListState currentState;

	private float openLevel;

	public static InGamePlayerList instance;

	public bool IsActive
	{
		get
		{
			if (currentState != ListState.Open)
			{
				return currentState == ListState.Opening;
			}
			return true;
		}
	}

	private void Awake()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		instance = this;
		((Component)this).gameObject.SetActive(false);
		windowWidth = backgroundSprite.size.x;
		Camera main = Camera.main;
		_ = main.aspect;
		_ = main.orthographicSize;
		offsetFromEdge.x += windowWidth * 0.5f;
		SetBackgroundMaskLayer(0);
	}

	private void OnEnable()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		openPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Left, offsetFromEdge);
		closedPosition = openPosition;
		closedPosition.x += (0f - windowWidth) * 1.05f;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void RefreshMenu()
	{
		controllerNavMenu.ControllerSelectable.Clear();
		PopulateButtons();
		controllerNavMenu.DefaultButtonSelected = controllerNavMenu.ControllerSelectable[0];
	}

	public void ToggleActive()
	{
		if (!ChatScreen.activeInHierarchy && !((Object)(object)ShipStatus.Instance == (Object)null))
		{
			switch (currentState)
			{
			case ListState.Closed:
				currentState = ListState.Opening;
				((Component)this).gameObject.SetActive(true);
				RefreshMenu();
				((MonoBehaviour)this).StartCoroutine(TransitionCoroutine(1f));
				break;
			case ListState.Open:
				currentState = ListState.Closing;
				((MonoBehaviour)this).StartCoroutine(TransitionCoroutine(0f));
				break;
			case ListState.Opening:
				currentState = ListState.Closing;
				((MonoBehaviour)this).StopAllCoroutines();
				((MonoBehaviour)this).StartCoroutine(TransitionCoroutine(0f));
				break;
			case ListState.Closing:
				currentState = ListState.Opening;
				((MonoBehaviour)this).StopAllCoroutines();
				((MonoBehaviour)this).StartCoroutine(TransitionCoroutine(1f));
				break;
			}
		}
	}

	public void SetActive(bool active)
	{
		switch (currentState)
		{
		case ListState.Closed:
			if (active)
			{
				currentState = ListState.Opening;
				RefreshMenu();
				((Component)this).gameObject.SetActive(true);
				((MonoBehaviour)this).StartCoroutine(TransitionCoroutine(1f));
			}
			break;
		case ListState.Open:
			if (!active)
			{
				currentState = ListState.Closing;
				((MonoBehaviour)this).StartCoroutine(TransitionCoroutine(0f));
			}
			break;
		case ListState.Opening:
			if (!active)
			{
				currentState = ListState.Closing;
				((MonoBehaviour)this).StopAllCoroutines();
				((MonoBehaviour)this).StartCoroutine(TransitionCoroutine(0f));
			}
			break;
		case ListState.Closing:
			if (active)
			{
				currentState = ListState.Opening;
				((MonoBehaviour)this).StopAllCoroutines();
				((MonoBehaviour)this).StartCoroutine(TransitionCoroutine(1f));
			}
			break;
		}
	}

	private IEnumerator TransitionCoroutine(float targetValue)
	{
		switch (currentState)
		{
		case ListState.Opening:
			((Behaviour)OpenChatButton).enabled = false;
			break;
		case ListState.Closing:
			((Behaviour)OpenChatButton).enabled = true;
			break;
		}
		yield return null;
		float dir = Mathf.Sign(targetValue - openLevel);
		while (openLevel != targetValue)
		{
			openLevel += dir * Time.deltaTime / openTime;
			if ((dir > 0f && openLevel >= targetValue) || (dir < 0f && openLevel <= targetValue))
			{
				openLevel = targetValue;
			}
			float num = ((!(dir >= 0f)) ? Mathf.Sin((float)Math.PI * (1f - 0.5f * openLevel)) : Mathf.Sin((float)Math.PI / 2f * openLevel));
			((Component)this).transform.localPosition = Vector3.Lerp(closedPosition, openPosition, num);
			yield return null;
		}
		((Component)this).transform.localPosition = Vector3.Lerp(closedPosition, openPosition, targetValue);
		switch (currentState)
		{
		case ListState.Opening:
			currentState = ListState.Open;
			break;
		case ListState.Closing:
			currentState = ListState.Closed;
			((Component)this).gameObject.SetActive(false);
			break;
		}
	}

	private void SetBackgroundMaskLayer(int maskLayer)
	{
		((Renderer)backgroundSpriteMask).material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		((Renderer)backgroundSpriteMask).material.SetInt("_MaskComp", 8);
	}

	private void PopulateButtons()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		buttonPool.ReclaimAll();
		for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
		{
			PlayerIdentifierButton component = ((Component)buttonPool.Get<PoolableBehavior>()).GetComponent<PlayerIdentifierButton>();
			Vector3 localPosition = new Vector3(0f, (0f - buttonHeight) * (float)i, 0f);
			((Component)component).transform.localPosition = localPosition;
			((Component)component).transform.localScale = Vector3.one;
			GameData.PlayerInfo player = GameData.Instance.AllPlayers[i];
			component.Populate(player);
			UiElement componentInChildren = ((Component)component).GetComponentInChildren<UiElement>(true);
			controllerNavMenu.ControllerSelectable.Add(componentInChildren);
		}
	}
}
