using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities;
internal static class WrapperUtils
{
    public static Optional<T> Optional<T>(T value) => new(value);
}
