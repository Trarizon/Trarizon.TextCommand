# TODO

## Features

- [?] `Multi-Flag`������ֱ�Ӷ��flagҲûʲô����
- [ ] `Multi-Value` supports more types(collection expressions)
	- `ROS<>`, `Span<>` unmanagedʹ��stackalloc������ʹ��`T[]`
	- `T[]`
	- `List<>`
	- `IEnumerable<>` ��`T[]`
	- `ImmutableArray<>` ��`T[]`
	- ����ʹ��collection expression������
- [ ] custom matcher
	- ֧��`ROS<char>`��`string`��ģʽƥ��
	- ��Ƭʱ��ȡ`ROS<string>`��ʵ��`IEnumerable<string>`�����ͣ���`Memory<char>`����
	- ��ƥ��input���͵ĵ���ctor
	- [x] �ǿ�ƥ������ʱҪ������
- [ ] `DefaultValue` in `IRequiredParameterAttribute`
	- ������`DefaultValue`����`Required`��Ч
	- `ArgResult<>`����`GetValue(T defaultValue)`����ȡ�趨Ĭ��ֵ��Ĳ���
- [x] `ContextParameters: string[]` in `ExecutionAttribute`
	- [x] `ContextParameterAttribute` on parameter
		- ���ܵ���`string`��������ʾ��Ӧ��context parameter��execution�еĲ�������executor��execution������һ��ʱ���Բ�ָ��
	- `ContextParameters`Ϊ��ʱ�����в���������input����Ϊ��context parameter
	- ����executorʱ��ֱ�ӽ���Ӧ��execution��������
	- ����Ϊ`in` `ref` `out` etc.��Ϊ`out`ʱ����executor����������context parameter
- [ ] `Value.Order` (maybe?)
- [ ] Error or support multiple execution
- [ ] ErrorHandler selection
	- ��˳�����ƥ�䷽��������˫�η��������ص�����������ֱ�ӻ���˫�η�����ֱ�ӻ��൥�η���...

## Analyzer:

- [~] �Ż�һ��diagnostic��λ��
