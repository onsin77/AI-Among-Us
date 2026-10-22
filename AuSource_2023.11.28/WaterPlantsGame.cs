using System.Collections;
using System.Linq;
using Rewired;
using TMPro;
using UnityEngine;

public class WaterPlantsGame : Minigame
{
	public GameObject stage1;

	public GameObject stage2;

	public AudioClip CanGrabSound;

	public PassiveButton WaterCan;

	public SpriteRenderer[] Plants;

	public AudioClip WaterPlantSound;

	public AudioClip[] PlantGrowSounds;

	public AudioClip[] PlantFinishedSounds;

	public TextMeshPro FloatText;

	public Transform[] Locations;

	public Transform selectorObject;

	public GameObject grabCanSubObject;

	public GameObject holdingCanSubObject;

	public GameObject waterPlantsSubObject;

	private Controller c = new Controller();

	public SpriteRenderer[] playerHandObjects;

	public ParticleSystem waterParticles;

	public GameObject greenDot;

	private bool Watered(int x)
	{
		return MyNormTask.Data[x] != 0;
	}

	private void Watered(int x, bool b)
	{
		MyNormTask.Data[x] = (byte)(b ? 1u : 0u);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		if (MyNormTask.taskStep == 0)
		{
			((Component)WaterCan).transform.localPosition = Locations.Random().localPosition;
			((Component)WaterCan).GetComponent<SpriteRenderer>().flipX = BoolRange.Next();
			stage1.gameObject.SetActive(true);
			stage2.gameObject.SetActive(false);
			SetupInput(disableCursor: false);
			grabCanSubObject.SetActive(true);
			holdingCanSubObject.SetActive(false);
			waterPlantsSubObject.SetActive(false);
			SpriteRenderer[] array = playerHandObjects;
			foreach (SpriteRenderer playerMaterialColors in array)
			{
				PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
			}
			greenDot.SetActive(false);
		}
		else
		{
			if (MyNormTask.taskStep != 1)
			{
				return;
			}
			stage1.gameObject.SetActive(false);
			stage2.gameObject.SetActive(true);
			for (int j = 0; j < Plants.Length; j++)
			{
				if (Watered(j))
				{
					SpriteRenderer obj = Plants[j];
					((Renderer)obj).material.SetFloat("_Desat", 0f);
					((Component)obj).transform.localScale = Vector3.one;
				}
			}
			SetupInput(disableCursor: false);
			grabCanSubObject.SetActive(false);
			holdingCanSubObject.SetActive(false);
			waterPlantsSubObject.SetActive(true);
			SpriteRenderer[] array = playerHandObjects;
			foreach (SpriteRenderer playerMaterialColors2 in array)
			{
				PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors2);
			}
			greenDot.SetActive(true);
		}
	}

	private void Update()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		c.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			ReInput.players.GetPlayer(0).GetButton(11);
			Vector3 position = ((Component)selectorObject).transform.position;
			position.x = VirtualCursor.currentPosition.x;
			position.y = VirtualCursor.currentPosition.y;
			((Component)selectorObject).transform.position = position;
		}
	}

	public void PickWaterCan()
	{
		grabCanSubObject.SetActive(false);
		holdingCanSubObject.SetActive(true);
		((Behaviour)WaterCan).enabled = false;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(CanGrabSound, loop: false);
		}
		MyNormTask.NextStep();
		((MonoBehaviour)this).StartCoroutine(CoPickWaterCan());
	}

	private IEnumerator CoPickWaterCan()
	{
		((TMP_Text)FloatText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WaterPlantsGetCan);
		((Component)FloatText).gameObject.SetActive(true);
		yield return Effects.All(Effects.ColorFade(((Component)WaterCan).GetComponent<SpriteRenderer>(), Color.white, Palette.ClearWhite, 0.25f), Effects.Slide2D(FloatText.transform, Vector2.op_Implicit(((Component)WaterCan).transform.localPosition + new Vector3(0f, 0.1f, 0f)), Vector2.op_Implicit(((Component)WaterCan).transform.localPosition + new Vector3(0f, 0.5f, 0f))), Effects.ColorFade(FloatText, Color.white, Palette.ClearWhite, 0.75f));
		yield return CoStartClose();
	}

	public void WaterPlant(int num)
	{
		if (!Watered(num))
		{
			Watered(num, b: true);
			if (Enumerable.Range(0, 4).All(Watered))
			{
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(WaterPlantSound, loop: false);
			}
			((MonoBehaviour)this).StartCoroutine(CoGrowPlant(num));
			if (Controller.currentTouchType == Controller.TouchType.Joystick)
			{
				waterParticles.Play();
			}
		}
	}

	private IEnumerator CoGrowPlant(int num)
	{
		SpriteRenderer plant = Plants[num];
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(PlantGrowSounds.Random(), loop: false).pitch = FloatRange.Next(0.9f, 1.1f);
		}
		for (float timer = 0f; timer < 1f; timer += Time.deltaTime)
		{
			float num2 = timer / 1f;
			((Renderer)plant).material.SetFloat("_Desat", (1f - num2) * 0.8f);
			((Component)plant).transform.localScale = new Vector3(0.8f, Mathf.Lerp(0.8f, 1.1f, num2), 1f);
			yield return null;
		}
		((Renderer)plant).material.SetFloat("_Desat", 0f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(PlantFinishedSounds.Random(), loop: false).pitch = FloatRange.Next(0.9f, 1.1f);
		}
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num3 = timer / 0.1f;
			((Component)plant).transform.localScale = new Vector3(Mathf.Lerp(0.8f, 1.1f, num3), Mathf.Lerp(1.1f, 0.95f, num3), 1f);
			yield return null;
		}
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num4 = timer / 0.1f;
			((Component)plant).transform.localScale = new Vector3(Mathf.Lerp(1.1f, 1f, num4), Mathf.Lerp(0.95f, 1f, num4), 1f);
			yield return null;
		}
	}
}
