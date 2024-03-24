using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
/// <summary>
/// Parsing a input into specific type, this delegate signature matches <see cref="IArgParser{T}.TryParse(InputArg, out T)"/>
/// </summary>
/// <typeparam name="T">Parser result type</typeparam>
/// <param name="input">Input</param>
/// <param name="result"></param>
/// <returns>Is parsing succeed</returns>
public delegate bool ArgParsingDelegate<T>(InputArg input, [MaybeNullWhen(false)] out T result);

/// <summary>
/// Parsing a ReadOnlySpan{char} input into specific type, this delegate signature matches <see cref="IArgParser{T}.TryParse(InputArg, out T)"/>
/// </summary>
/// <typeparam name="T">Parser result type</typeparam>
/// <param name="input">Input</param>
/// <param name="result"></param>
/// <returns>Is parsing succeed</returns>
public delegate bool ArgSpanParsingDelegate<T>(ReadOnlySpan<char> input, [MaybeNullWhen(false)] out T result);

/// <summary>
/// Parsing a string input into specific type, this delegate signature matches <see cref="IArgParser{T}.TryParse(InputArg, out T)"/>
/// </summary>
/// <typeparam name="T">Parser result type</typeparam>
/// <param name="input">Input</param>
/// <param name="result"></param>
/// <returns>Is parsing succeed</returns>
public delegate bool ArgStringParsingDelegate<T>(string input, [MaybeNullWhen(false)] out T result);

/// <summary>
/// Parsing a flag input into specific type, this delegate signature matches <see cref="IArgFlagParser{T}.Parse(bool)"/>
/// </summary>
/// <typeparam name="T">Parser result type</typeparam>
/// <param name="flag">Input</param>
/// <returns>Result</returns>
public delegate T ArgFlagParsingDelegate<T>(bool flag);
