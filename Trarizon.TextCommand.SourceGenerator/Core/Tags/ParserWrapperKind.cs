using System;

namespace Trarizon.TextCommand.SourceGenerator.Core.Tags;
[Flags]
internal enum ParserWrapperKind
{
    None = 0,
    Nullable,
    ImplicitConversion,
}
