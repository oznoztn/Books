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

    }
}
