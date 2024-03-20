# Trarizon.TextCommand.Design

��Ҫc#12���ϰ汾

## ���

### Execution

- ������Ψһ����
	- ��������Ϊ`string`��`ReadOnlySpan<char>`
- [warning] �������Ϳ�`default`����Ϊδƥ��ʱ����`default`
- Execution�ϱ��`[Executor]`�ᱻ����
- CommandName���ܴ��пհ��ַ���ǰ׺`-`
- δָ��ErrorHandlerʱʹ��ErrorBuilder���õ�DefaultErrorHandler
- Custom error handler��������Ҫ��
	- �������ڲ��ķ���
	- ����ֵ`void`�����ʽת����Execution��������
	- ��һ������Ϊ`ArgResultErrors`���ɱ��Ϊ`in`
	- �ڶ���������ѡ��Ϊ`string`����ʾ��������ʱ��Ӧ���õ�Executor������
- Execution�����в�����Ϊ[Context parameter](#contextparameter)�����Ա������refkind

#### ִ��

1. ������ת��Ϊ��ƥ��`string`�б�
	- `string`����ʹ������`StringInputMatcher`��`StringInputRest`ƥ��
2. ʹ���б�ģʽƥ������ǰ׺������ȡʣ��ֵ
3. ��ʣ��ֵ��Ϊ��������������ָ��Executor
	1. ͨ��`ParsingContext`��ȡ���������
		- �����������
	1. ͨ��`ArgsProvider`��ȡ����ʵ�Σ�����error����ӵ�error builder
	1. ������error�������Error handler���������Executor
4. δƥ���򷵻�`default`

### Executor

- ����������Ҫ����ʽת����Execution�ķ������ͣ���Execution����`void`
- Execution��ɵ���Executor����ExecutionΪstatic��ExecutorҲ�ǣ�
- ��ͬExecutor��Attribute���������ظ��򱻽ضϣ�����switch��������֧ƥ�䣩
- CommandPrefixes���ܴ��пհ��ַ���leading `-`
- һ���������Ա�Ƕ��`[Executor]`��ʾ��ͬ����ָ��ͬһ������

### ContextParameter

- ���`[ContextParameter]`��Ϊcontext parameter���������������parseֱ�Ӵ�Execution��ȡ
- ��������Ϊ`in` `ref` `out` `ref readonly`��ֻҪexecution�еĲ�������ֱ�Ӵ���
- execution�еĲ�����Ҫ����ʽת����context parameter

### InputParameter

- һ������ֻ����һ���̳���`[CLParameter]`��attribute
- ������û��Ĭ��Parser���������ʽָ��Parser
	- `bool`��CLParameter����Ĭ��ParserΪ`BooleanFlagParser`��`ParsableParser<>`
	- `ISpanParsable`Ĭ��ParserΪ`ParsableParser<>`
	- `enum`����Ĭ��ParserΪ`EnumParser`
	- `Nullable<T> where T : ISpanParsable<T>`Ĭ��ParserΪ`NullableParser<ParsableParser<T>, T>`
- Custom Parser��Ϊ��ǰ���͵��ֶΡ����ԡ�������Ƕ�����͡�ʹ��`Parser`��`ParserType`���Ա��
	- �ֶ�/����Parser��������ʵ��`IArgParser`��`IArgFlagParser`
	- ����Parserǩ����CLParameter����ƥ��`bool TryParse(InputArg|string|ReadOnlySpan<char>, out T)`��`T Parse(bool)`���������`DelegateParser<>`��װʹ��
	- ����Parser��Ҫʵ�ֶ�Ӧ��`IArgParser`��`IArgFlagParser`����ӦΪֵ����
- ��custom parser�Ľ������ʽת���������������ͣ�ʹ��`ConversionParser��װ`
- �������������ظ�
- [warning] RestValues֮��Ӧ����Value��Values����
- [warning] δ���Required�Ĳ�������Ϊ`default`

#### Flag

- `bool`Ĭ��Ϊ`Flag`��ʹ��`BooleanFlagParser`
- ��������ָ��Parserʵ��`IArgFlagParser<>`���򷽷�ǩ��ƥ��`ArgFlagParsingDelegate`

#### Option

- δָ����`bool`����Ĭ��Ϊ`Option`�������ṩĬ��parser
	- `ISpanParsable<>`ʹ��`ParsableParser`�����ж�`string`���⴦�����Ż�����
	- `enum`ʹ��`EnumParser`
	- `Nullable<T> where T:ISpanParsable<T>`ʹ��`NullableParser<ParsableParser<T>, T>`
- ��������ָ��Parserʵ��`IArgParser`����ǩ��ƥ��`bool TryParse(InputArg|string|ReadOnlySpan<char>, out T)`

#### Value

- Parserͬ[Option](#option)
- RestValue֮���ֵʼ��Ϊ������
 
#### MultiValue

- [warning] ��������<=0����ֵԤ����ΪRestValues
- ���ͱ���Ϊ`Span<>`,`ReadOnlySpan<>`,`T[]`,`List<>`֮һ
- + ֧����������
	- `ReadOnlySpan<>` - unmanaged ʹ��`stackalloc`������ʹ��`T[]`
	- `T[]`
	- `List<>`
	- `Span<>` - ʹ��`ReadOnlySpan<>`
	- `IEnumerable<>` - ʹ��`T[]`
	+ `ImmutableArray<>` - ʹ��`ImmutableCollectionMarshal.As(T[])`
	+ ����֧��collection expression�����ͣ�Ҫд�������ֶ�����𣿣�
- Parserͬ[Value](#value)
- ������ʽת��ʱʹ��`ConversionParser`
- RestValue֮���ֵʼ��Ϊ������
