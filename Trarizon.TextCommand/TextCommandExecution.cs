using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand;
public static class TextCommandExecution
{
    public static string[] SplitAsArgs(string input)
    {
        var matcher = new StringInputMatcher(input);
        var result = new string[matcher.Length];

        for (int i = 0; i < matcher.Length; i++) {
            result[i] = matcher[i];
        }
        return result;
    }
}
