using System.Collections.Generic;
using AmongUs.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlayScene : MonoBehaviour
{
	public enum HowToPlayCategory
	{
		None = 0,
		GameMode_Classic = 100,
		GameMode_HideAndSeek = 101,
		Roles = 200
	}

	public static string SceneOnClose;

	[Header("UI")]
	[SerializeField]
	private GameObject startPage;

	[Space(10f)]
	[SerializeField]
	private GameObject arrows;

	[SerializeField]
	private SpriteRenderer leftButton;

	[SerializeField]
	private SpriteRenderer rightButton;

	[Space(10f)]
	[SerializeField]
	private UiElement closeButton;

	[Space(10f)]
	[SerializeField]
	private Transform dotParent;

	[SerializeField]
	private Transform dotTemplate;

	[Header("Game Mode Scenes")]
	[SerializeField]
	private AbstractHowToPlayScenePage[] classicScenes;

	[SerializeField]
	private AbstractHowToPlayScenePage[] hideAndSeekScenes;

	[Space(10f)]
	[SerializeField]
	private ConditionalHowToPlayScenePage pcMove;

	[Header("Misc Scenes")]
	[SerializeField]
	private AbstractHowToPlayScenePage[] rolesScenes;

	[Header("Console Controller Navigation")]
	[SerializeField]
	private UiElement defaultButtonSelected;

	[SerializeField]
	private List<UiElement> controllerSelectables;

	private int sceneIndex;

	private Transform[] dots;

	private HowToPlayCategory category;

	public void Start()
	{
		category = HowToPlayCategory.None;
		arrows.SetActive(false);
		startPage.SetActive(true);
		ControllerManager.Instance.NewScene(((Object)this).name, closeButton, defaultButtonSelected, controllerSelectables);
		SetupDots(0);
		if (!DataManager.Player.Onboarding.ViewedHideAndSeekHowToPlay)
		{
			StartHideAndSeek();
		}
	}

	public void StartClassic()
	{
		sceneIndex = 0;
		category = HowToPlayCategory.GameMode_Classic;
		startPage.SetActive(false);
		arrows.SetActive(true);
		classicScenes[2] = pcMove;
		DisableAllScenes();
		SetupDots(11);
		ChangeScene(0);
		ControllerManager.Instance.SetDefaultSelection(defaultButtonSelected);
	}

	public void StartHideAndSeek()
	{
		sceneIndex = 0;
		category = HowToPlayCategory.GameMode_HideAndSeek;
		startPage.SetActive(false);
		arrows.SetActive(true);
		DisableAllScenes();
		SetupDots(5);
		ChangeScene(0);
		ControllerManager.Instance.SetDefaultSelection(defaultButtonSelected);
	}

	public void StartRoles()
	{
		sceneIndex = 0;
		category = HowToPlayCategory.Roles;
		startPage.SetActive(false);
		arrows.SetActive(true);
		DisableAllScenes();
		SetupDots(rolesScenes.Length);
		ChangeScene(0);
		ControllerManager.Instance.SetDefaultSelection(defaultButtonSelected);
	}

	private void Reset()
	{
		DisableAllScenes();
		category = HowToPlayCategory.None;
		startPage.SetActive(true);
		arrows.SetActive(false);
		sceneIndex = 0;
		SetupDots(0);
	}

	public void Update()
	{
		if (Input.GetKeyUp((KeyCode)27))
		{
			Close();
		}
	}

	private void DisableAllScenes()
	{
		classicScenes.ForEach((AbstractHowToPlayScenePage x) =>
		{
			((Component)x).gameObject.SetActive(false);
		});
		hideAndSeekScenes.ForEach((AbstractHowToPlayScenePage x) =>
		{
			((Component)x).gameObject.SetActive(false);
		});
		rolesScenes.ForEach((AbstractHowToPlayScenePage x) =>
		{
			((Component)x).gameObject.SetActive(false);
		});
	}

	public void NextScene()
	{
		ChangeScene(1);
	}

	public void PreviousScene()
	{
		ChangeScene(-1);
	}

	public void Close()
	{
		if (CanClose())
		{
			if (SceneOnClose != null)
			{
				ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
				string sceneOnClose = SceneOnClose;
				SceneOnClose = null;
				SceneManager.LoadScene(sceneOnClose);
			}
			else if (category == HowToPlayCategory.None)
			{
				ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
				SceneManager.LoadScene("MainMenu");
			}
			else
			{
				Reset();
			}
		}
	}

	private void ChangeScene(int del)
	{
		switch (category)
		{
		default:
			Debug.LogWarning((object)string.Format("{0}.{1} has not been configured.", "HowToPlayCategory", category));
			break;
		case HowToPlayCategory.None:
			Debug.LogWarning((object)"A HowToPlayCategory was not set. Aborting.");
			break;
		case HowToPlayCategory.GameMode_Classic:
			((Component)classicScenes[sceneIndex]).gameObject.SetActive(false);
			sceneIndex = Mathf.Clamp(sceneIndex + del, 0, classicScenes.Length - 1);
			((Component)classicScenes[sceneIndex]).gameObject.SetActive(true);
			((Component)rightButton).gameObject.SetActive(sceneIndex < classicScenes.Length - 1);
			break;
		case HowToPlayCategory.GameMode_HideAndSeek:
			((Component)hideAndSeekScenes[sceneIndex]).gameObject.SetActive(false);
			sceneIndex = Mathf.Clamp(sceneIndex + del, 0, hideAndSeekScenes.Length - 1);
			((Component)hideAndSeekScenes[sceneIndex]).gameObject.SetActive(true);
			((Component)rightButton).gameObject.SetActive(sceneIndex < hideAndSeekScenes.Length - 1);
			if (!DataManager.Player.Onboarding.ViewedHideAndSeekHowToPlay && sceneIndex == hideAndSeekScenes.Length - 1)
			{
				DataManager.Player.Onboarding.ViewedHideAndSeekHowToPlay = true;
			}
			break;
		case HowToPlayCategory.Roles:
			((Component)rolesScenes[sceneIndex]).gameObject.SetActive(false);
			sceneIndex = Mathf.Clamp(sceneIndex + del, 0, rolesScenes.Length - 1);
			((Component)rolesScenes[sceneIndex]).gameObject.SetActive(true);
			((Component)rightButton).gameObject.SetActive(sceneIndex < rolesScenes.Length - 1);
			break;
		}
		((Component)closeButton).gameObject.SetActive(CanClose());
		SetActiveDot(sceneIndex);
		((Component)leftButton).gameObject.SetActive(sceneIndex > 0);
	}

	private bool CanClose()
	{
		if (category == HowToPlayCategory.GameMode_HideAndSeek && !DataManager.Player.Onboarding.ViewedHideAndSeekHowToPlay)
		{
			return false;
		}
		return true;
	}

	private void SetupDots(int count)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (dots != null)
		{
			Transform[] array = dots;
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy((Object)(object)((Component)array[i]).gameObject);
			}
		}
		dots = new Transform[count];
		float num = ((count > 1) ? (count + 1) : 0);
		Vector3 val = new Vector3((0f - num) / 2f, 0f, 0f);
		Vector3 val2 = new Vector3(num / 2f, 0f, 0f);
		((Component)dotTemplate).gameObject.SetActive(true);
		for (int j = 0; j < count; j++)
		{
			float num2 = (float)j / ((float)count - 1f);
			dots[j] = Object.Instantiate<Transform>(dotTemplate, dotParent);
			dots[j].localPosition = Vector3.Lerp(val, val2, num2);
			((Object)dots[j]).name = $"Dot [{j:00}]";
		}
		((Component)dotTemplate).gameObject.SetActive(false);
		SetActiveDot(sceneIndex);
	}

	private void SetActiveDot(int index)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Transform[] array = dots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].localScale = Vector3.one;
		}
		if (index < dots.Length)
		{
			dots[index].localScale = new Vector3(1.5f, 1.5f, 1.5f);
		}
	}
}
