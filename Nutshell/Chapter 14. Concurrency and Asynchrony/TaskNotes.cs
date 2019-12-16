using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        #region # 2.5 - Awaiters
        public void Note5_Awaiters()
        {
            // Continuation, task tamamlandığında (başarılı veya başarısız) çalışan callback metoda denir.
            Task<int> task = Task.Run(() =>
            {
                Thread.Sleep(2000);
                return 1;
            });

            // task.GetAwaiter() metodu 'awaiter' objesi dönderir.
            TaskAwaiter<int> awaiter = task.GetAwaiter();

            // awaiter objesinin OnCompleted property'sine
            // task tamamlandığında çalışacak callback (Action) set edilir.
            awaiter.OnCompleted(() =>
            {
                int result = awaiter.GetResult();
                Console.WriteLine($"Hesaplama tamamlandı. Sonuç: {result}");
            });

            // Sonucu elde etmenin iki yolu var: 
            //    TaskAwaiter üzerinde GetResult() metodunu çağırmak
            //    Task'daki Result property'sine erişmek.
            Console.WriteLine(task.Result);
            Console.WriteLine(awaiter.GetResult());



            Task<int> faultyTask = Task.Run(() =>
            {
                // Unobserved Exception
                throw new ArgumentException();
                return 6;
            });

            TaskAwaiter<int> awaiterf = faultyTask.GetAwaiter();
            awaiterf.OnCompleted(() =>
            {
                // Task kodunda exception oluşmuşsa, 
                // continuation kodu sonuca erişmek istediğinde
                //   exception tekrar fırlatılır (re-throw)

                // Sonucu elde etme şekline göre fırlatılan exception tipi değişir:
                //   GetResult() -> Exception direk fırlatılır
                //   Result -> Exception AggregateException içerisine alınır (wrapping)

                try
                {
                    // accessing the result via GetResult
                    int r = awaiterf.GetResult(); // AggregateException

                    // or via Result property
                    int r2 = task.Result; // ArgumentException

                }
                catch (AggregateException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
            });

            Console.ReadLine(); // blocking
        }
        #endregion

        #region # 2.6 - Task Completion Source (Example)
        class TaskCompletionSourceExample
        {
            /*
             * TaskCompletionSource
             * Belirli bir süre başlayacak ve yine belirli bir süre sonra son bulacak 
             *  her türlü operasyon için bir Task oluşturmaya olacak tanır.
             * 
             * TSC, tsc metotları ile manual olarak yönetilebilen bir 'köle' task verir.
             *          
             * TSC üzerindeki metotlar yalnızca bir kez çalıştırılmalıdır. 
             * Aksi halde SetX şeklindeki metotlar exception, 
             *  TryX şeklinde olanlar false dönderir.
             * 
             * TSC'nin I/O-BOUND tipinde olan uzun süren veya mevcut threadi bloklayan bir operasyonun bulunduğu 
             *  senaryoda kullanılması uygundur.
             *  
             * TSC'nin asıl gücü thread bağlı olmayan / thread kullanmayan 
             *  bir operasyonu temsil eden task oluşturabilmesidir:         *  
             *  
             *  Örneğin 3 saniye sonra çalışacak ve bir değer dönderecek bir operasyon düşün.
             *  TSC ile bu operasyonu herhangi bir thread'e bağlı kalmadan Timer ile gerçekleştirebilirim.
             *   Böylece hem thread oluşturma masrafı ortadan kalkmış olur
             *    hem de mevcut thread 3 sn bloklanmamış olur.
             *  
             */
            public TaskCompletionSourceExample()
            {
                Task<int> task = GetNumber();

                TaskAwaiter<int> awaiter = task.GetAwaiter();

                // continuation tanımlıyorum:
                // continuation tanımlayarak, 
                //   task sonucunu HİÇBİR THREADi bloklamadan yazdırıyorum:
                awaiter.OnCompleted(() => Console.WriteLine(awaiter.GetResult()));
            }

            Task<int> GetNumber()
            {
                TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

                var timer = new System.Timers.Timer(5000);
                timer.Elapsed += delegate
                {
                    timer.Dispose();
                    tcs.SetResult(72);
                };
                timer.Start();
                return tcs.Task;
            }
        }
        #endregion

        #region # 2.7 - On Async, I/O & CPU Boundness
        /*
         * async kod CLR tarafından state machine'e çevrilir. 
         * Task tamamlandığında execution'un kaldığı yerden devam etmesini sağlayan şey budur.
         * 
         * I/O Bound: Database hit, requesting a web page
         * 
         * Task veya Task<T> dönderen I/O Bound işlemi async function içerisinde 'await' ile bekleriz
         *
         * Compute Bound işlemler için Task.Run() dan dönen 'hot' taskı bekleriz, 'await'. 
         * Task.Run() ile başlatılan maliyetli işlem background thread'de yapılır.
         * Bu task thread-bound bir task'tır.
         * 
         * Hatırlarsan tasklar thread-bound olmak zorunda değillerdi.
         *  Çoğu senaryoda da thread-bound değillerdir zaten.
         *  
         * Bir işlemin I/O Bound ve CPU Bound olduğunu tespit etmek önemli. 
         *  Çünkü doğru yapı kullanıldığında çok büyük bir performans artışı elde edilecektir.
         * 
         * 
         * Herhangi bir kod yazmadan önce kendine şunu sor
         *      1) Kod bir şeyler bekliyor mu? 
         *          Veritabanıdan bir şey, bir web sorgusunun sonucu, konsol girdisi ..
         *              Eğer cevap EVET ise I/O Bound işlem
         *      2) Kod yoğun işlem gücü gerektiren maliyetli bir işlem mi gerçekleştiriyor?
         *          Eğer cevap EVET ise CPU Bound işlem
         *  
         * İşlem I/O Bound ise async ve await'i Task.Run()'sız kullan.
         *     Never ever use TPL!
         *  
         * İşlem CPU Bound ise Task.Run ile işlemi bir başka thread üzerinde gerçekleştir.
         * İşin doğasına göre TPL kullanılabilir.
         *      *
         * Performing a computationally expensive operation on a different thread with Task.Run instruction
         * does not mean that operation is going to be highly performant
         * It is all about the context!
         *
         * I mean, we already know that, creating a brand new thread is not a cheap operation
         * and the context-switching procedure on the other hand is pretty expensive especially in multi-threaded environment.
         * As a result, an attempt to harness a new thread via Task.Run may not pay the dividens
         *  and may even be a total disservice,
         *   keep that in mind.
         *
         * */

        #endregion

        #region # 2.8 - Genel Notlar
        /*
         * Asenkron olan metot isimleri Async ile bitmelidir.
         * 
         * async metotlar 'await' keyword'unu metot gövdesinde barındırmalı.
         *  Aksi halde bu kod asla 'yield' etmez. 
         *    async Task<int> Run() { Task.Delay(3000); return 1; }
         *      await yok, senkronize çalışacak.
         *  
         * 'async void' ifadesi yalnızca event handler'ları asenkron yapmak için kullanılmalı.
         * 
         * BLOKLAMA YAPMA
         * Task'ın işin bitirmesini bekleyen kod yazarken mevcut thread'i bloklama
         * Veya bloklama yapmadan Task'ın işini bitirmesini bekleyen kod yaz.
         * Dolayısıyla:
         * **************************************************
         * Background task sonucunu elde etmek isterken
         *      Task.Result veya Task.Wait YERİNE await
         * Herhangi bir Task'ın tamamlanmasını beklediğinde
         *      Task.WaitAny YERİNE await Task.WhenAny
         * Bütün Task'ların tamamlanmasını beklediğinde
         *      Task.WaitAll YERİNE await Task.WhenAll
         * Belirli bir süre beklemek istediğinde
         *      Thread.Sleep(n) yerine await Task.Delay(n)
         ****************************************************
         * 
         * Daha az 'stateful' kod yaz
         *  Global obje state'ine veya 
         *      bir metodun çalışıp/çalışmamasına bağımlı olmak yerine 
         *           metodun dönderdiği değere bağımlı ol.
         * 
         * Neden mi?
         *  Kodu anlaması daha kolay olur.
         *  Kodu test etmesi daha kolay olur.
         *  Async ile sync kodları beraber çalıştırmak ÇOK daha kolay olur.
         *  'Race Condition' sorunlarından tamamıyla kaçınılabilir.
         *  Dönen değere bağımlı olmak async fonksiyonları kordine etmeyi kolaylaştırır.
         *  Dependency Injection ile beraber çok iyi iş çıkarır.
         *
         */
        #endregion

    }
}
