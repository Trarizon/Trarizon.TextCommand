# TODO

- [ ] `Multi-Flag` for enum type
- [ ] Better parsing-exception design
- [ ] `Multi-Value` supports more types
- [ ] `Value.Order` (maybe?)
- [x] `Parser` supports method
- [x] (BUG): `Get<,>`�ĵ�һ������Ӧ��Ϊ��ȫ�޶���
- [x] (BUG): No implicit parser��option������������
- [x] Input supports `ReadOnlySpan<char>`, as `string`
- [ ] �ǿ�ƥ������ʱҪ������
- [ ] Error or support multiple execution
- [ ] (BUG) result length of Values

Diagnostic TODO:
- [x] `Execution.CommandName` cannot contains white space, cannot with `-` prefix
- [x] `Executor.CommandPrefixes` cannot contains white space, cannot with `-` prefix
- [x] (warning) `Value` or `MultiValue` shouldn't appears after RestValue
- [x] (warning) Parameters Not `Required` may be `default`
- [x] OptionKey cannot repeat
- [x] static Execution with non-static Executors
- [ ] �Ż�һ��diagnostic��λ��
