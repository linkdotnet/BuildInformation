using System.Text;

namespace LinkDotNet.BuildInformation;

// netstandard2.0 analyzers can't reliably load System.Text.Json inside every compiler host (e.g. Visual Studio).
internal static class Json
{
    public static object? Parse(string json)
    {
        var index = 0;
        return ReadValue(json, ref index);
    }

    private static object? ReadValue(string json, ref int index)
    {
        return Peek(json, ref index) switch
        {
            '{' => ReadObject(json, ref index),
            '[' => ReadArray(json, ref index),
            '"' => ReadString(json, ref index),
            _ => SkipLiteral(json, ref index),
        };
    }

    private static Dictionary<string, object?> ReadObject(string json, ref int index)
    {
        var result = new Dictionary<string, object?>();
        index++;
        while (Peek(json, ref index) != '}')
        {
            var key = ReadString(json, ref index);
            Peek(json, ref index);
            index++;
            result[key] = ReadValue(json, ref index);
            if (Peek(json, ref index) == ',')
            {
                index++;
            }
        }

        index++;
        return result;
    }

    private static List<object?> ReadArray(string json, ref int index)
    {
        var result = new List<object?>();
        index++;
        while (Peek(json, ref index) != ']')
        {
            result.Add(ReadValue(json, ref index));
            if (Peek(json, ref index) == ',')
            {
                index++;
            }
        }

        index++;
        return result;
    }

    private static string ReadString(string json, ref int index)
    {
        var builder = new StringBuilder();
        index++;
        while (json[index] != '"')
        {
            var c = json[index++];
            if (c != '\\')
            {
                builder.Append(c);
                continue;
            }

            c = json[index++];
            if (c == 'u')
            {
                builder.Append((char)Convert.ToInt32(json.Substring(index, 4), 16));
                index += 4;
                continue;
            }

            builder.Append(c switch
            {
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'b' => '\b',
                'f' => '\f',
                _ => c,
            });
        }

        index++;
        return builder.ToString();
    }

    private static object? SkipLiteral(string json, ref int index)
    {
        while (index < json.Length && json[index] is not (',' or '}' or ']') && !char.IsWhiteSpace(json[index]))
        {
            index++;
        }

        return null;
    }

    private static char Peek(string json, ref int index)
    {
        while (char.IsWhiteSpace(json[index]))
        {
            index++;
        }

        return json[index];
    }
}
