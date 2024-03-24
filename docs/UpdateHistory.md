# Update History

## 0.1.1

- Add `ContextParameter`, thus you can pass extra parameters through execution to executor
- [Breaking] Changed parameter name of `ArgsProvider.GetAvailableArrayLength()`
- Mark public members of `ArgsProvider` as `EditorBrowsableState.Never`
- Removed unnecessary check in `ArgsProvider`, caller(generated code) should confirm the input is valid
- Optimize in `ConversionParser`
- Support `ReadOnlySpan<char>` and `string` as method parser input
- Make `[Execution]` and `[Executor]` inheritable
- Allow error handler to be defined in base type
- Allow multiple Execution.CommandName
- [Bug] Fix many wrong errors while member has overloading

## 0.1.0

- [Breaking] Rename `CLParameterAttribute` to `ParameterAttribute`, `ParameterSet` to `ParsingContext`
- [Breaking] If you haven't use any member with [EditorBrowsable(Never)], just re-build is ok
- Add Error handling
- Refactor generator

## 0.0.2

- [Bug] Fix bug on incorrect length of `MultiValue` parameter
- [Bug] Fix bug when Execution returns `void`
- [Bug] Fix bug on parsing failure when input option is escaped 
- [Breaking] Modified parameter of `ArgsProvider.GetValues` series
- [Breaking] Swapped type parameter of `NullableParser<,>`
- [Breaking] Removed `ArgsProvider.GetRestValues` use `GetValues` instead
- [Breaking] `StringInputMatcher.this[]` returns `ReadOnlySpan<char>` now
- Modified name of `Value` and `MultiValue`, now the output err message will not contains leading `--`
- Mark `ArrayArgsProvider` with `EditorBrowsableState.Never`
- `[Executor]` is allow multiple now
- If Execution returns `void`, executor can returns any type now
- Relax restrictions on parser type, now parser return type should be able to implicit convert to target parameter type
- Optimize nullable diagnostic

## 0.0.1

- First version