using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Philosophers
{
    public enum StatesPhilosopher
    {
        thinking = 0,
        hungry = 1,
        eating = 2,
        finished_eat = 3
    }

    public class Philosopher
    {
        private Semaphore block;
        private Thread[] philosopherThread;
        private StatesPhilosopher[] statesPhilosopher;

        public int CountPhilosopher { get; private set; }

        public Philosopher(int countPhilosopher = 5)
        {
            block = new Semaphore(1, 1);
            philosopherThread = new Thread[countPhilosopher];
            statesPhilosopher = new StatesPhilosopher[countPhilosopher];
            CountPhilosopher = countPhilosopher;

            for (int i = 0; i < statesPhilosopher.Length; i++)
            {
                statesPhilosopher[i] = StatesPhilosopher.thinking;
            }
        }

        private void Eat(int id_philosopher)
        {
            Random random = new Random();

            while (true)
            {
                if (random.Next(0, 2) == 0)
                {
                    block.WaitOne();

                    statesPhilosopher[id_philosopher] = StatesPhilosopher.hungry;
                    Console.WriteLine($"[@] Filosofo {id_philosopher} hambriento.");

                    statesPhilosopher[id_philosopher] = StatesPhilosopher.eating;
                    Console.WriteLine($"[+] Filosofo {id_philosopher} comiendo.");
                    Thread.Sleep(TimeSpan.FromSeconds(random.Next(3, 5)));
                    statesPhilosopher[id_philosopher] = StatesPhilosopher.finished_eat;
                    Console.WriteLine($"[!] Filosofo {id_philosopher} termino de comer.");

                    block.Release();
                }
                else
                {
                    statesPhilosopher[id_philosopher] = StatesPhilosopher.thinking;
                    Console.WriteLine($"[*] Filosofo {id_philosopher} pensando.");
                    Thread.Sleep(TimeSpan.FromSeconds(random.Next(2, 5)));
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public void Run()
        {
            for (int i = 0; i < philosopherThread.Length; i++)
            {
                int idx = i;
                philosopherThread[idx] = new Thread(() => Eat(idx));
                philosopherThread[idx].Start();
            }

            for (int i = 0; i < philosopherThread.Length; i++)
            {
                philosopherThread[i].Join();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Philosopher forTurn = new Philosopher();
            forTurn.Run();
        }
    }
}
