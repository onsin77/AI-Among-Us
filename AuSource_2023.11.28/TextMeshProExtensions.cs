using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public static class TextMeshProExtensions
{
	public static float GetNotDumbRenderedHeight(this TextMeshPro self)
	{
		if ((double)((TMP_Text)self).renderedHeight > 100000000.0 || (double)((TMP_Text)self).renderedHeight < -100000000.0)
		{
			return 0f;
		}
		return ((TMP_Text)self).renderedHeight;
	}

	public static Vector2 CursorPos(this TextMeshPro self)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (((TMP_Text)self).textInfo == null || ((TMP_Text)self).textInfo.lineCount == 0 || ((TMP_Text)self).textInfo.lineInfo[0].characterCount <= 0)
		{
			return Vector2.zero;
		}
		return ((TMP_Text)self).textInfo.lineInfo.Last((TMP_LineInfo l) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return l.characterCount > 0;
		}).lineExtents.max;
	}

	private static int TextInfoIndexOf(this TextMeshPro self, string str, int wordOffset = 0)
	{
		TMP_CharacterInfo[] characterInfo = ((TMP_Text)self).textInfo.characterInfo;
		for (int i = wordOffset; i < characterInfo.Length; i++)
		{
			int j = 0;
			int num = 0;
			for (; j < str.Length && i + j < characterInfo.Length && characterInfo[i + j].character == str[j]; j++)
			{
				num++;
			}
			if (num == str.Length)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool GetLinkPosition(this TextMeshPro self, int linkIndex, out Vector3 bottomLeft, out Vector3 topRight)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		TMP_TextInfo textInfo = ((TMP_Text)self).textInfo;
		if (linkIndex >= textInfo.linkCount)
		{
			bottomLeft = Vector3.zero;
			topRight = Vector3.zero;
			return false;
		}
		TMP_LinkInfo val = textInfo.linkInfo[linkIndex];
		TMP_CharacterInfo val2 = ((TMP_Text)self).textInfo.characterInfo[val.linkTextfirstCharacterIndex];
		TMP_CharacterInfo val3 = ((TMP_Text)self).textInfo.characterInfo[val.linkTextfirstCharacterIndex + val.linkTextLength];
		bottomLeft = val2.bottomLeft;
		topRight = val3.topRight;
		for (int i = val.linkTextfirstCharacterIndex + 1; i < val.linkTextfirstCharacterIndex + val.linkTextLength; i++)
		{
			Vector3 bottomLeft2 = ((TMP_Text)self).textInfo.characterInfo[i].bottomLeft;
			Vector3 topRight2 = ((TMP_Text)self).textInfo.characterInfo[i].topRight;
			bottomLeft.y = Mathf.Min(bottomLeft.y, bottomLeft2.y);
			topRight.y = Mathf.Max(topRight.y, topRight2.y);
		}
		bottomLeft.z = (topRight.z = self.transform.localPosition.z);
		return true;
	}

	public static bool GetWordPosition(this TextMeshPro self, string str, List<string> wordsPositioned, out Vector3 bottomLeft, out Vector3 topRight, out int lineBreakCount)
	{
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		int num = wordsPositioned.Count((string text) => text == str);
		int num2 = self.TextInfoIndexOf(str);
		for (int num3 = 0; num3 < num; num3++)
		{
			num2 = self.TextInfoIndexOf(str, num2 + 1);
		}
		if (num2 != -1)
		{
			int num4 = num2 + str.Length - 1;
			if (num4 < ((TMP_Text)self).textInfo.characterInfo.Length)
			{
				TMP_CharacterInfo val = ((TMP_Text)self).textInfo.characterInfo[num2];
				TMP_CharacterInfo val2 = ((TMP_Text)self).textInfo.characterInfo[num4];
				bottomLeft = val.bottomLeft;
				topRight = val2.topRight;
				lineBreakCount = 0;
				float x = val.bottomLeft.x;
				float num5 = 0f;
				float num6 = 0f;
				for (int num7 = num2 + 1; num7 < num4; num7++)
				{
					Vector3 bottomLeft2 = ((TMP_Text)self).textInfo.characterInfo[num7].bottomLeft;
					Vector3 topRight2 = ((TMP_Text)self).textInfo.characterInfo[num7].topRight;
					num6 = bottomLeft2.x - x;
					x = bottomLeft2.x;
					if (num5 * num6 < 0f)
					{
						lineBreakCount++;
					}
					num5 = num6;
					bottomLeft.y = Mathf.Min(bottomLeft.y, bottomLeft2.y);
					topRight.y = Mathf.Max(topRight.y, topRight2.y);
				}
				bottomLeft.z = (topRight.z = self.transform.localPosition.z);
				lineBreakCount /= 2;
				return true;
			}
		}
		bottomLeft = Vector3.zero;
		topRight = Vector3.zero;
		lineBreakCount = 0;
		return false;
	}

	public static int GetSplitWordPositions(this TextMeshPro self, string str, List<Vector3> bottomLeft, List<Vector3> topRight)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		int num = self.TextInfoIndexOf(str);
		bottomLeft.Clear();
		topRight.Clear();
		if (num != -1)
		{
			int num2 = num + str.Length - 1;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			if (num2 < ((TMP_Text)self).textInfo.characterInfo.Length)
			{
				TMP_CharacterInfo val = ((TMP_Text)self).textInfo.characterInfo[num];
				zero = val.bottomLeft;
				zero2 = val.topRight;
				float x = val.bottomLeft.x;
				float num3 = 0f;
				float num4 = 0f;
				bool flag = false;
				for (int i = num + 1; i <= num2; i++)
				{
					Vector3 bottomLeft2 = ((TMP_Text)self).textInfo.characterInfo[i].bottomLeft;
					Vector3 topRight2 = ((TMP_Text)self).textInfo.characterInfo[i].topRight;
					num4 = bottomLeft2.x - x;
					x = bottomLeft2.x;
					if (!flag && num3 * num4 < 0f)
					{
						flag = true;
						zero.z = (zero2.z = self.transform.localPosition.z);
						bottomLeft.Add(zero);
						topRight.Add(zero2);
						zero2 = topRight2;
						zero = bottomLeft2;
					}
					else if (flag)
					{
						flag = false;
					}
					num3 = num4;
					zero.x = Mathf.Min(zero.x, bottomLeft2.x);
					zero.y = Mathf.Min(zero.y, bottomLeft2.y);
					zero2.y = Mathf.Max(zero2.y, topRight2.y);
					zero2.x = Mathf.Max(zero2.x, topRight2.x);
				}
				zero.z = (zero2.z = self.transform.localPosition.z);
				bottomLeft.Add(zero);
				topRight.Add(zero2);
			}
		}
		return bottomLeft.Count;
	}
}
