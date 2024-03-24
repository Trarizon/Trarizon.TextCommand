using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trarizon.TextCommand.Attributes;

namespace Trarizon.TextCommand.Tester;
internal partial class _Design
{
    [Executor("multi", "mark", "no", "param")]
    [Executor("no-param")]
    public string NoParam()
    {
        Console.WriteLine("NoParam");
        return default!;

    }
}

