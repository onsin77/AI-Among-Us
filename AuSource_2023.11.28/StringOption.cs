using AmongUs.GameOptions;
using TMPro;
using UnityEngine;

public class StringOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public TextMeshPro ValueText;

	public StringNames[] Values;

	public int Value;

	private int oldValue = -1;

	public void OnEnable()
	{
		IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		switch (Title)
		{
		case StringNames.GameKillDistance:
			currentGameOptions.TryGetInt(Int32OptionNames.KillDistance, out Value);
			break;
		case StringNames.GameTaskBarMode:
			currentGameOptions.TryGetInt(Int32OptionNames.TaskBarMode, out Value);
			break;
		default:
			Debug.Log((object)("Ono, unrecognized setting: " + Title));
			break;
		}
		((TMP_Text)TitleText).text = DestroyableSingleton<TranslationController>.Instance.GetString(Title);
		((TMP_Text)ValueText).text = DestroyableSingleton<TranslationController>.Instance.GetString(Values[Value]);
	}

	private void FixedUpdate()
	{
		if (oldValue != Value)
		{
			oldValue = Value;
			((TMP_Text)ValueText).text = DestroyableSingleton<TranslationController>.Instance.GetString(Values[Value]);
		}
	}

	public void Increase()
	{
		Value = Mathf.Clamp(Value + 1, 0, Values.Length - 1);
		OnValueChanged(this);
	}

	public void Decrease()
	{
		Value = Mathf.Clamp(Value - 1, 0, Values.Length - 1);
		OnValueChanged(this);
	}

	public override int GetInt()
	{
		return Value;
	}
}
