using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Parsers;

public delegate bool ArgParsingDelegate<T>(ReadOnlySpan<char> rawArg, [MaybeNullWhen(false)] out T result);

public delegate T ArgFlagParsingDelegate<T>(bool flag);
