namespace Trarizon.TextCommand.SourceGenerator.ConstantValues;
internal static class Constants
{
    public const string GlobalNamespace_DisplayString = "<global namespace>";

    public const string ReadOnlySpan_TypeName = "System.ReadOnlySpan";
    public const string Span_TypeName = "System.Span";
    public const string List_TypeName = "System.Collections.Generic.List";
    public const string IEnumerable_TypeName = "System.Collections.Generic.IEnumerable";
    public const string ISpanParsable_TypeName = "System.ISpanParsable";

    public const string ReadOnlySpan_Char_TypeName = $"{ReadOnlySpan_TypeName}<char>";

    public const string MemoryExtensions_TypeName = "System.MemoryExtensions";
    public const string CollectionsMarshal_TypeName = "System.Runtime.InteropServices.CollectionsMarshal";

    public const string AsSpan_Identifier = "AsSpan";

    public const string Dictionary_TypeName = "System.Collections.Generic.Dictionary";
    public const string GeneratedCodeAttribute_TypeName = "System.CodeDom.Compiler.GeneratedCodeAttribute";

    public const string PartialMethodDeclarationHaveSignatureDifferences_ErrorCode = "CS8826";
    public const string LabelNotBeenReferenced_ErrorCode = "CS0164";
    public const string PossibleNullReferenceArgumentForParameter_ErrorCode = "CS8604";

    public const string Global = "global";
}
