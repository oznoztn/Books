using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nutshell.Chapter_14._Concurrency_and_Asynchrony
{
    public class AsynchronyNotes
    {
        // Note # 3.1: Call Graph
        //      Since the DisplayPrimeCounts method calls the GetPrimesCount method
        //          there's a call graph:
        int GetPrimesCount(int start, int count)
        {
            // PLINQ, I guess.
            return ParallelEnumerable.Range(start, count)
                .Count(n =>
                    Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0));
        }
    }
}
