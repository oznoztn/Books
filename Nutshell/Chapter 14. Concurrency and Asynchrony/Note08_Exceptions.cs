using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nutshell.Chapter_14._Concurrency_and_Asynchrony
{
    /// <summary>
    /// Note #8 - Exceptions
    /// </summary>
    public class Note08_Exceptions
    {
        public void Note8_Exceptions()
        {
            try
            {
                // Thread oluşturulurken devrede olan try/catch mekanizması, 
                // thread işini yaparken devrede olmaz.

                // Olası hatalar thread execution'unun başladığı noktada kontrol altına alınmalıdır.

                var t = new Thread(ThrowsException);
                t.Start();
                t.Join();

                // Her bir thread'in birbirinden farklı
                //  birer execution path'a sahip olduğunu göz önünde tutarsak bu gayet mantıklı.
            }
            catch (Exception e)
            {
                // Hiç bir zaman burası çalışmayacak.
                throw;
            }
        }

        void ThrowsException()
        {
            // try/catch bloğunun burada olması gerekir
            throw new ArgumentException();
        }
    }
}
