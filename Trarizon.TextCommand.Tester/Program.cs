// See https://aka.ms/new-console-template for more information
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Trarizon.TextCommand.Parsers;
using Trarizon.TextCommand.Tester;

_ = new Tu() switch {
[1, 2, 3, .. var rest] => throw new Exception(),
};


Console.WriteLine("Hello, World!");

var design = new _Design();

design.Run("/ghoti no-param");
Console.WriteLine();
design.Run("/ghoti multi mark no param");
Console.WriteLine();
design.Run("/ghoti value-only 1", "1919810");
Console.WriteLine();
design.Run("/ghoti default settings --flag --option a --number 114");
Console.WriteLine();
design.Run("/ghoti multi marked --flag --str string --option a --nullNumber 114");
Console.WriteLine();
design.Run("""
    /ghoti implicit-conversion
    1 str 2 3 4
    --nullable 1
    --nullableString str
    --intToA 2
    --intToLong 3
    --intToNullableA 4
    """);
Console.WriteLine();
design.Run("/ghoti implicit-conversion");
Console.WriteLine();
design.Run("""
    /ghoti explicit parameter type 
    -f str A 
    --non-flag true B 
    1  2 3 
    "string with space and "" escape "" " "str2" 
    1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16
    """);
Console.WriteLine();
design.Run("""
    /ghoti custom
    --custom customParserVal
    --strFlag
    --intFlag
    --customParser custom
    type
    v1 v2 v3 v4
    """);
Console.WriteLine();
Console.WriteLine("_err");
design.Run("""
    /ghoti custom
    --custom customParserVal
    --strFlag
    --intFlag
    type
    v1 v2 v3 v4
    """);

class B
{
    public int Length { get; }
}

class Tu : B
{
    public int this[int index] => index;

    public ReadOnlySpan<char> Slice(int startIndex, int length) => default;
}
