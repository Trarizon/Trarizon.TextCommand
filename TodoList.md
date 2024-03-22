# TODO

## Features

- [?] `Multi-Flag`（好像直接多个flag也没什么区别（
- [ ] `Multi-Value` supports more types(collection expressions)
	- `ROS<>`, `Span<>` unmanaged使用stackalloc，其他使用`T[]`
	- `T[]`
	- `List<>`
	- `IEnumerable<>` 用`T[]`
	- `ImmutableArray<>` 用`T[]`
	- 其他使用collection expression创建？
- [ ] custom matcher
	- 支持`ROS<char>`或`string`的模式匹配
	- 切片时获取`ROS<string>`或实现`IEnumerable<string>`的类型（或`Memory<char>`？）
	- 有匹配input类型的单参ctor
	- [x] 非可匹配类型时要报错吗？
- [ ] `DefaultValue` in `IRequiredParameterAttribute`
	- 设置了`DefaultValue`，则`Required`无效
	- `ArgResult<>`公开`GetValue(T defaultValue)`，获取设定默认值后的参数
- [x] `ContextParameters: string[]` in `ExecutionAttribute`
	- [x] `ContextParameterAttribute` on parameter
		- 接受单个`string`参数，表示对应的context parameter在execution中的参数名，executor和execution参数名一致时可以不指定
	- `ContextParameters`为空时，所有参数（包括input）认为是context parameter
	- 调用executor时，直接将对应的execution参数传入
	- 可以为`in` `ref` `out` etc.，为`out`时所有executor必需包含这个context parameter
- [ ] `Value.Order` (maybe?)
- [ ] Error or support multiple execution
- [ ] ErrorHandler selection
	- 按顺序查找匹配方法：本地双参方法，本地单参数方法，直接基类双参方法，直接基类单参方法...

## Analyzer:

- [~] 优化一下diagnostic的位置
