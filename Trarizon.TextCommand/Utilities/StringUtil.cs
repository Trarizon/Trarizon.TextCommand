namespace Trarizon.TextCommand.Utilities;
internal static class StringUtil
{
    public static string UnescapeToString(ReadOnlySpan<char> escaped)
    {
        int count = 0;
        var output = (stackalloc char[escaped.Length]);

        for (int i = 0; i < escaped.Length; i++) {
            if (escaped[i..].StartsWith(['"', '"']))
                i++;
            output[count++] = escaped[i];
        }
        return new(output[..count]);
    }
}
