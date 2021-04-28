using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Sleepy_barber
{
    class SleepyBarber
    {
        private int id { get; set; }
        private Semaphore block { get; set; }
        private Queue<int> clients { get; set; }

        public int CountChairs { get; private set; }

        public SleepyBarber(int countChairs)
        {
            id = 1;
            block = new Semaphore(1, 1);
            clients = new Queue<int>();
            CountChairs = countChairs;
        }

        public void Run()
        {
            Thread threadGenerateClients = new Thread(() => GenerateClients());
            Thread threadBarber = new Thread(() => Barber());

            threadGenerateClients.Start();
            threadBarber.Start();
            threadGenerateClients.Join();
            threadBarber.Join();
        }

        private void GenerateClients()
        {
            Random random = new Random();

            while (true)
            {
                block.WaitOne();

                if (clients.Count > CountChairs)
                    Console.WriteLine("La barberia esta llena.");
                else
                {
                    clients.Enqueue(id);
                    Console.WriteLine($"El cliente {id.ToString()} llega a la barberia.");
                    id++;
                }

                block.Release();
                Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 3)));
            }
        }

        private void Barber()
        {
            while (true)
            {
                block.WaitOne();

                if (clients.Count > 0)
                {
                    int client = clients.Dequeue();
                    block.Release();
                    Console.WriteLine($"El barbero empezo a cortarle el cabello al cliente {client}.");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    Console.WriteLine($"El barbero termino de cortarle el cabello al cliente {client}.");

                    if (clients.Count == 0)
                        Console.WriteLine("El barbero esta durmiendo.");
                }
                else
                {
                    Console.WriteLine("El barbero esta durmiendo.");
                    block.Release();
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SleepyBarber sleepyBarber = new SleepyBarber(5);
            sleepyBarber.Run();
        }
    }
}
