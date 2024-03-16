namespace Trarizon.TextCommand.SourceGenerator.Core.Tags;
internal enum ImplicitParameterKind
{
    Invalid = 0,
    Boolean,
    SpanParsable,
    Enum,
    //NullableSpanParsable = 0x10 | SpanParsable,
    //NullableEnum = 0x10 | Enum,
}
