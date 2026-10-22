using System.Collections.Generic;
using System.Text;

namespace AmongUs.QuickChat;

public class QuickChatPhraseBuilder
{
	public delegate void ChangeHandler(QuickChatPhraseBuilder builder);

	private QuickChatPhrase rootPhrase;

	private List<QuickChatPhrase> subPhrases;

	public bool IsEmpty => rootPhrase == null;

	public event ChangeHandler OnChangedEvent;

	public QuickChatPhraseBuilder()
	{
		Reset();
	}

	public void Reset()
	{
		rootPhrase = null;
		subPhrases = new List<QuickChatPhrase>();
		NotifyChange();
	}

	public void Start(QuickChatPhrase rootPhrase)
	{
		if (this.rootPhrase != null)
		{
			QuickChatMenu.Logger.Error("[QuickChatPhraseBuilder] Start() :: Building has already started! Aborting!");
			return;
		}
		this.rootPhrase = rootPhrase;
		NotifyChange();
	}

	public bool CanAddAsNextPhrase(QuickChatPhrase phrase)
	{
		if (rootPhrase != null)
		{
			if (!phrase.IsInput)
			{
				return false;
			}
			if (rootPhrase.IsNegative && phrase.IsNegative)
			{
				return false;
			}
		}
		return true;
	}

	public void AddNextPhrase(QuickChatPhrase phrase)
	{
		if (rootPhrase != null && phrase == rootPhrase)
		{
			QuickChatMenu.Logger.Error("[QuickChatPhraseBuilder] AddNextPhrase() :: Can't add root phrase as sub-phrases! WHY is this happening!? Who knows, but abort when it happens.");
			return;
		}
		if (rootPhrase.Type != QuickChatPhraseType.ComplexPhrase)
		{
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}() :: Can't add sub-phrases to non-{2} phrases.", "QuickChatPhraseBuilder", "AddNextPhrase", QuickChatPhraseType.ComplexPhrase));
			return;
		}
		if (subPhrases.Contains(phrase) && phrase.Type == QuickChatPhraseType.ComplexPhrase)
		{
			QuickChatMenu.Logger.Error("[QuickChatPhraseBuilder] AddNextPhrase() :: Already contains phrase '" + phrase.ToPreviewText(null) + "' - Cannot add duplicates!");
			return;
		}
		if (subPhrases.Count >= rootPhrase.PhraseInputs.Length)
		{
			QuickChatMenu.Logger.Error("[QuickChatPhraseBuilder] AddNextPhrase() :: Can't add any more phrases, input limit has already been met!");
			return;
		}
		subPhrases.Add(phrase);
		NotifyChange();
	}

	public void Undo()
	{
		if (subPhrases.Count < 1)
		{
			rootPhrase = null;
		}
		else
		{
			subPhrases.RemoveAt(subPhrases.Count - 1);
		}
		NotifyChange();
	}

	public QuickChatTag[] GetNextExpectedInput()
	{
		if (IsEmpty)
		{
			return null;
		}
		if (rootPhrase.Type != QuickChatPhraseType.ComplexPhrase)
		{
			return null;
		}
		if (rootPhrase.PhraseInputs == null || rootPhrase.PhraseInputs.Length < 1)
		{
			return null;
		}
		return rootPhrase.PhraseInputs[subPhrases.Count].Tags;
	}

	private void NotifyChange()
	{
		string text = "Root: " + ((rootPhrase != null) ? rootPhrase.ToPreviewText(null) : "NONE");
		for (int i = 0; i < subPhrases.Count; i++)
		{
			text = text + $"\r\n - Inputs[{i}]: " + ((subPhrases[i] != null) ? subPhrases[i].ToPreviewText(null) : "NONE");
		}
		QuickChatMenu.Logger.Debug("[QuickChatPhraseBuilder] NotifyChange() :: Builder changed! See stack for build info.\r\n\r\n" + text + "\r\n\r\n");
		OnChangedEvent?.Invoke(this);
	}

	public bool IsComplete()
	{
		if (IsEmpty)
		{
			return false;
		}
		switch (rootPhrase.Type)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}() :: {2}.{3} has not been configured. This should not be happening!", "QuickChatPhraseBuilder", "IsComplete", "QuickChatPhraseType", rootPhrase.Type));
			return false;
		case QuickChatPhraseType.PlayerId:
			return true;
		case QuickChatPhraseType.SimplePhrase:
			return CheckSimplePhraseComplete();
		case QuickChatPhraseType.ComplexPhrase:
			return CheckComplexPhraseComplete();
		}
	}

	private bool CheckSimplePhraseComplete()
	{
		return rootPhrase.PhraseKey != StringNames.None;
	}

	private bool CheckComplexPhraseComplete()
	{
		int num = rootPhrase.PhraseInputs.Length;
		if (subPhrases.Count != num)
		{
			return false;
		}
		return true;
	}

	public QuickChatPhraseBuilderResult Build()
	{
		if (IsEmpty)
		{
			return null;
		}
		QuickChatPhrase[] array = new QuickChatPhrase[(rootPhrase != null && rootPhrase.PhraseInputs != null) ? rootPhrase.PhraseInputs.Length : 0];
		for (int i = 0; i < subPhrases.Count; i++)
		{
			array[i] = subPhrases[i];
		}
		return new QuickChatPhraseBuilderResult(rootPhrase, array);
	}

	public void ToLogString(StringBuilder output)
	{
		output.AppendLine();
		output.AppendLine("Phrase Builder Status: " + (IsComplete() ? "Complete" : "Incomplete"));
		output.AppendLine("Root Phrase: ");
		if (rootPhrase == null)
		{
			output.AppendLine("  NOT STARTED");
			return;
		}
		output.AppendLine("  " + rootPhrase.ToLogString());
		output.AppendLine($"Expected Sub-Phrases: {rootPhrase.ExpectedInputCount}");
		for (int i = 0; i < subPhrases.Count; i++)
		{
			output.AppendLine($"  [{i}] {subPhrases[i].ToLogString()}");
		}
	}

	public QuickChatPhrase[] ToPreviewData()
	{
		if (IsEmpty)
		{
			return null;
		}
		List<QuickChatPhrase> list = new List<QuickChatPhrase>();
		list.Add(rootPhrase);
		list.AddRange(subPhrases);
		return list.ToArray();
	}
}
