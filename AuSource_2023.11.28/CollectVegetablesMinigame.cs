using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectVegetablesMinigame : Minigame
{
	public enum Vegetable
	{
		Carrot,
		Cauliflower,
		Chard,
		Curls
	}

	[Header("Design")]
	[SerializeField]
	private float navigationDelay = 0.2f;

	[SerializeField]
	[Header("Components")]
	private CollectableVegetable[] collectables;

	[SerializeField]
	private AudioClip selectionChangedSfx;

	private int selection;

	private int collected;

	private float lastNavigationTime;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: true);
		MyNormTask.Data = new byte[collectables.Length];
		List<Vegetable> list = new List<Vegetable>(Enum.GetValues(typeof(Vegetable)).Cast<Vegetable>());
		for (int i = 0; i < collectables.Length; i++)
		{
			CollectableVegetable obj = collectables[i];
			int index = list.RandomIdx();
			obj.Initialize(list[index], i);
			obj.OnCollectedEvent += OnVeggieCollected;
			obj.OnNavigationEvent += OnVeggieNavigation;
			obj.OnTouchedEvent += OnVeggieTouched;
			list.RemoveAt(index);
		}
		SetSelection(0);
		collected = 0;
	}

	private void OnVeggieTouched(CollectableVegetable vegetable)
	{
		SetSelection(vegetable.Index);
	}

	private void OnVeggieNavigation(CollectableVegetable vegetable)
	{
		if (vegetable.Index != selection && Time.time > lastNavigationTime + navigationDelay)
		{
			SoundManager.Instance.PlaySoundImmediate(selectionChangedSfx, loop: false);
			lastNavigationTime = Time.time;
			SetSelection(vegetable.Index);
		}
	}

	private void OnVeggieCollected(CollectableVegetable vegetable)
	{
		((MonoBehaviour)this).StartCoroutine(CoCheckAllVeggiesCollected(vegetable.Index));
	}

	private void SetSelection(int index)
	{
		CollectableVegetable[] array = collectables;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetSelection(isSelected: false);
		}
		selection = index;
		collectables[selection].SetSelection(isSelected: true);
	}

	private IEnumerator CoCheckAllVeggiesCollected(int veggieIndex)
	{
		MyNormTask.Data[collected] = (byte)veggieIndex;
		collected++;
		if (collected >= collectables.Length)
		{
			MyNormTask.NextStep();
			yield return Effects.Wait(0.5f);
			((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
		}
		yield return null;
	}
}
