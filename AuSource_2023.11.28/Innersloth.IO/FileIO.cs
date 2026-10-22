using System.IO;
using System.Text;
using UnityEngine;

namespace Innersloth.IO;

public static class FileIO
{
	private static IPlatformFileIO platform;

	static FileIO()
	{
		platform = new DefaultFileIO();
	}

	public static string GetUserName()
	{
		return platform.GetUserName();
	}

	public static string GetPlatformName()
	{
		return platform.GetPlatformName();
	}

	public static string GetRootDataPath()
	{
		return platform.GetRootDataPath();
	}

	public static bool Exists(string path)
	{
		return platform.Exists(path);
	}

	public static string ReadAllText(string path)
	{
		return platform.ReadAllText(path);
	}

	public static void WriteAllText(string path, string contents)
	{
		platform.WriteAllText(path, contents);
	}

	public static byte[] ReadAllBytes(string path)
	{
		return platform.ReadAllBytes(path);
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		platform.WriteAllBytes(path, bytes);
	}

	public static void Delete(string path)
	{
		platform.Delete(path);
	}

	public static string GetDataPathTo(params string[] directories)
	{
		string text = GetRootDataPath();
		foreach (string text2 in directories)
		{
			if (!string.IsNullOrWhiteSpace(text2))
			{
				text = Path.Combine(text, text2);
			}
		}
		return text;
	}

	public static string FilterText(string input, string inputCompo = "")
	{
		char c = ' ';
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Clear();
		foreach (char c2 in input)
		{
			if (c != ' ' || c2 != ' ')
			{
				if (c2 == '\b')
				{
					stringBuilder.Length = Mathf.Max(stringBuilder.Length - 1, 0);
				}
				if (IsCharAllowed(c2))
				{
					stringBuilder.Append(c2);
					c = c2;
				}
			}
		}
		stringBuilder.Length = Mathf.Min(stringBuilder.Length, 10);
		input = stringBuilder.ToString();
		return input;
	}

	public static bool IsCharAllowed(char i)
	{
		switch (i)
		{
		case ' ':
			return true;
		case 'A':
		case 'B':
		case 'C':
		case 'D':
		case 'E':
		case 'F':
		case 'G':
		case 'H':
		case 'I':
		case 'J':
		case 'K':
		case 'L':
		case 'M':
		case 'N':
		case 'O':
		case 'P':
		case 'Q':
		case 'R':
		case 'S':
		case 'T':
		case 'U':
		case 'V':
		case 'W':
		case 'X':
		case 'Y':
		case 'Z':
			return true;
		default:
			if (i >= 'a' && i <= 'z')
			{
				return true;
			}
			if (i >= '0' && i <= '9')
			{
				return true;
			}
			if (i >= 'À' && i <= 'ÿ')
			{
				return true;
			}
			if (i >= 'Ѐ' && i <= 'џ')
			{
				return true;
			}
			if (i >= '\u3040' && i <= '㆟')
			{
				return true;
			}
			if (i >= 'ⱡ' && i <= '힣')
			{
				return true;
			}
			return false;
		}
	}
}
