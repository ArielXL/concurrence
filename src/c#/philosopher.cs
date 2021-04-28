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
        left_fork = 3,
        right_fork = 4,
        finished_eat = 5
    }

    public class Philosopher
    {
        private Semaphore[] forks;
        private bool[] used_forks;
        private Thread[] philosopherThread;
        private StatesPhilosopher[] statesPhilosopher;

        public int CountPhilosopher { get; private set; }
        public int MaxFailed { get; set; }

        public Philosopher(int countPhilosopher = 5, int maxFailed = 3)
        {
            forks = new Semaphore[countPhilosopher];
            used_forks = new bool[countPhilosopher];
            philosopherThread = new Thread[countPhilosopher];
            statesPhilosopher = new StatesPhilosopher[countPhilosopher];
            CountPhilosopher = countPhilosopher;
            MaxFailed = maxFailed;

            for (int i = 0; i < countPhilosopher; i++)
            {
                used_forks[i] = false;
                forks[i] = new Semaphore(1, 1);
                statesPhilosopher[i] = StatesPhilosopher.thinking;
            }
        }

        private int GetLeftFork(int id_philosopher)
        {
            return id_philosopher;
        }

        private int GetRightFork(int id_philosopher)
        {
            return (id_philosopher + 1) % CountPhilosopher;
        }

        private void TakeLeftFork(int id_philosopher)
        {
            int id_left_fork = GetLeftFork(id_philosopher);
            if (!used_forks[id_left_fork])
            {
                forks[id_left_fork].WaitOne();
                used_forks[id_left_fork] = true;
                Console.WriteLine($"[<=] Filosofo {id_philosopher} cogio el tenedor izquierdo.");
                statesPhilosopher[id_left_fork] = StatesPhilosopher.left_fork;
            }
        }

        private void TakeRightFork(int id_philosopher)
        {
            int id_right_fork = GetRightFork(id_philosopher);
            if (!used_forks[id_right_fork])
            {
                forks[id_right_fork].WaitOne();
                used_forks[id_right_fork] = true;
                Console.WriteLine($"[=>] Filosofo {id_philosopher} cogio el tenedor derecho.");
                statesPhilosopher[id_right_fork] = StatesPhilosopher.right_fork;
            }
        }

        private void ReleaseLeftFork(int id_philosopher)
        {
            int id_left_fork = GetLeftFork(id_philosopher);
            if (used_forks[id_left_fork])
            {
                forks[id_left_fork].Release();
                used_forks[id_left_fork] = false;
                Console.WriteLine($"[<-] Filosofo {id_philosopher} solto el tenedor izquierdo.");
            }
        }

        private void ReleaseRightFork(int id_philosopher)
        {
            int id_right_fork = GetRightFork(id_philosopher);
            if (used_forks[id_right_fork])
            {
                forks[id_right_fork].Release();
                used_forks[id_right_fork] = false;
                Console.WriteLine($"[->] Filosofo {id_philosopher} solto el tenedor derecho.");
            }
        }

        private void Eat(int id_philosopher)
        {
            Random random = new Random();

            while (true)
            {
                if (random.Next(0, 2) == 0)
                {
                    statesPhilosopher[id_philosopher] = StatesPhilosopher.hungry;
                    Console.WriteLine($"[@] Filosofo {id_philosopher} hambiento.");

                    if (!used_forks[GetLeftFork(id_philosopher)])
                    {
                        TakeLeftFork(id_philosopher);
                        int failed = 0;

                        while (true)
                        {
                            if (failed == MaxFailed)
                            {
                                failed = 0;
                                ReleaseLeftFork(id_philosopher);
                                break;
                            }
                            else if (!used_forks[GetRightFork(id_philosopher)])
                            {
                                failed = 0;
                                TakeRightFork(id_philosopher);
                                Console.WriteLine($"[+] Filosofo {id_philosopher} comiendo.");
                                statesPhilosopher[id_philosopher] = StatesPhilosopher.eating;
                                Thread.Sleep(TimeSpan.FromSeconds(random.Next(3, 6)));
                                statesPhilosopher[id_philosopher] = StatesPhilosopher.finished_eat;
                                Console.WriteLine($"[!] Filosofo {id_philosopher} termino de comer.");
                                ReleaseLeftFork(id_philosopher);
                                ReleaseRightFork(id_philosopher);
                                break;
                            }
                            else
                            {
                                failed += 1;
                                Console.WriteLine($"[{failed}] Filosofo {id_philosopher} no pudo coger el tenedor derecho porque estaba ocupado.");
                            }
                            Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 4)));
                        }
                        statesPhilosopher[id_philosopher] = StatesPhilosopher.thinking;
                        Console.WriteLine($"[*] Filosofo {id_philosopher} pensando.");
                        Thread.Sleep(TimeSpan.FromSeconds(random.Next(2, 6)));
                    }
                    else if (!used_forks[GetRightFork(id_philosopher)])
                    {
                        TakeRightFork(id_philosopher);
                        int failed = 0;

                        while (true)
                        {
                            if (failed == MaxFailed)
                            {
                                failed = 0;
                                ReleaseRightFork(id_philosopher);
                                break;
                            }
                            else if (!used_forks[GetLeftFork(id_philosopher)])
                            {
                                failed = 0;
                                TakeLeftFork(id_philosopher);
                                Console.WriteLine($"[+] Filosofo {id_philosopher} comiendo.");
                                statesPhilosopher[id_philosopher] = StatesPhilosopher.eating;
                                Thread.Sleep(TimeSpan.FromSeconds(random.Next(3, 6)));
                                statesPhilosopher[id_philosopher] = StatesPhilosopher.finished_eat;
                                Console.WriteLine($"[!] Filosofo {id_philosopher} termino de comer.");
                                ReleaseLeftFork(id_philosopher);
                                ReleaseRightFork(id_philosopher);
                                break;
                            }
                            else
                            {
                                failed += 1;
                                Console.WriteLine($"[{failed}] Filosofo {id_philosopher} no pudo coger el tenedor izquierdo porque estaba ocupado.");
                            }
                            Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 4)));
                        }
                        statesPhilosopher[id_philosopher] = StatesPhilosopher.thinking;
                        Console.WriteLine($"[*] Filosofo {id_philosopher} pensando.");
                        Thread.Sleep(TimeSpan.FromSeconds(random.Next(2, 6)));
                    }
                    else
                    {
                        statesPhilosopher[id_philosopher] = StatesPhilosopher.thinking;
                        Console.WriteLine($"[*] Filosofo {id_philosopher} pensando.");
                        Thread.Sleep(TimeSpan.FromSeconds(random.Next(2, 6)));
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
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
            Philosopher forForkQueue = new Philosopher(5, 3);
            forForkQueue.Run();
        }
    }
}
