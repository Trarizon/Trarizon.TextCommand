# TODO

- [ ] `Multi-Flag` for enum type
- [ ] Better parsing-exception design
- [ ] `Multi-Value` supports more types
- [ ] `Value.Order` (maybe?)
- [x] `Parser` supports method
- [x] (BUG): `Get<,>`的第一个参数应该为完全限定名
- [x] (BUG): No implicit parser的option不会正常报错
- [x] Input supports `ReadOnlySpan<char>`, as `string`
- [ ] 非可匹配类型时要报错吗？
- [ ] Error or support multiple execution
- [ ] (BUG) result length of Values

Diagnostic TODO:
- [x] `Execution.CommandName` cannot contains white space, cannot with `-` prefix
- [x] `Executor.CommandPrefixes` cannot contains white space, cannot with `-` prefix
- [x] (warning) `Value` or `MultiValue` shouldn't appears after RestValue
- [x] (warning) Parameters Not `Required` may be `default`
- [x] OptionKey cannot repeat
- [x] static Execution with non-static Executors
- [ ] 优化一下diagnostic的位置
