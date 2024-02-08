using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;

public delegate bool ArgParsingDelegate<T>(InputArg rawArg, [MaybeNullWhen(false)] out T result);

public delegate T ArgFlagParsingDelegate<T>(bool flag);
