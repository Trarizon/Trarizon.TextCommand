using System;

namespace Trarizon.TextCommand.SourceGenerator.Core;
[Flags]
internal enum ParserWrapperKind
{
    None = 0,
    Nullable,
    ImplicitConversion,
}
