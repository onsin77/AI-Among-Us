using AmongUs.GameOptions;
using TMPro;
using UnityEngine;

public class ToggleOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public SpriteRenderer CheckMark;

	private bool oldValue;

	public void OnEnable()
	{
		((TMP_Text)TitleText).text = DestroyableSingleton<TranslationController>.Instance.GetString(Title);
		IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		switch (Title)
		{
		case StringNames.GameRecommendedSettings:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.IsDefaults);
			break;
		case StringNames.GameConfirmImpostor:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.ConfirmImpostor);
			break;
		case StringNames.GameVisualTasks:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.VisualTasks);
			break;
		case StringNames.GameAnonymousVotes:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.AnonymousVotes);
			break;
		case StringNames.UseFlashlight:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.UseFlashlight);
			break;
		case StringNames.SeekerFinalVents:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.SeekerFinalVents);
			break;
		case StringNames.SeekerFinalMap:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.SeekerFinalMap);
			break;
		case StringNames.SeekerPings:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.SeekerPings);
			break;
		case StringNames.ShowCrewmateNames:
			((Renderer)CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.ShowCrewmateNames);
			break;
		default:
			Debug.Log((object)("Ono, unrecognized setting: " + Title));
			break;
		}
	}

	private void FixedUpdate()
	{
		bool flag = GetBool();
		if (oldValue != flag)
		{
			oldValue = flag;
			((Renderer)CheckMark).enabled = flag;
		}
	}

	public void Toggle()
	{
		((Renderer)CheckMark).enabled = !((Renderer)CheckMark).enabled;
		OnValueChanged(this);
	}

	public override bool GetBool()
	{
		return ((Renderer)CheckMark).enabled;
	}
}
