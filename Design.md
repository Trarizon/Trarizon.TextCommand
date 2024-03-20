# Trarizon.TextCommand.Design

需要c#12以上版本

## 设计

### Execution

- 必须有唯一参数
	- 参数类型为`string`、`ReadOnlySpan<char>`
- [warning] 返回类型可`default`，因为未匹配时返回`default`
- Execution上标记`[Executor]`会被忽略
- CommandName不能带有空白字符或前缀`-`
- 未指定ErrorHandler时使用ErrorBuilder内置的DefaultErrorHandler
- Custom error handler满足以下要求
	- 是类型内部的方法
	- 返回值`void`或可隐式转换到Execution返回类型
	- 第一个参数为`ArgResultErrors`，可标记为`in`
	- 第二个参数可选，为`string`，表示发生错误时本应调用的Executor方法名
- Execution的所有参数视为[Context parameter](#contextparameter)，可以标记任意refkind

#### 执行

1. 将输入转换为可匹配`string`列表
	- `string`输入使用内置`StringInputMatcher`和`StringInputRest`匹配
2. 使用列表模式匹配命令前缀，并获取剩余值
3. 将剩余值作为参数解析，调用指定Executor
	1. 通过`ParsingContext`获取解析后参数
		- 多余参数无视
	1. 通过`ArgsProvider`获取传入实参，存在error则添加到error builder
	1. 若存在error，则调用Error handler，否则调用Executor
4. 未匹配则返回`default`

### Executor

- 返回类型需要可隐式转换到Execution的返回类型，或Execution返回`void`
- Execution需可调用Executor（当Execution为static是Executor也是）
- 不同Executor的Attribute参数不能重复或被截断（会在switch被其他分支匹配）
- CommandPrefixes不能带有空白字符或leading `-`
- 一个方法可以标记多个`[Executor]`表示不同命令指向同一个方法

### ContextParameter

- 标记`[ContextParameter]`即为context parameter，这类参数不经过parse直接从Execution获取
- 参数可以为`in` `ref` `out` `ref readonly`，只要execution中的参数可以直接传入
- execution中的参数需要可隐式转换到context parameter

### InputParameter

- 一个参数只能有一个继承自`[CLParameter]`的attribute
- 若参数没有默认Parser，则必需显式指定Parser
	- `bool`视CLParameter类型默认Parser为`BooleanFlagParser`或`ParsableParser<>`
	- `ISpanParsable`默认Parser为`ParsableParser<>`
	- `enum`类型默认Parser为`EnumParser`
	- `Nullable<T> where T : ISpanParsable<T>`默认Parser为`NullableParser<ParsableParser<T>, T>`
- Custom Parser需为当前类型的字段、属性、方法或嵌套类型。使用`Parser`或`ParserType`属性标记
	- 字段/属性Parser视类型需实现`IArgParser`或`IArgFlagParser`
	- 方法Parser签名视CLParameter类型匹配`bool TryParse(InputArg|string|ReadOnlySpan<char>, out T)`或`T Parse(bool)`，程序会以`DelegateParser<>`包装使用
	- 类型Parser需要实现对应的`IArgParser`或`IArgFlagParser`，且应为值类型
- 若custom parser的结果可隐式转换到方法参数类型，使用`ConversionParser包装`
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
- 其余类型指定Parser实现`IArgParser`，或签名匹配`bool TryParse(InputArg|string|ReadOnlySpan<char>, out T)`

#### Value

- Parser同[Option](#option)
- RestValue之后的值始终为空数组
 
#### MultiValue

- [warning] 参数不能<=0，该值预留作为RestValues
- 类型必须为`Span<>`,`ReadOnlySpan<>`,`T[]`,`List<>`之一
- + 支持以下类型
	- `ReadOnlySpan<>` - unmanaged 使用`stackalloc`，其他使用`T[]`
	- `T[]`
	- `List<>`
	- `Span<>` - 使用`ReadOnlySpan<>`
	- `IEnumerable<>` - 使用`T[]`
	+ `ImmutableArray<>` - 使用`ImmutableCollectionMarshal.As(T[])`
	+ 其他支持collection expression的类型（要写分析器手动诊断吗？）
- Parser同[Value](#value)
- 存在隐式转换时使用`ConversionParser`
- RestValue之后的值始终为空数组
