using Trarizon.TextCommand.Input;
using Trarizon.TextCommand.Utilities;

namespace Trarizon.TextCommand;
/// <summary>
/// Utilities
/// </summary>
public static class TextCommandExecution
{
    /// <summary>
    /// Split input into <see cref="string"/><c>[]</c> as args
    /// </summary>
    public static string[] SplitAsArgs(this string input)
    {
        var rest = new StringInputMatcher(input)[..];
        var result = new string[rest.Indices.Length];

        for (int i = 0; i < rest.Indices.Length; i++) {
            var index = rest.Indices[i];
            if (index.Kind == ArgIndexKind.Slice) {
                var (start, length) = index.SliceRange;
                result[i] = rest.Source.Slice(start, length).ToString();
            }
            else {
                var (start, length) = index.EscapedRange;
                result[i] = StringUtil.UnescapeToString(rest.Source.Slice(start, length));
            }
        }
        return result;
    }
}
