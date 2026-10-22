using System.Collections.Generic;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;

public class KeyValueOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public TextMeshPro ValueText;

	public List<KeyValuePair<string, int>> Values;

	private int Selected;

	private int oldValue = -1;

	public void OnEnable()
	{
		IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		if (Title == StringNames.GameMapName)
		{
			Selected = ((currentGameOptions.MapId > 3) ? (currentGameOptions.MapId - 1) : currentGameOptions.MapId);
		}
		else
		{
			Debug.Log((object)("Ono, unrecognized setting: " + Title));
		}
		((TMP_Text)TitleText).text = DestroyableSingleton<TranslationController>.Instance.GetString(Title);
	}

	private void FixedUpdate()
	{
		if (oldValue != Selected)
		{
			oldValue = Selected;
			((TMP_Text)ValueText).text = Values[Selected].Key;
		}
	}

	public void Increase()
	{
		Selected = Mathf.Clamp(Selected + 1, 0, Values.Count - 1);
		OnValueChanged(this);
	}

	public void Decrease()
	{
		Selected = Mathf.Clamp(Selected - 1, 0, Values.Count - 1);
		OnValueChanged(this);
	}

	public override int GetInt()
	{
		return Values[Selected].Value;
	}
}
