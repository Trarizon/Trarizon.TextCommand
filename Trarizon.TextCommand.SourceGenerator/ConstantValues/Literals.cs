namespace Trarizon.TextCommand.SourceGenerator.ConstantValues;
internal static class Literals
{
    public const string Namespace = "Trarizon.TextCommand";
    public const string GeneratorNamespace = $"{Namespace}.SourceGenerator";
    public const string Version = "0.0.2";

    // Attributes

    public const string ExecutionAttribute_TypeName = $"{Namespace}.Attributes.ExecutionAttribute";
    public const string ExecutorAttribute_TypeName = $"{Namespace}.Attributes.ExecutorAttribute";

    public const string FlagAttribute_TypeName = $"{Namespace}.Attributes.Parameters.FlagAttribute";
    public const string OptionAttribute_TypeName = $"{Namespace}.Attributes.Parameters.OptionAttribute";
    public const string ValueAttribute_TypeName = $"{Namespace}.Attributes.Parameters.ValueAttribute";
    public const string MultiValueAttribute_TypeName = $"{Namespace}.Attributes.Parameters.MultiValueAttribute";

    public const int ExecutionAttribute_CommandName_CtorParameterIndex = 0;
    public const string ExecutionAttribute_ErrorHandler_PropertyIdentifier = "ErrorHandler";
    public const int ExecutorAttribute_CommandPrefixes_CtorParameterIndex = 0;
    public const string ParameterAttribute_Parser_PropertyIdentifier = "Parser";
    public const string ParameterAttribute_ParserType_PropertyIdentifier = "ParserType";
    public const int FlagAttribute_Alias_CtorParameterIndex = 0;
    public const string FlagAttribute_Name_PropertyIdentifier = "Name";
    public const int OptionAttribute_Alias_CtorParameterIndex = 0;
    public const string OptionAttribute_Name_PropertyIdentifier = "Name";
    public const string OptionAttribute_Required_PropertyIdentifier = "Required";
    public const string ValueAttribute_Required_PropertyIdentifier = "Required";
    public const int MultiValueAttribute_MaxCount_CtorParameterIndex = 0;
    public const string MultiValueAttribute_Required_PropertyIdentifier = "Required";

    // Input

    public const string ParsingContext_TypeName = $"{Namespace}.Input.ParsingContext";
    public const string StringInputMatcher_TypeName = $"{Namespace}.Input.StringInputMatcher";
    public const string InputArg_TypeName = $"{Namespace}.Input.InputArg";

    public const string ParsingContext_Parse_MethodIdentifier = "Parse";
    public const string ArgsProvider_GetFlag_MethodIdentifier = "GetFlag";
    public const string ArgsProvider_GetOption_MethodIdentifier = "GetOption";
    public const string ArgsProvider_GetValue_MethodIdentifier = "GetValue";
    public const string ArgsProvider_GetValuesUnmanaged_MethodIdentifier = "GetValuesUnmanaged";
    public const string ArgsProvider_GetValuesArray_MethodIdentifier = "GetValuesArray";
    public const string ArgsProvider_GetValuesList_MethodIdentifier = "GetValuesList";
    public const string ArgsProvider_GetAvailableArrayLength_MethodIdentifier = "GetAvailableArrayLength";

    // Input.Result

    public const string ArgParsingErrors_TypeName = $"{Namespace}.Input.Result.ArgParsingErrors";
    public const string ArgParsingErrorsBuilder_TypeName = $"{Namespace}.Input.Result.ArgParsingErrors.Builder";
    public const string ArgResultKind_TypeName = $"{Namespace}.Input.Result.ArgResultKind";
    public const string ArgResult_TypeName = $"{Namespace}.Input.Result.ArgResult";
    public const string ArgRawResultInfo_TypeName = $"{Namespace}.Input.Result.ArgRawResultInfo";

    public const string ArgParsingErrorsBuilder_HasError_PropertyIdentifier = "HasError";
    public const string ArgParsingErrorsBuilder_AddWhenError_MethodIdentifier = "AddWhenError";
    public const string ArgParsingErrorsBuilder_DefaultErrorHandler_MethodIdentifier = "DefaultErrorHandler";
    public const string ArgParsingErrorsBuilder_Build_MethodIdentifier = "Build";
    public const string ArgResult_Value_PropertyIdentifier = "Value";
    public const string ArgResults_Values_PropertyIdentifier = "Values";

    public const string ArgResultKind_ParameterNotSet_FieldName = "ParameterNotSet";
    public const string ArgResultKind_ParsingFailed_FieldName = "ParsingFailed";

    // Parsers

    public const string IArgParser_TypeName = $"{Namespace}.Parsers.IArgParser";
    public const string IArgFlagParser_TypeName = $"{Namespace}.Parsers.IArgFlagParser";

    public const string BooleanFlagParser_TypeName = $"{Namespace}.Parsers.BooleanFlagParser";
    public const string EnumParser_TypeName = $"{Namespace}.Parsers.EnumParser";
    public const string ParsableParser_TypeName = $"{Namespace}.Parsers.ParsableParser";
    public const string DelegateParser_TypeName = $"{Namespace}.Parsers.DelegateParser";
    public const string DelegateFlagParser_TypeName = $"{Namespace}.Parsers.DelegateFlagParser";

    // Parsers.Wrappers

    public const string NullableParser_TypeName = $"{Namespace}.Parsers.Wrapped.NullableParser";
    public const string ConversionParser_TypeName = $"{Namespace}.Parsers.Wrapped.ConversionParser";

    // Vars

    public const string StringInputMatcher_VarIdentifier = "__matcher";
    public const string ParsingContextProvider_TypeIdentifier = "__ParsingContextProvider";
    public const string Input_ParameterIdentifier = "input";
    public const int StackAllocThreshold = 128; // As this lib is mainly design for parsing manually input, we don't need a large threshold

    public const string Prefix_Alias = "-";
    public const string Prefix = "--";
}
