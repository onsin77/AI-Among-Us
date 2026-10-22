using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class MineOreMinigame : Minigame
{
	private const string BAR_FILL_SFX = "mineOrePowerBarLoop";

	private const int ORE_NUMBER = 3;

	private const int MAX_GOOD_ROCKS = 2;

	private const int MAX_PERFECT_ROCKS = 4;

	[SerializeField]
	[Tooltip("Speed of power meter when breaking rocks")]
	private float powerBarSpeed = 1f;

	[Tooltip("angle of pickaxe while animating breaking a rock")]
	[SerializeField]
	private float pikeaxeFirstAngle = -90f;

	[Tooltip("angle of pickaxe while animating breaking a rock")]
	[SerializeField]
	private float pikeaxeSecondAngle = 50f;

	[Tooltip("time of pickeaxe animation in seconds")]
	[SerializeField]
	private float pikeaxeAnimTime = 0.3f;

	[Tooltip("Good Zone percentage max value")]
	[SerializeField]
	private float goodPercentage = 0.5f;

	[Tooltip("Great Zone percentage max value")]
	[SerializeField]
	private float greatPercentage = 0.8f;

	[SerializeField]
	private Collider2D pickaxe;

	[SerializeField]
	private GameObject pickaxeSprite;

	[SerializeField]
	private GameObject pickControllerHelper;

	[SerializeField]
	private PowerBarMining powerBarMining;

	[SerializeField]
	private List<CollectableOre> ores;

	[Header("Audio Files")]
	[SerializeField]
	private AudioClip[] rockHitWeak;

	[SerializeField]
	private AudioClip[] rockHitStrong;

	[SerializeField]
	private AudioClip[] rockRelease;

	[SerializeField]
	private AudioClip pickaxeSwing;

	[SerializeField]
	private AudioClip powerBarLoop;

	private AudioSource powerBarAudioSource;

	private int currentOreCount;

	private CollectableOre currentOre;

	private Controller controller = new Controller();

	private bool isMining;

	private Player gamepad;

	private void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if (isMining || ((Behaviour)powerBarMining).isActiveAndEnabled)
		{
			if ((Object)(object)VirtualCursor.instance != (Object)null)
			{
				VirtualCursor.instance.SetWorldPosition(Vector2.op_Implicit(((Component)pickaxe).transform.position));
			}
			return;
		}
		controller.Update();
		if (!IsUsingController())
		{
			return;
		}
		((Component)pickaxe).transform.position = Vector2.op_Implicit(VirtualCursor.currentPosition);
		((Component)pickaxe).transform.SetLocalZ(-0.5f);
		if (!gamepad.GetButtonDown(21))
		{
			return;
		}
		List<Collider2D> list = new List<Collider2D>();
		ContactFilter2D val = default;
		if (Physics2D.OverlapCircle(VirtualCursor.currentPosition, 0.2f, val.NoFilter(), list) <= 0)
		{
			return;
		}
		foreach (Collider2D item in list)
		{
			BreakableRock component = ((Component)item).GetComponent<BreakableRock>();
			if ((Object)(object)component != (Object)null && component.CanInteract)
			{
				CallPowerMeter(component.GetOre(), Vector2.op_Implicit(VirtualCursor.currentPosition));
				break;
			}
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: false);
		((Component)pickaxe).gameObject.SetActive(IsUsingController());
		pickControllerHelper.SetActive(IsUsingController());
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputChanged));
		gamepad = ReInput.players.GetPlayer(0);
		currentOreCount = 0;
		SpawnRocksAndOres();
		powerBarAudioSource = SoundManager.Instance.GetNamedSfxSource("mineOrePowerBarLoop");
		powerBarAudioSource.clip = powerBarLoop;
		powerBarAudioSource.loop = true;
		powerBarAudioSource.pitch = 0f;
		powerBarAudioSource.volume = 0f;
		foreach (CollectableOre ore in ores)
		{
			ore.Initialize(OreCollected, CallPowerMeter, controller, RockReleaseAudio);
		}
		SetRockOreInput(isEnabled: true);
	}

	public override void Close()
	{
		MyNormTask.Data = new byte[ores.Count * 2];
		for (int i = 0; i < ores.Count; i++)
		{
			MyNormTask.Data[i] = (byte)ores[i].GetNumberRocksBroken();
		}
		for (int j = ores.Count; j < ores.Count * 2; j++)
		{
			if (!ores[j - ores.Count].GetIsRock())
			{
				MyNormTask.Data[j] = 0;
			}
			else
			{
				MyNormTask.Data[j] = 1;
			}
		}
		powerBarAudioSource.Stop();
		base.Close();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputChanged));
	}

	private void OnInputChanged()
	{
		if (!isMining)
		{
			((Component)pickaxe).gameObject.SetActive(IsUsingController());
			pickControllerHelper.SetActive(IsUsingController());
		}
	}

	private void SpawnRocksAndOres()
	{
		List<CollectableOre> list = new List<CollectableOre>(ores);
		if (MyNormTask.Data != null && MyNormTask.Data.Length == ores.Count * 2)
		{
			int num = 0;
			for (int i = 0; i < ores.Count; i++)
			{
				int num2 = MyNormTask.Data[i];
				ores[i].SetNumRocks(num2);
				bool flag = MyNormTask.Data[i + ores.Count] == 0;
				if (num2 >= 4)
				{
					if (flag)
					{
						currentOreCount++;
					}
					else
					{
						ores[i].SetAsRock();
					}
					((Component)ores[i]).gameObject.SetActive(false);
					continue;
				}
				ores[i].RotateObjects();
				if (!flag)
				{
					ores[i].SetAsRock();
					continue;
				}
				ores[i].SetOreSprite(num);
				num++;
				num = Mathf.Clamp(num, 0, 2);
			}
		}
		else
		{
			for (int j = 0; j < 3; j++)
			{
				int index = list.RandomIdx();
				CollectableOre collectableOre = list[index];
				collectableOre.SetNumRocks(0);
				collectableOre.RotateObjects();
				collectableOre.SetOreSprite(j);
				list.Remove(collectableOre);
			}
			for (int k = 0; k < list.Count; k++)
			{
				CollectableOre collectableOre2 = list[k];
				collectableOre2.SetNumRocks(0);
				collectableOre2.SetAsRock();
				collectableOre2.RotateObjects();
				((Component)collectableOre2).transform.SetParent(((Component)this).transform);
			}
		}
	}

	private void OreCollected()
	{
		currentOreCount++;
		if (currentOreCount >= 3)
		{
			MyNormTask.NextStep();
			if (MyNormTask.IsComplete)
			{
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
		}
	}

	private void CallPowerMeter(CollectableOre ore, Vector3 clickPos)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (Constants.ShouldPlaySfx())
		{
			powerBarAudioSource.volume = 1f;
			powerBarAudioSource.Play();
		}
		currentOre = ore;
		SetRockOreInput(isEnabled: false);
		((Component)powerBarMining).gameObject.SetActive(true);
		powerBarMining.Initialize(powerBarSpeed, HitRock, clickPos, UpdateAudioPitch);
		((Component)pickaxe).gameObject.SetActive(true);
		((Component)pickaxe).transform.position = new Vector3(clickPos.x, clickPos.y, ((Component)pickaxe).transform.position.z);
		((MonoBehaviour)this).StartCoroutine(Effects.Rotate2D(pickaxeSprite.transform, 0f, pikeaxeFirstAngle, pikeaxeAnimTime));
	}

	private void UpdateAudioPitch(float power)
	{
		if (Constants.ShouldPlaySfx())
		{
			powerBarAudioSource.pitch = 1f + power;
		}
	}

	private void HitRock(float power, Vector3 clickPos)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		AudioClip clipToPlay;
		List<BreakableRock> rocksToBreak;
		if (power <= goodPercentage)
		{
			clipToPlay = rockHitWeak.Random();
			rocksToBreak = GetRocks(clickPos, 1);
		}
		else if (power <= greatPercentage)
		{
			clipToPlay = rockHitStrong.Random();
			rocksToBreak = GetRocks(clickPos, 2);
		}
		else
		{
			clipToPlay = rockHitStrong.Random();
			rocksToBreak = GetRocks(clickPos, 4);
		}
		isMining = true;
		VibrationManager.Vibrate(power * 2f, power * 2f, 0.1f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(pickaxeSwing, loop: false);
		}
		powerBarAudioSource.Stop();
		((MonoBehaviour)this).StartCoroutine(Effects.Sequence(Effects.Rotate2D(pickaxeSprite.transform, pikeaxeFirstAngle, pikeaxeSecondAngle, pikeaxeAnimTime), Effects.Action(() =>
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			((Component)powerBarMining).gameObject.SetActive(false);
			if (!IsUsingController())
			{
				((Component)pickaxe).gameObject.SetActive(false);
			}
			else if ((Object)(object)VirtualCursor.instance != (Object)null)
			{
				VirtualCursor.instance.SetWorldPosition(Vector2.op_Implicit(((Component)pickaxe).transform.position));
			}
			foreach (BreakableRock item in rocksToBreak)
			{
				item.SendPower();
				if (Object.op_Implicit((Object)(object)currentOre))
				{
					currentOre.BrokenRock();
				}
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(clipToPlay, loop: false);
			}
			currentOre = null;
			((MonoBehaviour)this).StartCoroutine(CoWaitBreakAnimations());
		})));
	}

	private IEnumerator CoWaitBreakAnimations()
	{
		yield return Effects.Wait(0.5f);
		isMining = false;
		SetRockOreInput(isEnabled: true);
	}

	private List<BreakableRock> GetRocks(Vector3 startPoint, int numRocks)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		List<BreakableRock> list = new List<BreakableRock>();
		BreakableRock[] rocks = currentOre.GetRocks();
		foreach (BreakableRock breakableRock in rocks)
		{
			if (((Component)breakableRock).gameObject.activeInHierarchy)
			{
				list.Add(breakableRock);
			}
		}
		if (numRocks >= list.Count)
		{
			return new List<BreakableRock>(list);
		}
		List<BreakableRock> list2 = new List<BreakableRock>();
		BreakableRock closestRock = GetClosestRock(startPoint, list);
		if (Object.op_Implicit((Object)(object)closestRock))
		{
			list2.Add(closestRock);
			list.Remove(closestRock);
		}
		if (numRocks == 1 || list.Count <= 0)
		{
			return list2;
		}
		closestRock = GetClosestRock(startPoint, list);
		if (Object.op_Implicit((Object)(object)closestRock))
		{
			list2.Add(closestRock);
		}
		return list2;
	}

	private BreakableRock GetClosestRock(Vector3 startPoint, List<BreakableRock> potentialRocks)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		float num = 10000f;
		BreakableRock result = null;
		foreach (BreakableRock potentialRock in potentialRocks)
		{
			float num2 = Vector3.Distance(((Component)potentialRock).transform.position, startPoint);
			if (num2 < num)
			{
				result = potentialRock;
				num = num2;
			}
		}
		return result;
	}

	private bool IsUsingController()
	{
		return ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
	}

	private void SetRockOreInput(bool isEnabled)
	{
		foreach (CollectableOre ore in ores)
		{
			BreakableRock[] rocks = ore.GetRocks();
			for (int i = 0; i < rocks.Length; i++)
			{
				rocks[i].SetInput(isEnabled);
			}
		}
	}

	private void RockReleaseAudio()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(rockRelease.Random(), loop: false);
		}
	}
}
