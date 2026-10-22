using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class VentCleaningMinigame : Minigame
{
	public FloatRange XSpan = new FloatRange(-1.15f, 1.15f);

	public FloatRange YSpan = new FloatRange(-1.15f, 1.15f);

	public ObjectPoolBehavior dirtPool;

	public GameObject ventLidClosed;

	public GameObject ventLidOpened;

	public UiElement backButton;

	public AudioClip VentOpenSound;

	public AudioClip ImpostorDiscoveredSound;

	public AudioClip CleanedSound;

	public SpriteRenderer[] SpiderWebs;

	public float cleaningDistanceThreshold;

	public Transform selectorObject;

	public SpriteRenderer selectorHand;

	private int numberOfDirts;

	private int numberOfDirtsCleanedUp;

	private bool ventOpen;

	private VentilationSystem ventSystem;

	private int VentId => ConsoleId;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		ventOpen = false;
		ventLidClosed.SetActive(true);
		ventLidOpened.SetActive(false);
		numberOfDirts = MyNormTask.Data[MyNormTask.MaxStep];
		numberOfDirtsCleanedUp = 0;
		ventSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation] as VentilationSystem;
		for (int i = 0; i < numberOfDirts; i++)
		{
			SpawnDirt();
		}
		SpiderWebs.ForEach((SpriteRenderer s) =>
		{
			((Renderer)s).enabled = false;
		});
		SpiderWebs.RandomSet(2).ForEach((SpriteRenderer s) =>
		{
			((Renderer)s).enabled = true;
		});
		if (!PlayerControl.LocalPlayer.Data.IsDead)
		{
			VentilationSystem.Update(VentilationSystem.Operation.StartCleaning, VentId);
		}
		((Component)selectorObject).gameObject.SetActive(false);
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)selectorHand);
		SetupInput(disableCursor: false);
	}

	public override void Close()
	{
		if (amClosing != CloseState.Closing)
		{
			VentilationSystem.Update(VentilationSystem.Operation.StopCleaning, VentId);
		}
		base.Close();
	}

	public void OpenVent()
	{
		if (!ventOpen)
		{
			((MonoBehaviour)this).StartCoroutine(CoOpenVent());
		}
	}

	private void FixedUpdate()
	{
		if (amOpening || amClosing != CloseState.None)
		{
			return;
		}
		if (!PlayerControl.LocalPlayer.Data.IsDead && ventSystem != null && ventSystem.IsImpostorInsideVent(VentId))
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ImpostorDiscoveredSound, loop: false, 0.8f);
			}
			VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, VentId);
			Close();
		}
		else if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			HandleJoystick();
		}
		else if (((Component)selectorObject).gameObject.activeSelf)
		{
			((Component)selectorObject).gameObject.SetActive(false);
		}
	}

	private void HandleJoystick()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(0);
		bool button = player.GetButton(11);
		if (ventOpen && !((Component)selectorObject).gameObject.activeSelf)
		{
			((Component)selectorObject).gameObject.SetActive(true);
		}
		if (ventOpen)
		{
			Vector3 position = selectorObject.position;
			position.x = VirtualCursor.currentPosition.x;
			position.y = VirtualCursor.currentPosition.y;
			selectorObject.position = position;
			if (!player.GetButtonUp(11))
			{
				return;
			}
			float num = 0f;
			int num2 = -1;
			List<PoolableBehavior> activeChildren = dirtPool.activeChildren;
			for (int i = 0; i < activeChildren.Count; i++)
			{
				Vector2 val = Vector2.op_Implicit(((Component)activeChildren[i]).transform.localPosition - selectorObject.localPosition);
				float sqrMagnitude = val.sqrMagnitude;
				if (sqrMagnitude <= cleaningDistanceThreshold && (num2 == -1 || sqrMagnitude < num))
				{
					num = sqrMagnitude;
					num2 = i;
				}
			}
			if (num2 != -1)
			{
				PoolableBehavior poolableBehavior = activeChildren[num2];
				((MonoBehaviour)poolableBehavior).StopAllCoroutines();
				CleanUp(((Component)poolableBehavior).GetComponent<VentDirt>());
			}
		}
		else if (button)
		{
			OpenVent();
		}
	}

	private IEnumerator CoOpenVent()
	{
		SoundManager.Instance.PlaySound(VentOpenSound, loop: false);
		yield return Effects.Lerp(0.2f, (float t) =>
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			ventLidClosed.transform.localScale = new Vector3(1f, Mathf.Lerp(1f, -0.5f, t), 1f);
			ventLidClosed.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, ventLidOpened.transform.localPosition.y, t), 3f);
		});
		ventLidClosed.SetActive(false);
		ventLidOpened.SetActive(true);
		ventOpen = true;
	}

	private void CleanUp(VentDirt ventDirt)
	{
		if (!ventOpen)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(CleanedSound, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		if (MyNormTask.IsComplete)
		{
			return;
		}
		((MonoBehaviour)this).StartCoroutine(ventDirt.CoDisappear());
		numberOfDirtsCleanedUp++;
		if (numberOfDirtsCleanedUp < numberOfDirts)
		{
			return;
		}
		MyNormTask.NextStep();
		((MonoBehaviour)this).StartCoroutine(CoStartClose());
		foreach (VentDirt activeChild in dirtPool.activeChildren)
		{
			if (!((Object)(object)activeChild == (Object)(object)ventDirt))
			{
				((MonoBehaviour)this).StartCoroutine(activeChild.CoDisappear());
			}
		}
	}

	private void SpawnDirt()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected Obj, but got Unknown
		VentDirt dirt = dirtPool.Get<VentDirt>();
		((Component)dirt).transform.localPosition = new Vector3(XSpan.Next(), YSpan.Next(), 4f);
		((Component)dirt).transform.localEulerAngles = new Vector3(0f, 0f, (float)Random.Range(0, 360));
		((UnityEvent)((Component)dirt).GetComponent<ButtonBehavior>().OnClick).AddListener((UnityAction)(() =>
		{
			CleanUp(dirt);
		}));
	}
}
