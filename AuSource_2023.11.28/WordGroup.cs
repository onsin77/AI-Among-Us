using System.Collections.Generic;
using System.IO;

public class WordGroup : List<string>
{
	public static WordGroup Parse(TextReader reader)
	{
		WordGroup wordGroup = new WordGroup();
		string text;
		while (!(text = reader.ReadLine()).IsNullOrWhiteSpace())
		{
			wordGroup.AddRange(text.Trim().Split(','));
		}
		return wordGroup;
	}
}
