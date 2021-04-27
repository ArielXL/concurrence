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
        private Semaphore[] forks;
        private Thread[] philosopherThread;
        private StatesPhilosopher[] statesPhilosopher;

        public int CountPhilosopher { get; private set; }

        public Philosopher(int countPhilosopher = 5)
        {
            forks = new Semaphore[countPhilosopher];
            philosopherThread = new Thread[countPhilosopher];
            statesPhilosopher = new StatesPhilosopher[countPhilosopher];
            CountPhilosopher = countPhilosopher;

            for (int i = 0; i < countPhilosopher; i++)
            {
                forks[i] = new Semaphore(1, 1);
                statesPhilosopher[i] = StatesPhilosopher.thinking;
            }
        }

        private void TakeForks(int id_philosopher)
        {
            forks[id_philosopher].WaitOne();
            forks[(id_philosopher + 1) % CountPhilosopher].WaitOne();
        }

        private void ReleaseForks(int id_philosopher)
        {
            forks[id_philosopher].Release();
            forks[(id_philosopher + 1) % CountPhilosopher].Release();
        }

        private void Eat(int id_philosopher)
        {
            Random random = new Random();

            while (true)
            {
                if (random.Next(0, 2) == 0)
                {
                    statesPhilosopher[id_philosopher] = StatesPhilosopher.thinking;
                    Console.WriteLine($"[*] Filosofo {id_philosopher} pensando.");
                    Thread.Sleep(TimeSpan.FromSeconds(random.Next(2, 5)));
                }
                else
                {
                    statesPhilosopher[id_philosopher] = StatesPhilosopher.hungry;
                    Console.WriteLine($"[@] Filosofo {id_philosopher} hambriento.");

                    TakeForks(id_philosopher);

                    statesPhilosopher[id_philosopher] = StatesPhilosopher.eating;
                    Console.WriteLine($"[+] Filosofo {id_philosopher} comiendo.");
                    Thread.Sleep(TimeSpan.FromSeconds(random.Next(3, 5)));
                    statesPhilosopher[id_philosopher] = StatesPhilosopher.finished_eat;
                    Console.WriteLine($"[!] Filosofo {id_philosopher} termino de comer.");

                    ReleaseForks(id_philosopher);
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
            Philosopher forManyTurn = new Philosopher(5);
            forManyTurn.Run();
        }
    }
}
