using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nutshell.Chapter_14._Concurrency_and_Asynchrony
{
    public class Notes
    {
        #region Note #1 - Creating a new thread
        public void Note1_ThreadCreation()
        {
            /*
             * The simplest way of creating a new thread is passing ThreadStart delegate to the Thread's constructor.
             * ThreadStart delegate points to a method takes no argument.
             * The delegate indicates where the execution should begin for the thread.
             *
             * Calling its Start method starts the thread and the method that runs ON that thread.
             */

            Thread thread = new Thread(() =>
            {
                for (int i = 0; i < 500; i++)
                {
                    Console.Write("x");
                    Thread.Sleep(new Random(3).Next(0, 1000));
                }
            });

            // Start() runs the thread
            thread.Start();

            // Do something on main thread concurrently:
            for (int i = 0; i < 500; i++)
            {
                Console.Write("y");
                Thread.Sleep(new Random(1).Next(0, 1000));
            }

            // xxxxxxxxxxyyyyyyyyyy şeklinde olmasının nedeni Context-switching ...
        }
        #endregion

        #region Note #2 - IsAlive, Name, Thread.CurrentThread
        public void Note2()
        {
            // 1)
            // Once it's started, a thread's IsAlive property returns True, 
            // until the thread reaches the point at which its execution ends.

            Thread secondaryThread = new Thread(() =>
            {
                Console.WriteLine("Greetings from secondaryThread.");
                Thread.Sleep(50);
            });

            bool state = secondaryThread.IsAlive; // False

            // 2)
            // A thread ends when the passed delegate finishes executing. 
            // So, IsAlive property returns False.

            // 3)
            // Once a thread ended, you can not re-start.
            // So, finishedThread.Start() will throw an exception.

            // 4)
            // You can name a thread. It is useful for debugging in VS.
            // But you can set its name just for once.
            // An attempt to re-name it will throw an exception.
            secondaryThread.Name = "Secondary Thread";

            // 5) Thread.CurrentThread returns the thread executing the code
            Thread.CurrentThread.Name = "Main Thread";

            secondaryThread.Start();

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("M");
            }
        }
        #endregion

        #region Note #3 - Join, Sleep, Yield
        public void Note3_Join_Sleep_Yield()
        {
            // Join
            // Bir thread'in, başka bir thread'in işini bitirmesini beklemesi
            //   Thread'in Join metodunu çağırarak, thread'in sonlanmasını/işini bitirmesini bekleyebilirim.
            //    Join metoduna timeout süresini belirtebilmek için TimeSpan objesi verilebilir.
            //    Thread sonlanırsa True, zamanaşımına uğrarsa False dönderir.

            // Sleep(n)
            // Pauses a thread 

            // Sleep(0)
            // Söz konusu thread kendisine tahsis edilen cpu time slice'ından 
            // (anında ve gönüllü olarak) 
            // feragat eder ki CPU bu süreyi diğer thread'lere verebilsin.

            // Yield()
            // Sleep(0) ile aynı şeyi yapar. 
            // Farkı, Yield() yalnızca aynı processor üzerindeki thread'ler için kendi CPU time'ından feragat eder.

            // NOT: Yield()
            //      Eğer bu kodu programın herhangi bir kısmında çalıştırmak, 
            //      application'ı patlatıyorsa
            //      neredeyse kesinlikle application'da bug var demektir.

            Thread thread = new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Console.WriteLine("_");
                }
            });

            thread.Start();
            thread.Join(); // Who is waiting? Main Thread.

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("#");
            }

            // With Join   : ______________________################
            // Without Join: ___________#######_________#########__

            // Join veya Sleep ile bekletilen bir thread "bloklanmış" olur.
        }
        #endregion

        #region Note #4 - On I/O & Compute Bound operations
        /* Temelde iki tip işlem tipi vardır:
         *  1) I/O Bound 
         *      Zamanının büyük bir kısmını bir şeyin gerçekleşmesini beklemeye harcayan operasyonlar:
         *        En basit örnek Console.ReadLine.
         *        Ayrıca Thread.Sleep() veya .Join() de bu kategoriye girer.
         *        
         *     Bu tip bir işlem iki farklı şekilde çalışır.
         *       1) Senkronize: 
         *            Mevcut thread üzerinde, işlemin bitmesini bekler. 
         *            Senkronize çalışan bir işlem bekleme yaparken mevcut thread'i bloklar.       
         *            
         *       2) A-Senkronize: 
         *          İşlem bittiğinde verilen callback fonksiyonunu çalıştırır.
         *        
         *  2) Compute Bound
         *      Adı üstünde.
         */
        #endregion

        #region Note #5 - On Blocking and Spinning
        public void Note5_Blocking_Spinning()
        {
            /*
             *          BLOCKING of a thread
             * Çalışması herhangi bir nedenden ötürü duran thread bloklanmış addedilir.
             * 
             * Örneğin:
             *  Sleep ile duraklatılmış,
             *  Join ile bir başka thread'in işini bitirmesini bekleyen bir thread.
             * 
             * Bloklanan thread CPU time'ını başkaları için bırakır (yield), 
             * ve o andan itibaren CPU time tüketmemeye başlar.
             * Ta ki bloklama kalkana kadar.
             * 
             * Thread BLOCKING ve UNBLOCKING esnasında OS context switching denilen işlemi gerçekleştirir.
             * Bu işlemin, genelde 1-2 ms'lik, göreceli olarak ufak, bir masrafı vardır.
             * 
             * WIKI:
             * In computing, a context switch is the process 
             * of storing and restoring the state (more specifically, the execution context) 
             * of a process or thread so that execution can be resumed from the same point at a later time. 
             * This enables multiple processes to share a single CPU 
             * and is an essential feature of a multitasking operating system.
             */

            DateTime nextStartDate = DateTime.Now.AddSeconds(30);

            // BLOCKING
            Thread blockingThread = new Thread(() =>
            {
                while (DateTime.Now < nextStartDate)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }

                Console.WriteLine("the blocking thread has run");
            });

            // SPINNING
            Thread spinningThread = new Thread(() =>
            {
                while (DateTime.Now < nextStartDate)
                {
                }

                Console.WriteLine("the spinning thread has run");
            });

            /*
             * Spinning'in kısa süreceği senaryolarda, birkaç ms, spinning tekniği faydalıdır.
             * Çünkü context switching işlemi maliyetlidir.
             * 
             * Spinning, why?
             *   Spinning is useful because blocking a thread is not FREE.
             *     In a scenario of heavy I/O-bound computing, blocking a thread will definitely incur performance penalty.
             */

            blockingThread.Start();
            spinningThread.Start();
        }
        #endregion

        #region Note #6 - On captured values and lambda expressions
        public void Note6_Lamb()
        {
            for (int i = 0; i < 10; i++)
                new Thread(() => Console.WriteLine(i)).Start();

            Console.WriteLine();

            // As long as the loop is alive, the variable i will point to the same location in the memory.
            // Therefore, in a multi thread scenario, the thread that loops through this loop is to work with the same variable i.
            // As a result, the outcome is going to be indeterministic in this case.
            // To prevent this, copy the i into a new local variable.
            // By doing this you let the new variable live in the local stack of the very thread that executes the loop.
            for (int i = 0; i < 100; i++)
            {
                int copy = i;
                new Thread(() => { Console.WriteLine(copy); }).Start();
            }
        }
        #endregion

        #region Note #7 - Foreground and Background Threads
        public void Note7_ForegroundBackgroundThreads(string[] args)
        {
            // Oluşturulan her bir thread, default olarak, Foreground Thread'dir. 
            // Main thread bir BACKGROUND THREAD'dir.

            // Bir FT, çalıştığı müddetçe, programı canlı tutar.
            // Yani FT'ler execution'ını bitirmeden program sonlanmaz.
            //    Main Thread sonlanmış olsa dahi.

            // FT'lerin çalışması (execution) sonlandığında application sonlanır.
            // O anda çalışan BT'ler (varsa) ansızın (abruptly) kapatılırlar.

            // Prosesin bu şekilde sonlanması sonucu Background Thread'lerin birden kapatılması
            // varsa finally bloklarının atlanmasına sebebiyet verir.
            // Eğer finally bloğunda bazı temizlik işlemleri yapılıyorsa bu durum sıkıntı oluşturacaktır.

            // BT lerin sonlanmasını bekleyerek böyle bir durumun oluşması önlenebilir:
            // If you’ve created the thread yourself, call Join on the thread (or use other signaling contructs).
            // If you’re on a pooled thread, use an event wait handle.

            // Thread durumunu sorgulamak veya değiştirmek için IsBackround kullanılır.

            Thread worker = new Thread(() => Console.ReadLine());

            if (args.Length > 0)
                worker.IsBackground = true;

            worker.Start();

            if (!Thread.CurrentThread.IsAlive)
                Console.WriteLine("Main Thread (Background Thread) is dead.");
        }
        #endregion

        #region Note #9 - The Local State
        class Note9_LocalState
        {
            public Note9_LocalState()
            {
                // CLR her bir thread'e ayrı memory stack'ı tahsis eder. 
                // Dolayısıyla lokal değişkenler birbirlerinden ayrı stack'da bulunacaklarından
                // lokal değişkenler ortaklaşa kullanılamazlar.

                // Local değişkenler birbirlerinden ayrı olduğundan ekrana yazılan toplam "M" harfi 2+2=4            

                new Thread(Go).Start(); // Yeni bir thread Go metodunu çağırıyor
                Go(); // Main thread Go metodunu çağırıyor
            }

            static void Go()
            {
                // 'cycles' lokal değişkeni ...
                for (int cycles = 0; cycles < 2; cycles++)
                    Console.Write('M');
            }
        }
        #endregion

        #region Note #10 - Shared State A - The Thread Unsafety
        class Note10_SharedStateA_ThreadUnsafety
        {
            public Note10_SharedStateA_ThreadUnsafety()
            {
                // Threads share data if they have a common reference to the same object instance:

                // Alttaki örnekte main thread ve yeni oluşturulan thread,
                // aynı ThreadTest instance'ı üzerindeki Go metodunu çağırdıklarından, 
                // _done değişkenini paylaşacaklar.

                // Dolayısıyla bir önceki örnekte olduğu gibi iki tane "Done!" çıktısı almayacağız.

                SharedClass sharedClass = new SharedClass();

                new Thread(sharedClass.Go).Start(); // a newly created thread
                sharedClass.Go(); // Main thread
            }

            internal class SharedClass
            {
                bool _done;

                // Statik metot olmadığına dikkat et.
                public void Go()
                {
                    if (!_done)
                    {
                        _done = true;
                        Console.WriteLine("Done!");
                    }
                }
            }
        }
        #endregion

        #region Note #11 - Shared State B - More on Thread Unsafety 
        class Note11_SharedStateB_MoreOnThreadUnsafety
        {
            // Static fields are shared between the threads in the application domain.
            static bool _done;

            public Note11_SharedStateB_MoreOnThreadUnsafety()
            {
                new Thread(Go).Start();
                Go();
            }

            public void Go()
            {
                if (!_done)
                {
                    // I. Durum
                    // Bu işlem ilk başta yapıldığında çift yazılma olasılığı DÜŞÜK,
                    _done = true;
                    Console.WriteLine("Done!");

                    // II. Durum
                    // Çift yazılma olasılığı çok daha YÜKSEK olur
                    Console.WriteLine("Done!");
                    _done = true;

                    // What is the rationale behind the problem?
                    //  One of the thread may evaluate the if statement while the other one is setting the variable _done true.
                    //  This introduces the possibility that is more than one thread is already in the if block
                    //    which eventually defeats the purpose of the if control statement.

                    // I. durumda bunun olasılığı daha düşük olmasının nedeni bu. 
                    // Ama her türlü bir olasılık var ve bu göz ardı edilemez.

                    // This uncertainty makes the code "thread unsafe".

                    // REMEMBER: Singleton classes are not thread safe by default.
                }
            }
        }
        #endregion

        #region Note #12 - Shared State C - Obtaining An Exclusive Lock
        class Note12_SharedStateC_ObtainingAnExclusiveLock
        {
            static bool _done;
            static readonly object Locker = new object();

            public Note12_SharedStateC_ObtainingAnExclusiveLock()
            {
                new Thread(Go).Start(); // calls Go() on a newly created thread
                Go(); // calls Go() on the main thread
            }

            static void Go()
            {
                // The remedy is to obtain an exclusive lock while reading and writing to the common field. 
                // C# provides the lock statement for just this purpose:

                // The mechanism is used to make the critical shared state/data accessible
                //   only' by one thread at the same time.

                // Multithread bir ortamda kodun bu şekilde korunmasıyla 
                // önceki örnekteki kod davranışındaki belirsizliğin kaldırılması 
                // (birden fazla Done yazılabilme olasılığının ortadan kaldırılması)
                // kodun THREAD-SAFE olduğu anlamına gelir.

                // lock mekanizması silver-bullet bir teknik değildir.
                // Locklama işlemi düzgün yapılmadığında deadlock'a neden olabilir.
                // lock'lama işleminin kendisi başlı başına sorun oluşturabilir.
                // Bundan dolayı lock-free algoritmalar önem kazanmaktadır.

                lock (Locker)
                {
                    if (!_done)
                    {
                        _done = true;
                        Console.WriteLine("Done");
                    }
                }
            }
        }
        #endregion

        #region Note #13 - Passing Data to Threads
        class Note13_PassingDataToThreads
        {
            public void PassingDataLambda()
            {
                // Lambda ifadelerini kullanarak
                Thread thread = new Thread(() => Print("M"));
                thread.Start();


                Thread thread2 = new Thread(() => Print("G", 1));
                thread2.Start();
            }

            public void PassingDataOldSchool()
            {
                // Eski yol, lambda ifadeler henüz tanıtılmamışken...

                // thread.Start() metodunun object alan bir overload'u mevcut ...

                // Dezevantajı birden fazla argüman verememek
                // Object tipinde argüman aldığından, çoğu zaman cast gerekir 

                Thread oldSchoolThread = new Thread(Print);
                oldSchoolThread.Start("M G E ");
            }

            public void Print(string message)
            {
                Console.WriteLine(message);
            }

            public void Print(string message, int t)
            {
                for (int i = 0; i < t; i++)
                {
                    Console.WriteLine(message);
                }
            }

            public void Print(object obj)
            {
                string message = (string)obj;

                Console.WriteLine(message);
            }
        }
        #endregion
    }
}
