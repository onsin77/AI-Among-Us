using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using Steamworks;
using UnityEngine;

public class TranslationController : DestroyableSingleton<TranslationController>
{
	private static readonly StringNames[] SystemTypesToStringNames;

	private static readonly StringNames[] TaskTypesToStringNames;

	public List<ITranslatedText> ActiveTexts = new List<ITranslatedText>();

	public Dictionary<SupportedLangs, string> dateFormats = new Dictionary<SupportedLangs, string>
	{
		{
			SupportedLangs.English,
			"MM/dd/yyyy"
		},
		{
			SupportedLangs.Latam,
			"dd/MM/yyyy"
		},
		{
			SupportedLangs.Brazilian,
			"dd/MM/yyyy"
		},
		{
			SupportedLangs.Portuguese,
			"dd/MM/yyyy"
		},
		{
			SupportedLangs.Korean,
			"yyyy-MM-dd"
		},
		{
			SupportedLangs.Russian,
			"yyyy-MM-dd"
		},
		{
			SupportedLangs.Dutch,
			"dd/MM/yyyy"
		},
		{
			SupportedLangs.Filipino,
			"MM/dd/yyyy "
		},
		{
			SupportedLangs.French,
			"dd/MM/yyyy"
		},
		{
			SupportedLangs.German,
			"dd/MM/yyyy"
		},
		{
			SupportedLangs.Italian,
			"dd/MM/yyyy"
		},
		{
			SupportedLangs.Japanese,
			"yyyy年MM月dd日"
		},
		{
			SupportedLangs.Spanish,
			"dd/MM/yyyy"
		},
		{
			SupportedLangs.SChinese,
			"yyyy-MM-dd"
		},
		{
			SupportedLangs.TChinese,
			"yyyy-MM-dd"
		},
		{
			SupportedLangs.Irish,
			"dd-MM-yyyy"
		}
	};

	private LanguageUnit currentLanguage;

	private LanguageUnit fallbackLanguage;

	public Dictionary<SupportedLangs, TranslatedImageSet> Languages { get; private set; }

	static TranslationController()
	{
		SystemTypesToStringNames = SystemTypeHelpers.AllTypes.Select((SystemTypes t) =>
		{
			Enum.TryParse<StringNames>(t.ToString(), out var result);
			return result;
		}).ToArray();
		TaskTypesToStringNames = TaskTypesHelpers.AllTypes.Select((TaskTypes t) =>
		{
			Enum.TryParse<StringNames>(t.ToString(), out var result);
			return result;
		}).ToArray();
	}

	public void Initialize()
	{
		Languages = new Dictionary<SupportedLangs, TranslatedImageSet>();
		foreach (TranslatedImageSet language in DestroyableSingleton<ReferenceDataManager>.Instance.Refdata.languages)
		{
			Languages[language.languageID] = language;
		}
		TranslatedImageSet translatedImageSet = Languages[DataManager.Settings.Language.CurrentLanguage];
		currentLanguage = new LanguageUnit(translatedImageSet);
		BlockedWords.SetLanguage(translatedImageSet);
		TranslatedImageSet value = Languages.First((KeyValuePair<SupportedLangs, TranslatedImageSet> lang) => lang.Key == SupportedLangs.English).Value;
		fallbackLanguage = ((translatedImageSet.languageID == SupportedLangs.English) ? currentLanguage : new LanguageUnit(value));
		DataManager.Settings.Multiplayer.ValidateFilterTags();
	}

	public void SetLanguage(TranslatedImageSet lang)
	{
		Debug.Log((object)("Set language to " + lang.languageID));
		DataManager.Settings.Language.CurrentLanguage = lang.languageID;
		AnnouncementPopUp.ClearAnnouncementCache();
		currentLanguage = new LanguageUnit(Languages[lang.languageID]);
		BlockedWords.SetLanguage(lang);
		DataManager.Settings.Multiplayer.ValidGameFilterOptions.ResetToDefaults(DateTime.MinValue);
		for (int i = 0; i < ActiveTexts.Count; i++)
		{
			ActiveTexts[i].ResetText();
		}
	}

	public Sprite GetImage(ImageNames id)
	{
		return currentLanguage.GetImage(id) ?? fallbackLanguage.GetImage(id);
	}

	public string GetString(StringNames id, params object[] parts)
	{
		return GetString(id.ToString(), string.Empty, parts);
	}

	public string GetString(string id, string defaultStr, params object[] parts)
	{
		if (currentLanguage == null)
		{
			Debug.LogError((object)("Accessing Translations before initialization: " + id));
			return "STRMISS";
		}
		string text = currentLanguage.GetString(id, parts);
		if (text.IsNullOrWhiteSpace() && fallbackLanguage != null)
		{
			text = fallbackLanguage.GetString(id, parts);
		}
		if (text.IsNullOrWhiteSpace())
		{
			text = defaultStr;
		}
		if (text.IsNullOrWhiteSpace())
		{
			return "STRMISS";
		}
		return text;
	}

	public string GetStringWithDefault(StringNames id, string defaultStr, params object[] parts)
	{
		if (id == StringNames.NoTranslation && !string.IsNullOrEmpty(defaultStr))
		{
			return defaultStr;
		}
		return GetString(id.ToString(), defaultStr, parts);
	}

	public string GetString(SystemTypes room)
	{
		StringNames stringNames = SystemTypesToStringNames[(uint)room];
		if (stringNames == StringNames.None)
		{
			return "STRMISS";
		}
		return GetString(stringNames);
	}

	public string GetString(TaskTypes task)
	{
		return GetString(TaskTypesToStringNames[(byte)task]);
	}

	public string GetMonthStringViaNumber(int monthNum)
	{
		StringNames id = (StringNames)(2071 + monthNum - 1);
		return DestroyableSingleton<TranslationController>.Instance.GetString(id);
	}

	public StringNames GetTaskName(TaskTypes task)
	{
		return TaskTypesToStringNames[(byte)task];
	}

	public StringNames GetSystemName(SystemTypes room)
	{
		return SystemTypesToStringNames[(uint)room];
	}

	internal static uint SelectDefaultLanguage()
	{
		if (!SteamManager.Initialized)
		{
			return 0u;
		}
		if (Enum.TryParse<SupportedLangs>(SteamApps.GetCurrentGameLanguage(), ignoreCase: true, out var result))
		{
			return (uint)result;
		}
		return 0u;
	}

	public static bool TryParseLanguage(string language, out SupportedLangs output)
	{
		if (Enum.TryParse<SupportedLangs>(language, out var result))
		{
			output = result;
			return true;
		}
		language = language.ToLower();
		switch (language)
		{
		case "english":
		case "en":
		case "en-us":
		case "en-gb":
			output = SupportedLangs.English;
			return true;
		case "latin american spanish":
		case "latin american":
		case "latinoamérica":
		case "latinoamerica":
		case "español latinoamérica":
		case "espanol latinoamerica":
		case "latin":
		case "latino":
		case "es-mx":
		case "es-419":
			output = SupportedLangs.Latam;
			return true;
		case "brazilian":
		case "português brasil":
		case "portugues brasil":
		case "brasil":
		case "pt-br":
		case "br":
			output = SupportedLangs.Brazilian;
			return true;
		case "portuguese":
		case "português":
		case "portugues":
		case "pt":
		case "pt-pt":
			output = SupportedLangs.Portuguese;
			return true;
		case "korean":
		case "한국어":
		case "ko":
		case "ko-kr":
		case "kr":
			output = SupportedLangs.Korean;
			return true;
		case "russian":
		case "Русский":
		case "русский":
		case "ru":
		case "ru-ru":
			output = SupportedLangs.Russian;
			return true;
		case "dutch":
		case "nederlands":
		case "nl":
		case "nl-nl":
			output = SupportedLangs.Dutch;
			return true;
		case "filipino":
		case "Bisaya":
		case "bisaya":
			output = SupportedLangs.Filipino;
			return true;
		case "french":
		case "français":
		case "francais":
		case "fr":
		case "fr-fr":
			output = SupportedLangs.French;
			return true;
		case "german":
		case "deutsch":
		case "de":
		case "de-de":
			output = SupportedLangs.German;
			return true;
		case "italian":
		case "italiano":
		case "it":
		case "it-it":
			output = SupportedLangs.Italian;
			return true;
		case "japanese":
		case "日本語":
		case "ja":
		case "ja-jp":
		case "jp":
			output = SupportedLangs.Japanese;
			return true;
		case "spanish":
		case "español":
		case "espanol":
		case "es":
		case "es-es":
			output = SupportedLangs.Spanish;
			return true;
		case "simplified chinese":
		case "chinese simplified":
		case "chinese":
		case "zh":
		case "zh-cn":
		case "简体中文":
			output = SupportedLangs.SChinese;
			return true;
		case "traditional chinese":
		case "chinese traditional":
		case "zh-tw":
		case "繁體中文":
			output = SupportedLangs.TChinese;
			return true;
		case "irish":
		case "gaeilge":
			output = SupportedLangs.Irish;
			return true;
		default:
		{
			SupportedLangs supportedLangs = (SupportedLangs)SelectDefaultLanguage();
			Debug.Log((object)string.Format("[{0}] Language '{1}' not found. Output set to default language: {2}", "TranslationController", language, supportedLangs));
			output = supportedLangs;
			return false;
		}
		}
	}

	public static bool DoesLanguageSupportBold(SupportedLangs language)
	{
		switch (language)
		{
		default:
			Debug.LogError((object)string.Format("[{0}] {1}() Language '{2}' has not been configured. Defaulting to TRUE", "TranslationController", "DoesLanguageSupportBold", language));
			return true;
		case SupportedLangs.English:
		case SupportedLangs.Latam:
		case SupportedLangs.Brazilian:
		case SupportedLangs.Portuguese:
		case SupportedLangs.Russian:
		case SupportedLangs.Dutch:
		case SupportedLangs.French:
		case SupportedLangs.German:
		case SupportedLangs.Italian:
		case SupportedLangs.Spanish:
		case SupportedLangs.Irish:
			return true;
		case SupportedLangs.Korean:
		case SupportedLangs.Filipino:
		case SupportedLangs.Japanese:
		case SupportedLangs.SChinese:
		case SupportedLangs.TChinese:
			return false;
		}
	}
}
