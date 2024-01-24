﻿using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities;
internal static class SymbolDisplayFormats
{
    public static readonly SymbolDisplayFormat CSharpErrorMessageWithoutGeneric = SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);
    public static readonly SymbolDisplayFormat FullQualifiedFormatIncludeNullableRefTypeModifier = SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
}
