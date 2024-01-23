using Trarizon.TextCommand.Input;
using Trarizon.TextCommand.Utilities;

namespace Trarizon.TextCommand;
public static class TextCommandExecution
{
    public static string[] SplitAsArgs(string input)
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
