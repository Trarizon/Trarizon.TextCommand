using System;

namespace Trarizon.TextCommand.SourceGenerator.Core.Tags;
[Flags]
internal enum ParserWrapperKinds
{
    None = 0,
    Nullable = 1,
    ImplicitConversion = 2,
}
