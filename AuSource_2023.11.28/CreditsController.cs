using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
	public enum CreditType
	{
		TEXT,
		IMAGE
	}

	[Serializable]
	public struct FormatStruct
	{
		public string format;

		public CreditType creditType;

		public List<GameObject> formatPrefabs;
	}

	[Serializable]
	public struct CreditStruct
	{
		public string format;

		public string[] columns;
	}

	public TextAsset CSVCredits;

	public int NumberOfContentColumns = 3;

	public GameObject creditPanelPrefab;

	public GameObject creditMainPanel;

	public float creditScrollSpeed;

	public float initialDelay = 2f;

	public float startPos = 230f;

	private float remainingDelay;

	private RectTransform creditsRect;

	private bool paused;

	public List<FormatStruct> formats;

	public List<CreditStruct> credits;

	public event Action OnFinish;

	private void Awake()
	{
		creditsRect = creditMainPanel.GetComponent<RectTransform>();
		LoadCredits();
	}

	private void Start()
	{
		remainingDelay = initialDelay;
		Debug.Log((object)("First Credit: " + credits[0].columns[0]));
		_ = ((Component)this).gameObject;
		for (int i = 0; i < credits.Count; i++)
		{
			FormatStruct format = GetFormat(credits[i].format);
			GameObject val = Object.Instantiate<GameObject>(creditPanelPrefab, creditMainPanel.transform);
			for (int j = 0; j < format.formatPrefabs.Count; j++)
			{
				GameObject val2 = Object.Instantiate<GameObject>(format.formatPrefabs[j], val.transform);
				if (format.creditType == CreditType.TEXT)
				{
					string text = credits[i].columns[j].Trim();
					if (text.IsNullOrWhiteSpace())
					{
						((TMP_Text)val2.GetComponent<TextMeshProUGUI>()).text = string.Empty;
						continue;
					}
					TextMeshProUGUI component = val2.GetComponent<TextMeshProUGUI>();
					((TMP_Text)component).text = ((TMP_Text)component).text + text;
				}
				else if (format.creditType == CreditType.IMAGE)
				{
					val2.GetComponent<Image>().sprite = Resources.Load<Sprite>("Credits/" + credits[i].columns[j]);
					val2.GetComponent<Image>().preserveAspect = true;
				}
			}
		}
	}

	private void OnEnable()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		creditMainPanel.transform.localPosition = new Vector3(creditMainPanel.transform.localPosition.x, startPos, creditMainPanel.transform.localPosition.z);
		remainingDelay = initialDelay;
	}

	private void OnDisable()
	{
	}

	private void PauseUnpauseCreditsScroll()
	{
		paused = !paused;
	}

	private void FixedUpdate()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (paused)
		{
			return;
		}
		if (remainingDelay > 0f)
		{
			remainingDelay -= Time.unscaledDeltaTime;
			return;
		}
		creditMainPanel.transform.localPosition = Vector2.op_Implicit(new Vector2(creditMainPanel.transform.localPosition.x, creditMainPanel.transform.localPosition.y + creditScrollSpeed));
		float y = creditsRect.anchoredPosition.y;
		Rect rect = creditsRect.rect;
		if (y >= rect.height && OnFinish != null)
		{
			OnFinish();
		}
	}

	private void GenerateCreditEntry(CreditStruct credit)
	{
		if (string.IsNullOrEmpty(GetFormat(credit.format).format))
		{
			Debug.LogError((object)("Credit entry format (" + credit.format + ") is missing from formats"));
		}
	}

	public void ClearCredits()
	{
		Debug.Log((object)"CLearing Credits");
		credits = new List<CreditStruct>();
	}

	public void AddCredit(CreditStruct credit)
	{
		credits.Add(credit);
	}

	public void AddFormat(string formatName)
	{
		for (int i = 0; i < formats.Count; i++)
		{
			if (formats[i].format == formatName)
			{
				formatName = "";
				break;
			}
		}
		if (!string.IsNullOrEmpty(formatName))
		{
			FormatStruct item = new FormatStruct
			{
				format = formatName
			};
			formats.Add(item);
		}
	}

	public FormatStruct GetFormat(string formatName)
	{
		foreach (FormatStruct format in formats)
		{
			if (format.format == formatName)
			{
				return format;
			}
		}
		return new FormatStruct
		{
			format = ""
		};
	}

	private void LoadCredits()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		CsvReader val = new CsvReader((TextReader)new StringReader(((object)CSVCredits).ToString()));
		ClearCredits();
		while (val.Read())
		{
			CreditStruct credit = default;
			string formatName = (credit.format = val.GetField<string>("Format"));
			credit.columns = new string[NumberOfContentColumns];
			for (int i = 1; i <= NumberOfContentColumns; i++)
			{
				string text = "";
				text = val.GetField<string>("Column" + i);
				credit.columns[i - 1] = text;
			}
			AddCredit(credit);
			AddFormat(formatName);
		}
	}
}
