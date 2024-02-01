# Update History

## 0.1.0

- [Breaking] Method signature of `ArgProvider.Get`s changed.
- [Breaking] Changed format of keys in parameter set
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