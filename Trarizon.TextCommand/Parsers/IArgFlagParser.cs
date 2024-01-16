namespace Trarizon.TextCommand.Parsers;
public interface IArgFlagParser<T>
{
    T Parse(bool flag);
}
