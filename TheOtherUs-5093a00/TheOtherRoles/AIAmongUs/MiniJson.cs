using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AIAmongUs;

/// <summary>
/// 零依赖 JSON 工具(IL2CPP 环境不引入序列化库):
/// 写侧由 StateCollector 直接拼 JSON 片段,这里提供转义/数字格式化;
/// 读侧为下行指令提供最小递归下降解析器。
/// 解析结果类型:Dictionary&lt;string,object&gt; / List&lt;object&gt; / string / double / bool / null。
/// </summary>
public static class MiniJson
{
    // ---------- 写侧 ----------

    public static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var sb = new StringBuilder(s.Length + 8);
        foreach (var c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                default:
                    if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    else sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    public static string Str(string s) => "\"" + Escape(s ?? string.Empty) + "\"";

    public static string B(bool b) => b ? "true" : "false";

    /// <summary>浮点格式化:定点十进制、不变文化、最多 dec 位小数(整数不带小数点)。</summary>
    public static string F(double v, int dec = 2)
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) v = 0;
        var rounded = Math.Round(v, dec);
        var fmt = "0." + new string('#', dec);
        return rounded.ToString(fmt, CultureInfo.InvariantCulture);
    }

    // ---------- 读侧 ----------

    public class JsonException : Exception
    {
        public JsonException(string m) : base(m) { }
    }

    public static object Parse(string json)
    {
        if (json == null) throw new JsonException("null input");
        int i = 0;
        var v = ParseValue(json, ref i, 0);
        SkipWs(json, ref i);
        if (i != json.Length) throw new JsonException($"trailing chars at {i}");
        return v;
    }

    // 深度上限:指令 JSON 正常不过几层;深嵌套输入会让递归下降解析器栈溢出,
    // StackOverflowException 无法被 catch,会直接杀死游戏进程(TCP 口在本机无鉴权,必须自防)
    private const int MaxDepth = 64;

    private static object ParseValue(string s, ref int i, int depth)
    {
        if (depth > MaxDepth) throw new JsonException($"nesting deeper than {MaxDepth}");
        SkipWs(s, ref i);
        if (i >= s.Length) throw new JsonException("unexpected end");
        var c = s[i];
        switch (c)
        {
            case '{': return ParseObject(s, ref i, depth + 1);
            case '[': return ParseArray(s, ref i, depth + 1);
            case '"': return ParseString(s, ref i);
            case 't': Expect(s, ref i, "true"); return true;
            case 'f': Expect(s, ref i, "false"); return false;
            case 'n': Expect(s, ref i, "null"); return null;
            default: return ParseNumber(s, ref i);
        }
    }

    private static Dictionary<string, object> ParseObject(string s, ref int i, int depth)
    {
        var dict = new Dictionary<string, object>();
        i++; // '{'
        SkipWs(s, ref i);
        if (i < s.Length && s[i] == '}') { i++; return dict; }
        while (true)
        {
            SkipWs(s, ref i);
            if (i >= s.Length || s[i] != '"') throw new JsonException($"expected key at {i}");
            var key = ParseString(s, ref i);
            SkipWs(s, ref i);
            if (i >= s.Length || s[i] != ':') throw new JsonException($"expected ':' at {i}");
            i++;
            var val = ParseValue(s, ref i, depth);
            dict[key] = val;
            SkipWs(s, ref i);
            if (i >= s.Length) throw new JsonException("unterminated object");
            if (s[i] == ',') { i++; continue; }
            if (s[i] == '}') { i++; return dict; }
            throw new JsonException($"expected ',' or '}}' at {i}");
        }
    }

    private static List<object> ParseArray(string s, ref int i, int depth)
    {
        var list = new List<object>();
        i++; // '['
        SkipWs(s, ref i);
        if (i < s.Length && s[i] == ']') { i++; return list; }
        while (true)
        {
            var val = ParseValue(s, ref i, depth);
            list.Add(val);
            SkipWs(s, ref i);
            if (i >= s.Length) throw new JsonException("unterminated array");
            if (s[i] == ',') { i++; continue; }
            if (s[i] == ']') { i++; return list; }
            throw new JsonException($"expected ',' or ']' at {i}");
        }
    }

    private static string ParseString(string s, ref int i)
    {
        i++; // '"'
        var sb = new StringBuilder();
        while (i < s.Length)
        {
            var c = s[i];
            if (c == '"') { i++; return sb.ToString(); }
            if (c == '\\')
            {
                i++;
                if (i >= s.Length) throw new JsonException("bad escape");
                var e = s[i];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u':
                        if (i + 4 >= s.Length) throw new JsonException("bad \\u");
                        var hex = s.Substring(i + 1, 4);
                        sb.Append((char)Convert.ToInt32(hex, 16));
                        i += 4;
                        break;
                    default: throw new JsonException($"bad escape \\{e}");
                }
                i++;
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }
        throw new JsonException("unterminated string");
    }

    private static double ParseNumber(string s, ref int i)
    {
        int start = i;
        while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '-' || s[i] == '+' || s[i] == '.' || s[i] == 'e' || s[i] == 'E'))
            i++;
        if (i == start) throw new JsonException($"not a number at {i}");
        return double.Parse(s.Substring(start, i - start), CultureInfo.InvariantCulture);
    }

    private static void Expect(string s, ref int i, string word)
    {
        if (i + word.Length > s.Length || s.Substring(i, word.Length) != word)
            throw new JsonException($"expected '{word}' at {i}");
        i += word.Length;
    }

    private static void SkipWs(string s, ref int i)
    {
        while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n')) i++;
    }
}
