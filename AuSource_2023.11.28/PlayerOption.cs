using System.Collections.Generic;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public TextMeshPro ValueText;

	[SerializeField]
	private TextMeshPro PlusTxt;

	[SerializeField]
	private TextMeshPro MinusTxt;

	[SerializeField]
	private TextMeshPro OptionUnavailableTxt;

	private List<GameData.PlayerInfo> Values;

	public int Value = -1;

	private int oldValue = -1;

	private int playerIndex = -1;

	private PassiveButton PlusBtn;

	private PassiveButton MinusBtn;

	public void OnEnable()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Values = new List<GameData.PlayerInfo>(GameData.Instance.AllPlayers);
		Values.Sort((GameData.PlayerInfo a, GameData.PlayerInfo b) => a.PlayerId.CompareTo(b.PlayerId));
		Value = GameOptionsManager.Instance.GameHostOptions.GetInt(Int32OptionNames.ImpostorPlayerID);
		playerIndex = Values.FindIndex((GameData.PlayerInfo p) => p.PlayerId == Value);
		((TMP_Text)TitleText).text = DestroyableSingleton<TranslationController>.Instance.GetString(Title);
		((Graphic)OptionUnavailableTxt).color = Palette.DisabledGrey;
		SetValueText();
	}

	private void Awake()
	{
		PlusBtn = ((Component)PlusTxt).gameObject.GetComponent<PassiveButton>();
		MinusBtn = ((Component)MinusTxt).gameObject.GetComponent<PassiveButton>();
	}

	private void SetValueText()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		bool isGamePublic = AmongUsClient.Instance.IsGamePublic;
		((Component)ValueText).gameObject.SetActive(!isGamePublic);
		if (playerIndex < 0)
		{
			((TMP_Text)ValueText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoundRobin);
		}
		else
		{
			((TMP_Text)ValueText).text = Values[playerIndex].PlayerName;
		}
		((Graphic)ValueText).color = (isGamePublic ? Palette.DisabledGrey : Color.white);
		((Graphic)TitleText).color = (isGamePublic ? Palette.DisabledGrey : Color.white);
		((Component)PlusTxt).gameObject.SetActive(!isGamePublic);
		((Component)MinusTxt).gameObject.SetActive(!isGamePublic);
		((Behaviour)PlusBtn).enabled = !isGamePublic;
		((Behaviour)MinusBtn).enabled = !isGamePublic;
		((Component)OptionUnavailableTxt).gameObject.SetActive(isGamePublic);
	}

	private void FixedUpdate()
	{
		if (GameData.Instance.AllPlayers.Count != Values.Count)
		{
			Values = new List<GameData.PlayerInfo>(GameData.Instance.AllPlayers);
			Values.Sort((GameData.PlayerInfo a, GameData.PlayerInfo b) => a.PlayerId.CompareTo(b.PlayerId));
			playerIndex = Values.FindIndex((GameData.PlayerInfo p) => p.PlayerId == Value);
			SetValueText();
		}
		CheckValueChanged();
	}

	public void Increase()
	{
		UpdatePlayerIndex(Mathf.Clamp(playerIndex + 1, -1, Values.Count - 1));
		CheckValueChanged();
		OnValueChanged(this);
	}

	public void Decrease()
	{
		UpdatePlayerIndex(Mathf.Clamp(playerIndex - 1, -1, Values.Count - 1));
		CheckValueChanged();
		OnValueChanged(this);
	}

	public override int GetInt()
	{
		if (AmongUsClient.Instance.IsGamePublic)
		{
			return -1;
		}
		return Value;
	}

	private void UpdatePlayerIndex(int index)
	{
		if (index >= 0 && index < Values.Count)
		{
			playerIndex = index;
			Value = Values[index].PlayerId;
		}
		else
		{
			playerIndex = -1;
			Value = -1;
		}
	}

	private void CheckValueChanged()
	{
		if (oldValue != Value)
		{
			oldValue = Value;
			SetValueText();
		}
	}
}
