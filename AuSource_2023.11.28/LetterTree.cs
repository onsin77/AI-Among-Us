using System;
using System.Collections.Generic;
using System.Text;

public class LetterTree
{
	private enum NodeTypes : byte
	{
		NonTerm,
		Terminal,
		TerminalStrict,
		TerminalExact,
		TerminalUnbroken
	}

	private class LetterNode
	{
		public char Letter;

		public NodeTypes Terminal;

		public LetterNode[] Children = new LetterNode[27];

		public LetterNode[] SpecialChildren;

		public LetterNode(char l)
		{
			Letter = l;
		}

		public LetterNode CreateChild(char l)
		{
			int num = ToIndex(l, out var _);
			if (num >= Children.Length)
			{
				if (SpecialChildren == null)
				{
					SpecialChildren = new LetterNode[1];
					SpecialChildren[0] = new LetterNode(l);
					return SpecialChildren[0];
				}
				int num2 = SpecialChildren.Length;
				Array.Resize(ref SpecialChildren, num2 + 1);
				return SpecialChildren[num2] = new LetterNode(l);
			}
			LetterNode letterNode = Children[num];
			if (letterNode == null)
			{
				letterNode = (Children[num] = new LetterNode(l));
			}
			return letterNode;
		}

		public LetterNode FindChild(char l)
		{
			int num = ToIndex(l, out var _);
			if (num >= Children.Length)
			{
				if (SpecialChildren != null)
				{
					for (int i = 0; i < SpecialChildren.Length; i++)
					{
						if (SpecialChildren[i].Letter == l)
						{
							return SpecialChildren[i];
						}
					}
				}
				return null;
			}
			return Children[num];
		}
	}

	private LetterNode root = new LetterNode('\0');

	public void Clear()
	{
		root = new LetterNode('\0');
	}

	public void AddWord(string word)
	{
		LetterNode letterNode = root;
		foreach (char l in word)
		{
			if ((!IsFiller(l, out var isUnicode) || isUnicode) && !IsTerminator(l))
			{
				letterNode = letterNode.CreateChild(l);
			}
		}
		if (letterNode.Terminal == NodeTypes.NonTerm)
		{
			letterNode.Terminal = NodeTypes.Terminal;
			if (word[word.Length - 1] == '~')
			{
				letterNode.Terminal = NodeTypes.TerminalStrict;
			}
			if (word[word.Length - 1] == '^')
			{
				letterNode.Terminal = NodeTypes.TerminalExact;
			}
			if (word[word.Length - 1] == '`')
			{
				letterNode.Terminal = NodeTypes.TerminalUnbroken;
			}
		}
	}

	private bool IsTerminator(char l)
	{
		if (l != '~' && l != '^')
		{
			return l == '`';
		}
		return true;
	}

	public static bool IsFiller(char l)
	{
		bool isUnicode;
		return ToIndex(l, out isUnicode) == l;
	}

	public static bool IsFiller(char l, out bool isUnicode)
	{
		return ToIndex(l, out isUnicode) == l;
	}

	public static int ToIndex(char c)
	{
		bool isUnicode;
		return ToIndex(c, out isUnicode);
	}

	public static int ToIndex(char c, out bool isUnicode)
	{
		isUnicode = false;
		if (c >= 'A' && c <= 'Z')
		{
			return c - 65;
		}
		if (c >= 'a' && c <= 'z')
		{
			return c - 97;
		}
		if (c == 'с')
		{
			return 2;
		}
		if (c == 'к')
		{
			return 10;
		}
		if (c == 'ø')
		{
			return 14;
		}
		if (c == 'ц')
		{
			return 20;
		}
		if (c == 'и')
		{
			return 13;
		}
		if (c == 'И')
		{
			return 13;
		}
		if (c == 'я')
		{
			return 17;
		}
		if (c == 'Я')
		{
			return 17;
		}
		if (c == 'Ƨ')
		{
			return 18;
		}
		if (c == 'е')
		{
			return 4;
		}
		if (c == '$')
		{
			return 18;
		}
		if (c == '+')
		{
			return 19;
		}
		if (c == '0')
		{
			return 14;
		}
		if (c == '1')
		{
			return 8;
		}
		if (c == '!')
		{
			return 8;
		}
		if (c == '2')
		{
			return 18;
		}
		if (c == '3')
		{
			return 4;
		}
		if (c == '4')
		{
			return 0;
		}
		if (c == '5')
		{
			return 18;
		}
		if (c == '7')
		{
			return 19;
		}
		if (c == '8')
		{
			return 1;
		}
		if (c == '9')
		{
			return 6;
		}
		if (c > 'z')
		{
			isUnicode = true;
			string text = c.ToString().Normalize(NormalizationForm.FormD);
			foreach (char c2 in text)
			{
				if (c2 <= 'z')
				{
					return ToIndex(c2, out isUnicode);
				}
			}
		}
		return c;
	}

	public int SearchTestOnly(string input, int start)
	{
		char[] input2 = input.ToCharArray();
		return Search(input2, start);
	}

	public int Search(char[] input, int start)
	{
		if (start >= input.Length)
		{
			return 0;
		}
		if (IsFiller(input[start], out var isUnicode) && !isUnicode)
		{
			return 0;
		}
		bool exactStart = start == 0 || IsFiller(input[start - 1]);
		return SubSearchRec(input, start, root, postDupes: false, postBreak: false, exactStart);
	}

	private int SubSearchRec(char[] input, int start, LetterNode previous, bool postDupes, bool postBreak, bool exactStart)
	{
		if (start >= input.Length)
		{
			return -2;
		}
		char c = input[start];
		if (IsFiller(c, out var isUnicode) && (!isUnicode || previous.FindChild(c) == null))
		{
			if (postDupes)
			{
				return -2;
			}
			int num = SubSearchRec(input, start + 1, previous, postDupes, postBreak: true, exactStart);
			if (num > 0)
			{
				return num + 1;
			}
			return -2;
		}
		if (ToIndex(c) == ToIndex(previous.Letter) && !postBreak)
		{
			int num2 = SubSearchRec(input, start + 1, previous, postDupes: true, postBreak, exactStart);
			if (num2 > 0)
			{
				return num2 + 1;
			}
			if (previous.Terminal != NodeTypes.NonTerm)
			{
				return 1;
			}
		}
		LetterNode letterNode = previous.FindChild(c);
		if (letterNode == null)
		{
			return -3;
		}
		int num3 = SubSearchRec(input, start + 1, letterNode, postDupes, postBreak, exactStart);
		if (num3 > 0)
		{
			return num3 + 1;
		}
		if (letterNode.Terminal == NodeTypes.TerminalStrict && num3 == -2 && (exactStart || !postBreak))
		{
			return 1;
		}
		if (letterNode.Terminal == NodeTypes.TerminalUnbroken && num3 == -2 && !postBreak)
		{
			return 1;
		}
		if (letterNode.Terminal == NodeTypes.TerminalExact && ((num3 == -2) & exactStart))
		{
			return 1;
		}
		if (letterNode.Terminal == NodeTypes.Terminal && num3 <= 0)
		{
			return 1;
		}
		return num3;
	}

	public IEnumerable<string> GetWords()
	{
		StringBuilder b = new StringBuilder();
		LetterNode[] children = root.Children;
		foreach (LetterNode node in children)
		{
			foreach (string word in GetWords(b, 0, node))
			{
				yield return word;
			}
		}
		if (root.SpecialChildren == null)
		{
			yield break;
		}
		children = root.SpecialChildren;
		foreach (LetterNode node2 in children)
		{
			foreach (string word2 in GetWords(b, 0, node2))
			{
				yield return word2;
			}
		}
	}

	private IEnumerable<string> GetWords(StringBuilder b, int i, LetterNode node)
	{
		if (node == null)
		{
			yield break;
		}
		b.Length++;
		b[i] = node.Letter;
		switch (node.Terminal)
		{
		case NodeTypes.Terminal:
			yield return b.ToString();
			break;
		case NodeTypes.TerminalExact:
			b.Length++;
			b[i + 1] = '^';
			yield return b.ToString();
			b.Length--;
			break;
		case NodeTypes.TerminalStrict:
			b.Length++;
			b[i + 1] = '~';
			yield return b.ToString();
			b.Length--;
			break;
		case NodeTypes.TerminalUnbroken:
			b.Length++;
			b[i + 1] = '`';
			yield return b.ToString();
			b.Length--;
			break;
		}
		LetterNode[] children = node.Children;
		foreach (LetterNode node2 in children)
		{
			foreach (string word in GetWords(b, i + 1, node2))
			{
				yield return word;
			}
		}
		if (node.SpecialChildren != null)
		{
			children = node.SpecialChildren;
			foreach (LetterNode node3 in children)
			{
				foreach (string word2 in GetWords(b, i + 1, node3))
				{
					yield return word2;
				}
			}
		}
		b.Length--;
	}
}
