using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nutshell.Chapter_14._Concurrency_and_Asynchrony
{
    public class AsynchronyNotes
    {
        // Note # 3.1: Call Graph
        //      Since the DisplayPrimeCounts method calls the GetPrimesCount method
        //          there's a call graph:
        public int GetPrimesCount(int start, int count)
        {
            // PLINQ, I guess.
            return ParallelEnumerable.Range(start, count)
                .Count(n =>
                    Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0));
        }

        // Note # 3.2:
        // Coarse-grained concurrency
        //  The GetPrimesCount operation is already being executed on parallel.
        //  Therefore we don't need to use the Task mechanism actually.
        public void DisplayPrimeCountsCoarse()
        {
            // Bütün call-graph'i Task'a veriyorum
            //   Bu yüzden coarse-grained...
            Task.Run(DisplayPrimeCounts);
        }

        public void DisplayPrimeCounts()
        {
            for (int i = 0; i < 10; i++)
                Console.WriteLine(
                    GetPrimesCount(i * 1000000 + 2, 1000000) +
                    " primes between " + (i * 1000000) +
                    " and " + ((i + 1) * 1000000 - 1));

            Console.WriteLine("Done!");
        }

        // Fine-grained concurrency
        // I have to rewrite the GetResult method for that end:
        Task<int> GetPrimesCountAsync(int start, int count)
        {
            return Task.Run(() =>
                ParallelEnumerable.Range(start, count)
                    .Count(n =>
                        Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0)));
        }
    }
}
