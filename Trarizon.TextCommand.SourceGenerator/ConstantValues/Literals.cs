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
    public const int ExecutorAttribute_CommandPrefixes_CtorParameterIndex = 0;
    public const string ParameterAttribute_ParserPropertyIdentifier = "Parser";
    public const int FlagAttribute_Alias_CtorParameterIndex = 0;
    public const string FlagAttribute_Name_PropertyIdentifier = "Name";
    public const int OptionAttribute_Alias_CtorParameterIndex = 0;
    public const string OptionAttribute_Name_PropertyIdentifier = "Name";
    public const string OptionAttribute_Required_PropertyIdentifier = "Required";
    public const string ValueAttribute_Required_PropertyIdentifier = "Required";
    public const int MultiValueAttribute_MaxCount_CtorParameterIndex = 0;
    public const string MultiValueAttribute_Required_PropertyIdentifier = "Required";


    // Input

    public const string ParameterSet_TypeName = $"{Namespace}.Input.ParameterSet";
    public const string StringInputMatcher_TypeName = $"{Namespace}.Input.StringInputMatcher";

    public const string ParameterSet_Parse_MethodIdentifier = "Parse";
    public const string ArgsProvider_GetFlag_MethodIdentifier = "GetFlag";
    public const string ArgsProvider_GetOption_MethodIdentifier = "GetOption";
    public const string ArgsProvider_GetValue_MethodIdentifier = "GetValue";
    public const string ArgsProvider_GetValues_MethodIdentifier = "GetValues";
    public const string ArgsProvider_GetValuesArray_MethodIdentifier = "GetValuesArray";
    public const string ArgsProvider_GetValuesList_MethodIdentifier = "GetValuesList";
    public const string ArgsProvider_GetAvailableArrayLength_MethodIdentifier = "GetAvailableArrayLength";
   
    // Parsers

    public const string IArgParser_TypeName = $"{Namespace}.Parsers.IArgParser";
    public const string IArgFlagParser_TypeName = $"{Namespace}.Parsers.IArgFlagParser";

    public const string BooleanFlagParser_TypeName = $"{Namespace}.Parsers.BooleanFlagParser";
    public const string EnumParser_TypeName = $"{Namespace}.Parsers.EnumParser";
    public const string ParsableParser_TypeName = $"{Namespace}.Parsers.ParsableParser";
    public const string NullableParser_TypeName = $"{Namespace}.Parsers.NullableParser";
    public const string DelegateParser_TypeName = $"{Namespace}.Parsers.DelegateParser";
    public const string DelegateFlagParser_TypeName = $"{Namespace}.Parsers.DelegateFlagParser";

    // Vars

    public const string StringInputMatcher_VarIdentifier = "__matcher";
    public const string ParameterSets_TypeIdentifier = "__ParameterSets";
    public const string Input_ParameterIdentifier = "input";
    public const int StackAllocThreshold = 1024;

    public const string Prefix_Alias = "-";
    public const string Prefix = "--";
}
