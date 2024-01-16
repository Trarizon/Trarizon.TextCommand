namespace Trarizon.TextCommand.SourceGenerator.Core;
internal enum ImplicitCLParameterKind
{
    Invalid,
    Boolean,
    SpanParsable,
    Enum,
    NullableSpanParsable = 0x10 | SpanParsable,
    NullableEnum = 0x10 | Enum,
}
