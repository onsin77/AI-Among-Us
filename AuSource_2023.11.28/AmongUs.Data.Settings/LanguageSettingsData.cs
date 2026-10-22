using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Settings;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class LanguageSettingsData : AbstractSaveDataGroup
{
	public const string DEFAULT_LANGUAGE = "none";

	public const SupportedLangs DEFAULT_CACHED_LANGUAGE = (SupportedLangs)(-1);

	[JsonProperty("currentLanguage")]
	private string language = "none";

	private SupportedLangs cachedLanguage = (SupportedLangs)(-1);

	public SupportedLangs CurrentLanguage
	{
		get
		{
			return cachedLanguage;
		}
		set
		{
			SetValue(ref language, value.ToString(), OnLanguageChanged);
			cachedLanguage = value;
		}
	}

	public event Action OnLanguageChanged;

	protected override void OnSaveStart()
	{
		base.OnSaveStart();
		if (Enum.IsDefined(typeof(SupportedLangs), cachedLanguage))
		{
			language = cachedLanguage.ToString();
		}
	}

	protected override void OnLoadComplete()
	{
		base.OnLoadComplete();
		TranslationController.TryParseLanguage(language, out cachedLanguage);
	}
}
