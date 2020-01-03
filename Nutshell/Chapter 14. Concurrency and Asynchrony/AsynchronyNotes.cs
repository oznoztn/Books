using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nutshell.Chapter_14._Concurrency_and_Asynchrony
{
    public class AsynchronyNotes
    {
        #region Note # 3.1: Call Graph
        // Since the DisplayPrimeCounts method calls the GetPrimesCount method
        //  there's a call graph:
        public int GetPrimesCount(int start, int count)
        {
            // PLINQ, I guess.
            return ParallelEnumerable.Range(start, count)
                .Count(n =>
                    Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0));
        }
        #endregion

        #region Note # 3.2: Coarse-grained Concurrency
        // Coarse-grained concurrency
        //  The GetPrimesCount operation is already being executed on parallel.
        //  Therefore we don't need to use the Task mechanism actually.
        public void DisplayPrimeCountsCoarse()
        {
            // Bütün call-graph'i Task'a veriyorum
            //   Bu yüzden coarse-grained...
            Task.Run(DisplayPrimeCounts);
        }
        #endregion

        #region Note # 3.3: Fine-grained Concurrency
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

        // Async metodu çağıran bir metot yazmam gerek:
        //   Burada C#'daki await ve async keyword'ları devreye giriyor.
        //   Bu keyword'leri kullanmadan düzgün sonuç elde etmek göründüğü gibi kolay değil
        /*
         * Fine-grained concurrency örneğini tamamlamak adına,
         *  async metodu çağıran yeni bir print metodu yazmam gerek
         * Burada C#'daki await ve async keyword'ları devreye giriyor.
         * Bu keyword'leri kullanmadan düzgün sonuç elde etmek göründüğü gibi kolay değil.
         * 
         * Sorun 1.
         * Alttaki fonksiyondaki ilk sorun (Thread bloklama olmadığından)
         *  for döngüsü anında çalışmayı bitirecek
         *
         *   Dikkat et!
         *      Hesaplama paralelde gerçekleşirken for döngüsü sonlanıyor, 
         *          dolayısıyla zamansız bir "Done!" çıktısı elde edeceksin. 
         *     
         *   İkinci sorun output belirsiz bir şekilde yazılacak. 
         *   Bu da çok doğal çünkü, 
         *      paralelde gerçekleşen işlemlerden 
         *          işini bitiren continuation'unu çalıştıracak (printing the result).
         *
         *      Tipik bir sonuç:
         *        Done!
         *        70435 primes between 1000000 and 1999999
         *        78498 primes between 0 and 999999
         *        67883 primes between 2000000 and 2999999
         *        63799 primes between 6000000 and 6999999 ...
         * 
         * Sorun 2. 
         * Print metodunun kendisini async yapmak başlı başına bir mesele..
         * Bunun içini TaskCompletionSource kullanmak gerek.
         */
        public void PrintResults_CallsAsync()
        {
            for (int i = 0; i < 10; i++)
            {
                var awaiter = GetPrimesCountAsync(i * 1000000 + 2, 1000000).GetAwaiter();
                var copy = i;
                awaiter.OnCompleted(() =>
                {
                    Console.WriteLine(awaiter.GetResult() +
                                      " primes between " + (copy * 1000000) +
                                      " and " + ((copy + 1) * 1000000 - 1));
                });
            }

            Console.WriteLine("Done!");
        }
        #endregion
    }
}
