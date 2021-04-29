using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Semaphores
{
    class Program
    {
        public static Random random;
        public static Semaphore semaphore;

        public static void WatchingMovies()
        {
            semaphore.WaitOne();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Cliente {Thread.CurrentThread.Name} entra al teatro.");
            Console.ResetColor();
            Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 5)));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Cliente {Thread.CurrentThread.Name} sale del teatro.");
            Console.ResetColor();
            semaphore.Release();
        }

        static void Main(string[] args)
        {
            random = new Random();
            semaphore = new Semaphore(3, 3);

            for (int i = 1; i <= 10; i++)
            {
                Thread thread = new Thread(() => WatchingMovies());
                thread.Name = i.ToString();
                thread.Start();
            }
        }
    }
}
