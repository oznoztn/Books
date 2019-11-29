using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nutshell.Chapter_14._Concurrency_and_Asynchrony
{
    public class TaskNotes
    {
        #region # 2.1 - Thread Pooling
        public void Note1_ThreadPooling()
        {
            /*
             * Thread Pooling
             * 
             * Thread Pool Nedir?
             * Thread Pool önceden oluşturulmuş (precreated) ve
             *  geridönüştürülebilir (recycable) thread'ler barındıran havuza denir.
             * 
             * 
             * Neden var?
             * Yeni bir thread oluşturulurken bazı masraflar oluşur. 
             * Örneğin oluşturulan thread'e 
             *  kendi local değişkenleri için local stack tahsis etmek
             *  ve diğer thread yönetim masrafları gibi...
             * TP, önceden oluşturulmuş ve geri--dönüştürülebilir thread'ler ile bu masrafların önüne geçer.
             * 
             * TP ile kısa sürecek bir işlem için thread yaratarak
             *  bunca masrafa katlanmak zorunda kalmayız.
             * 
             * Bir diğer işlevi de CPU oversubscription durumunun önüne geçmektir.
             * Aktif thread sayısının CPU'daki core sayısından fazla olması
             *   dolayısıyla OS'in timeslicing yapmak zorunda olması durumudur.
             *   
             * Oversubsciption'un performans üzerinde negatif bir etkisi vardır 
             *   çünkü timeslicing işlemi context-switching'i gerektirir 
             *     ve bu oldukça külfetli bir işlemdir.
             *     
             * CLR'ın Thread Pool'da oversubscription oluşmasını önlemek için belirli bir stratejisi vardır
             * Bu stratejinin en iyi şekilde uygulanabilmesi için
             *  1. TP'deki thread'lerin işlemleri kısa süreli olmalı 
             *     (ideal olarak <100 ms, max <250 ms olmalıdır)
             *  2. Zamanının çoğunu bloklanmış şekilde geçiren işlemler thread pool'u domine etmemeli!
             *  
             *  Pooled Thread'ler 'daima' Background Thread'dirler.
             */
        }
        #endregion

        #region # 2.2 - Tasks basics
        public void Note2_TaskBasics()
        {
            // HOT TASK: Otomatik olarak çalışmaya başlayan task tipidir.
            // COLD TASK: Otomatik çalışmaya başlamazlar. Nadiren kullanılırlar.

            Action writeHelloAction = () => Console.WriteLine("Greetings from the hot one!");
            Task hotTask = Task.Run(writeHelloAction);

            Task frozenTask = new Task(() => Console.WriteLine("Greetings from the cold one!"));
            frozenTask.Start();

            // Default olarak CLR taskları thread pool'da çalıştırır.
            // TP'deki threadler BACKGROUND T. olduklarına göre
            // Main thread sonlandığında bütün task'lar kapatılacaktır.
            // Bunu engellemek adına main thread'i işlem bitene dek bloklaman gerekir.

            Task blockingTask = Task.Run(() =>
            {
                // simulates a cpu-intensive operation ...
                Thread.Sleep(4000);
                Console.WriteLine("Foo");
            });
            Console.WriteLine(blockingTask.IsCompleted); // False
            blockingTask.Wait(); // Blocks until task is complete

            // Tasks should perform short-term operations
            //  and should NOT block the thread (that runs that shit son)
            //      Üstteki örnekteki gibi olmamalı yani.
            // Suppose you have a task of cpu-bound operation
            //  and you don't want it to use a pooled thread
            //      Then pass TaskCreationOptions.LongRunning ...
            Task longRunninTask =
                Task.Factory.StartNew(() => {/*do sth*/}, TaskCreationOptions.LongRunning);

            // (s. 583)
            // TP'de tek bir tane uzun işlem yapmak sıkıntı oluşturmaz
            // Fakat uzun süren fazla işlem performansa büyük sıkıntı oluşturur. 

            // In general, there're way better methods than using TaskCreationOptions.LongRunning:
            //   If the tasks are I/O-bound, use TaskCompletionSource and asynchronous functions ..
            //   If the tasks are compute-bound, use a producer/consumer queue ..
        }
        #endregion

        #region # 2.3 - Returning a value from a Task
        public void Note3_ReturningAValueFromATask()
        {
            // Değer dönderebilen task için Task<TResult> jenerik sınıfı bulunur
            // Task.Run()'dan, Task<TResult> alabilmek için 
            //   Task.Run() metodunu Func<TResult> ile çağırmalısın.

            Func<int> mathFunc = () =>
            {
                Thread.Sleep(2500);
                return 6;
            };

            Task<int> task = Task.Run(mathFunc);

            // Result property'sini sorgulayarak sonuç elde edilebilir.
            // Eğer sonuç henüz hesaplanmamışsa, 
            //   Result'a erişim mevcut thread'i bloklayacaktır.

            if (task.Status == TaskStatus.Running)
                Console.WriteLine("Task is running");

            int result = task.Result;
            Console.WriteLine(result);

            // Task<T> ye içerisinde gelecek zamanda kullanıma uygun olacak
            // bir Result barındıran "future" denebilir.
        }
        #endregion

        #region # 2.4 - Tasks and throwing exceptions
        public void Note4_Exceptions()
        {
            // Thread'in aksine Task kendisine verilen iş exception fırlattığında bu exception'ı 
            //  Wait() metodunu çağırana veya Result property'sine erişmek isteyene dönderir

            // Wait metodu veya Result property'si ile etkileşime girilmezse exception ses çıkarmaz,
            //   yani hiç bir şey olmamış gibi program çalışmaya devam eder.

            // Bu tip task'lara OTONOM TASK denir (set-and-forget)      

            // Otonom tasklarda meydana gelen unhandled exception'lara ise UNOBSERVED EXCEPTION denir.

            // Otonom tasklara verilen işlemlerin exception fırlatma olasılığı varsa
            //   thread de olduğu gibi task kodunu try/catch bloğu içine almakta fayda var.
            //   Aksi halde sessiz kalarak zamanında tespit edilemeyecek olan bu exception
            //     programın mevcut state'ini bozarak (can cause the app to fall to an invalid state), 
            //     programın ileri safhasında yeni hatalara sebebiyet verebilir
            //       Tüm bu olan biten bug teşhisini zorlaştıracaktır.

            // CLR wraps the exception in an AggregateException.
            try
            {
                Func<int> func = ThrowException; // defining an action to pass in a task's ctor

                Task<int> task = Task.Run(func);

                // accessing Result property propagates an exception, if any, to caller 
                int r = task.Result;

                Console.ReadLine();
            }
            catch (AggregateException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        int ThrowException()
        {
            Console.WriteLine("slam");
            throw new ArgumentException();
        }


        #endregion
    }
}
