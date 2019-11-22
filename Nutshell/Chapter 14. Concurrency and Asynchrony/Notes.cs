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
    }
}
