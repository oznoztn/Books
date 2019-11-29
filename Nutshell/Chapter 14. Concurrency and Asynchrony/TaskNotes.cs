using System;
using System.Collections.Generic;
using System.Text;

namespace Nutshell.Chapter_14._Concurrency_and_Asynchrony
{
    public class TaskNotes
    {
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
    }
}
