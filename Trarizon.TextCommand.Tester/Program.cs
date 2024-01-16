// See https://aka.ms/new-console-template for more information
using Trarizon.TextCommand.Tester;

Console.WriteLine("Hello, World!");

var design = new _Design();

design.Run("/ghoti no-param");
design.Run("/ghoti default settings --flag --option a --number 114");
design.Run(@"/ghoti explicit parameter type -f A -nf true B ""string with space and """" escape """" "" 1 2 3 5 4 7 8");
design.Run("/ghoti custom --custom value --strFlag");