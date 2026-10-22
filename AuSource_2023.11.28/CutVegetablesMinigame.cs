using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class CutVegetablesMinigame : Minigame
{
	private const float KNIFE_CUT_SPEED = 0.1f;

	[SerializeField]
	private CuttableVegetable[] cuttableVeggies;

	[SerializeField]
	private Transform startArea;

	[SerializeField]
	private PassiveButton startAreaButton;

	[SerializeField]
	private PassiveButton cuttingBoardButton;

	[SerializeField]
	private Transform cuttingBoard;

	[SerializeField]
	private Transform endArea;

	[SerializeField]
	private GameObject arrow;

	[Header("Knife")]
	[SerializeField]
	private SpriteRenderer knife;

	[SerializeField]
	private Sprite cuttingKnifeSprite;

	[SerializeField]
	private float knifeCutYOffset = 1f;

	[Header("Audio")]
	[SerializeField]
	private AudioClip getSfx;

	[SerializeField]
	private AudioClip finishSfx;

	[SerializeField]
	private AudioClip[] cutSfx;

	[SerializeField]
	[Header("Glyphs")]
	private GameObject selectGlyph;

	[SerializeField]
	private GameObject chopGlyph;

	private Player player;

	private bool selecting;

	private bool chopping;

	private readonly List<CuttableVegetable> veggies = new List<CuttableVegetable>();

	private int numVeggiesCut;

	private CuttableVegetable CurrentVeggie => veggies[numVeggiesCut];

	public override void Begin(PlayerTask task)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected Obj, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected Obj, but got Unknown
		base.Begin(task);
		SetupInput(disableCursor: true);
		player = ReInput.players.GetPlayer(0);
		for (int i = 0; i < MyNormTask.Data.Length; i++)
		{
			byte b = MyNormTask.Data[i];
			CuttableVegetable cuttableVegetable = Object.Instantiate<CuttableVegetable>(cuttableVeggies[b], ((Component)this).transform, false);
			Vector3 localPosition = startArea.localPosition;
			localPosition.x += Random.Range(-0.1f, 0.1f);
			localPosition.y += Random.Range(-0.1f, 0.1f);
			localPosition.z += (float)(MyNormTask.Data.Length - i) * -1f;
			((Component)cuttableVegetable).transform.localPosition = localPosition;
			veggies.Add(cuttableVegetable);
		}
		SetSelectState();
		((UnityEvent)startAreaButton.OnClick).AddListener((UnityAction)SelectVegetable);
		((UnityEvent)cuttingBoardButton.OnClick).AddListener((UnityAction)ChopVegetable);
	}

	private void Update()
	{
		if (selecting && player.GetButtonDown(11))
		{
			SelectVegetable();
		}
		if (chopping && player.GetButtonDown(11))
		{
			ChopVegetable();
		}
	}

	private void SelectVegetable()
	{
		if (selecting)
		{
			((MonoBehaviour)this).StartCoroutine(CoMoveNextVegetableToCuttingBoard());
		}
	}

	private void ChopVegetable()
	{
		if (chopping)
		{
			((MonoBehaviour)this).StartCoroutine(CoCutVeggie());
		}
	}

	private void SetSelectState()
	{
		selecting = true;
		chopping = false;
		startAreaButton.SetButtonEnableState(enabled: true);
		cuttingBoardButton.SetButtonEnableState(enabled: false);
		selectGlyph.SetActive(true);
		chopGlyph.SetActive(false);
	}

	private void SetChopState()
	{
		selecting = false;
		chopping = true;
		startAreaButton.SetButtonEnableState(enabled: false);
		cuttingBoardButton.SetButtonEnableState(enabled: true);
		selectGlyph.SetActive(false);
		chopGlyph.SetActive(true);
	}

	private void SetCompletedState()
	{
		selecting = false;
		chopping = false;
		startAreaButton.SetButtonEnableState(enabled: false);
		cuttingBoardButton.SetButtonEnableState(enabled: false);
		selectGlyph.SetActive(false);
		chopGlyph.SetActive(false);
	}

	private IEnumerator CoMoveNextVegetableToCuttingBoard()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(getSfx, loop: false);
		}
		arrow.SetActive(false);
		selecting = false;
		selectGlyph.SetActive(false);
		yield return Effects.Slide2D(((Component)CurrentVeggie).transform, Vector2.op_Implicit(((Component)CurrentVeggie).transform.localPosition), Vector2.op_Implicit(cuttingBoard.localPosition));
		SetChopState();
	}

	private IEnumerator CoCutVeggie()
	{
		chopping = false;
		knife.sprite = cuttingKnifeSprite;
		cuttingBoardButton.SetButtonEnableState(enabled: false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(cutSfx.Random(), loop: false);
		}
		Vector3 localPosition = ((Component)knife).transform.localPosition;
		Vector3 dest = Vector2.op_Implicit(Vector2.op_Implicit(((Component)CurrentVeggie.NextPiece).transform.localPosition) - new Vector2(CurrentVeggie.NextPiece.size.x / 2f, knifeCutYOffset));
		dest.z = localPosition.z;
		yield return Effects.Slide3D(((Component)knife).transform, localPosition, dest, 0.1f);
		yield return CurrentVeggie.CoCutNextPiece();
		if (CurrentVeggie.IsFullyCut)
		{
			Vector3 localPosition2 = cuttingBoard.localPosition;
			localPosition2.z = (float)(1 + numVeggiesCut) * -1f;
			Vector3 localPosition3 = endArea.localPosition;
			localPosition3.x += Random.Range(-0.1f, 0.1f);
			localPosition3.y += Random.Range(-0.1f, 0.1f);
			localPosition3.z += localPosition2.z;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(finishSfx, loop: false);
			}
			yield return Effects.Slide3D(((Component)CurrentVeggie).transform, localPosition2, localPosition3);
			numVeggiesCut++;
			if (numVeggiesCut >= veggies.Count)
			{
				SetCompletedState();
				MyNormTask.NextStep();
				yield return Effects.Wait(0.5f);
				((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
			}
			else
			{
				arrow.SetActive(true);
				SetSelectState();
			}
		}
		else
		{
			SetChopState();
		}
	}
}
