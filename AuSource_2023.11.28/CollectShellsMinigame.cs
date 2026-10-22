using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class CollectShellsMinigame : Minigame
{
	[SerializeField]
	private CollectableShell[] shellPrefabs;

	[SerializeField]
	private Transform[] spawnPositions;

	[SerializeField]
	private IntRange numShellsRange;

	[SerializeField]
	private GameObject controllerCursor;

	[SerializeField]
	private GameObject handWipeObject;

	[Header("Audio")]
	[SerializeField]
	private AudioClip[] dustSounds;

	private List<CollectableShell> shells = new List<CollectableShell>();

	private Controller controller = new Controller();

	private Vector2 prevMousePos;

	private Vector2 prevControllerStickPos;

	private float wipeDistance = 0.5f;

	private float maxProgress = 2f;

	private float controllerDifficulty = 0.5f;

	private float mouseDifficulty = 1f;

	public override void Begin(PlayerTask task)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		SetupInput(disableCursor: true);
		SpriteRenderer[] componentsInChildren = controllerCursor.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer playerMaterialColors in componentsInChildren)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
		if (MyNormTask.MaxStep == 1)
		{
			MyNormTask.MaxStep = numShellsRange.Next();
			MyNormTask.ShowTaskStep = true;
		}
		int num = MyNormTask.MaxStep - MyNormTask.TaskStep;
		List<Transform> list = new List<Transform>(spawnPositions);
		for (int j = 0; j < num; j++)
		{
			CollectableShell collectableShell = shellPrefabs.Random();
			Transform val = list.Random();
			list.Remove(val);
			CollectableShell collectableShell2 = Object.Instantiate<CollectableShell>(collectableShell, val, false);
			((Component)collectableShell2).transform.localPosition = Vector3.zero;
			collectableShell2.SetOnSandClearedAction(OnSandCleared);
			shells.Add(collectableShell2);
		}
	}

	public void Update()
	{
		if (amClosing == CloseState.None)
		{
			controller.Update();
			if (Controller.currentTouchType == Controller.TouchType.Joystick)
			{
				UpdateController();
			}
			else
			{
				UpdateTouch();
			}
		}
	}

	private void UpdateTouch()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		controllerCursor.gameObject.SetActive(false);
		Controller controller = DestroyableSingleton<PassiveButtonManager>.Instance.controller;
		if (!controller.AnyTouch)
		{
			return;
		}
		Vector2 position = controller.Touches[0].Position;
		foreach (CollectableShell shell in shells)
		{
			if (!((Behaviour)shell).isActiveAndEnabled)
			{
				continue;
			}
			for (int i = 0; i < shell.SandButtons.Count; i++)
			{
				PassiveButton passiveButton = shell.SandButtons[i];
				if (((Behaviour)passiveButton).isActiveAndEnabled && passiveButton.Colliders[0].OverlapPoint(position))
				{
					float num = Mathf.Abs(prevMousePos.x - position.x) + Mathf.Abs(prevMousePos.y - position.y);
					num *= mouseDifficulty;
					SpriteRenderer sandSprite = shell.GetSandSprite(i);
					WipeSand(passiveButton, sandSprite, num);
					prevMousePos = position;
				}
			}
			shell.CheckIfAllSandCleared();
		}
	}

	private void UpdateController()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		controllerCursor.gameObject.SetActive(true);
		Player player = ReInput.players.GetPlayer(0);
		Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
		Vector2 axis2DRaw2 = player.GetAxis2DRaw(16, 17);
		Vector3 localPosition = controllerCursor.transform.localPosition;
		Vector3 localPosition2 = handWipeObject.transform.localPosition;
		localPosition.x += axis2DRaw.x * Time.deltaTime * 3f;
		localPosition.y += axis2DRaw.y * Time.deltaTime * 3f;
		localPosition2.x = axis2DRaw2.x * wipeDistance;
		localPosition2.y = axis2DRaw2.y * wipeDistance;
		controllerCursor.transform.localPosition = localPosition;
		handWipeObject.transform.localPosition = localPosition2;
		float num = Mathf.Abs(prevControllerStickPos.x - axis2DRaw2.x) + Mathf.Abs(prevControllerStickPos.y - axis2DRaw2.y);
		num *= controllerDifficulty;
		prevControllerStickPos = axis2DRaw2;
		foreach (CollectableShell shell in shells)
		{
			if (!((Behaviour)shell).isActiveAndEnabled)
			{
				continue;
			}
			for (int i = 0; i < shell.SandButtons.Count; i++)
			{
				PassiveButton passiveButton = shell.SandButtons[i];
				if (((Behaviour)passiveButton).isActiveAndEnabled && passiveButton.Colliders[0].OverlapPoint(Vector2.op_Implicit(controllerCursor.transform.position)))
				{
					SpriteRenderer sandSprite = shell.GetSandSprite(i);
					WipeSand(passiveButton, sandSprite, num);
				}
			}
			shell.CheckIfAllSandCleared();
		}
	}

	private void WipeSand(PassiveButton sand, SpriteRenderer sprite, float diff)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		diff = Mathf.Min(diff, Time.deltaTime * maxProgress);
		if (!(diff <= 0f))
		{
			VibrationManager.Vibrate(0.3f, 0.3f);
			Color color = sprite.color;
			color.a -= diff;
			sprite.color = color;
			if (color.a <= 0.05f)
			{
				((Component)sand).gameObject.SetActive(false);
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(dustSounds.Random(), loop: false);
			}
		}
	}

	private void OnSandCleared()
	{
		MyNormTask.NextStep();
		VibrationManager.Vibrate(0.3f, 0.3f);
		if (MyNormTask.IsComplete)
		{
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
	}
}
