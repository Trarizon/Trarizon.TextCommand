# Trarizon.TextCommand.Design

TODO:
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

Diagnostic TODO:
- [x] `Execution.CommandName` cannot contains white space, cannot with `-` prefix
- [x] `Executor.CommandPrefixes` cannot contains white space, cannot with `-` prefix
- [x] (warning) `Value` or `MultiValue` shouldn't appears after RestValue
- [x] (warning) Parameters Not `Required` may be `default`
- [x] OptionKey cannot repeat
- [x] static Execution with non-static Executors

需要c#12以上版本

## 设计

### Execution

- 必须有唯一参数
	- 参数类型为`string`、`ReadOnlySpan<char>` 、`Span<string>`、`ReadOnlySpan<string>`、`string[]`、`List<string>`或其他可匹配类型
		- 可匹配类型指：可使用`string`进行匹配，可切片获得`IEnumerable<string>`类型，分析器不做保证
- [warning] 返回类型可`default`，因为未匹配时返回`default`
- Execution上标记`[Executor]`会被忽略
- CommandName不能带有空白字符或前缀`-`

#### 执行

- 将输入转换为可匹配`string`列表
	- `string`输入使用内置`StringInputMatcher`和`StringInputRest`匹配，特殊设计减少`string`分配
	- `Span<string>`、`ReadOnlySpan<string>`输入直接匹配
	- `string[]`、`List<string>`使用`AsSpan<string>()`进行匹配
	- 其余可用类型直接匹配，不做保证
- 使用列表模式匹配命令前缀，并获取剩余值
- 将剩余值作为参数解析，调用指定Executor
	- 通过`ParameterSet`获取解析后参数
		- 多余参数无视
	- 通过`ArgsProvider`获取传入实参
- 未匹配则返回`default`

### Executor

- 返回类型需要可隐式转换到Execution的返回类型
- 不同Executor的Attribute参数不能重复或被截断（会在switch被其他分支匹配）
- CommandPrefixes不能带有空白字符，

### Parameter

- 一个参数只能有一个继承自`[CLParameter]`的attribute
- 若参数没有默认Parser，则必需显式指定Parser
	- `bool`视CLParameter类型默认Parser为`BooleanFlagParser`或`ParsableParser<>`
	- `ISpanParsable`默认Parser为`ParsableParser<>`
	- `enum`类型默认Parser为`EnumParser`
	- `Nullable<T> where T : ISpanParsable<T>`默认Parser为`NullableParser<ParsableParser<T>, T>`
- Parser需为当前类型的字段、属性或方法
	- 字段/属性Parser视类型需实现`IArgParser`或`IArgFlagParser`
	- 方法Parser签名视CLParameter类型匹配`ArgParsingDelegate`或`ArgParsingDelegate`，程序会以`DelegateParser<>`或`DelegateFlagParser<>`包装使用
- 参数名不允许重复
- [warning] RestValues之后不应出现Value或Values参数
- [warning] 未标记Required的参数可能为`default`

#### Flag

- `bool`默认为`Flag`，使用`BooleanFlagParser`
- 其余类型指定Parser实现`IArgFlagParser<>`，或方法签名匹配`ArgFlagParsingDelegate`

#### Option

- 未指定非`bool`类型默认为`Option`，部分提供默认parser
	- `ISpanParsable<>`使用`ParsableParser`，其中对`string`特殊处理以优化性能
	- `enum`使用`EnumParser`
	- `Nullable<T> where T:ISpanParsable<T>`使用`NullableParser<ParsableParser<T>, T>`
- 其余类型指定Parser实现`IArgParser`，或签名匹配`ArgParsingDelegate`

#### Value

- Parser同[Option](#option)
 
#### Values

- [warning] 参数不能<=0，该值预留作为RestValues
- 类型必须为`Span<>`,`ReadOnlySpan<>`,`T[]`,`List<>`,`IEnumerable<>`之一
- Parser同[Value](#value)