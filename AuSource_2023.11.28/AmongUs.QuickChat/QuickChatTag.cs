using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatTag : ScriptableObject
{
	[SerializeField]
	private StringNames tagKey;

	[SerializeField]
	private StringNames inputKey;

	[SerializeField]
	[ColorUsage(false)]
	private Color color;

	[SerializeField]
	private string[] parsingKeys;

	public StringNames TagNameKey => tagKey;

	public StringNames InputNameKey => inputKey;

	public string LocalizedTagName => DestroyableSingleton<TranslationController>.Instance.GetString(tagKey);

	public string LocalizedInputName => DestroyableSingleton<TranslationController>.Instance.GetString(InputNameKey);

	public Color Color
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return color;
		}
	}

	public bool ContainsParsingKey(string key)
	{
		if (parsingKeys == null || parsingKeys.Length < 1)
		{
			return false;
		}
		string[] array = parsingKeys;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].ToLower() == key.ToLower())
			{
				return true;
			}
		}
		return false;
	}
}
