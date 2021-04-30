using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Countdown
{
    public class MyCountdownEvent
    {
        private Semaphore block;
        private Semaphore countdown;
        private int waitingThreads;

        /// <summary>
        /// Obtiene el número de señales restantes necesario para establecer el evento.
        /// </summary>
        /// <returns>
        /// Devuelve el número de señales restantes necesario para establecer el evento.
        /// </returns>
        public int CurrentCount { get; private set; }

        /// <summary>
        /// Obtiene los números de señales que se necesitan inicialmente para establecer 
        /// el evento.
        /// </summary>
        /// <returns>
        /// Devuelve el número de señales que se necesitan inicialmente para establecer 
        /// el evento.
        /// </returns>
        public int InitialCount { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase MyCountdown con el recuento 
        /// especificado.
        /// </summary>
        /// <param name="initialCount">
        /// Número de señales necesarias inicialmente para establecer MyCountdown.
        /// </param>
        public MyCountdownEvent(int initialCount)
        {
            if (initialCount < 0)
                throw new ArgumentOutOfRangeException();

            InitialCount = initialCount;
            CurrentCount = initialCount;
            waitingThreads = 0;
            block = new Semaphore(1, 1);
            countdown = new Semaphore(0, int.MaxValue);
        }

        /// <summary>
        /// Incrementa en uno el recuento actual de MyCountdown.
        /// </summary>
        public void AddCount()
        {
            block.WaitOne();
            CurrentCount++;
            block.Release();
        }

        /// <summary>
        /// Restablece el valor de CurrentCount en el valor de InitialCount.
        /// </summary>
        public void Reset()
        {
            block.WaitOne();
            CurrentCount = InitialCount;
            block.Release();
        }

        /// <summary>
        /// Registra una señal y disminuye el valor de CurrentCount.
        /// </summary>
        /// <returns>
        /// Devuelve true si la señal hizo que el recuento alcanzara el valor cero de lo 
        /// contrario devuelve falso.
        /// </returns>
        public bool Signal()
        {
            block.WaitOne();
            if (CurrentCount > 1)
            {
                CurrentCount--;
                block.Release();
                return false;
            }
            else
            {
                countdown.Release(waitingThreads);
                waitingThreads = 0;
                block.Release();
                return true;
            }
        }

        /// <summary>
        /// Bloquea el subproceso actual hasta que se establezca el objeto MyCountdown.
        /// </summary>
        public void Wait()
        {
            block.WaitOne();
            if (CurrentCount > 0)
            {
                waitingThreads++;
                block.Release();
                countdown.WaitOne();
            }
            else
                block.Release();
        }
    }

    class Program
    {
        public static string[] teams = { "Cuba", "Francia", "Italia", "Rusia", "Alemania" };
        public static MyCountdownEvent countdown = new MyCountdownEvent(teams.Length);
        public static List<string> places = new List<string>();

        public static void Winner(int team)
        {
            Random random = new Random();

            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 4)));

                if (random.Next(0, 4) == 0)
                {
                    Console.WriteLine($"El equipo {teams[team]} llega a la meta.");
                    places.Add(teams[team]);
                    countdown.Signal();
                    break;
                }
            }
        }

        public static void RunTeam()
        {
            for (int i = 0; i < teams.Length; i++)
            {
                new Thread(() => Winner(i)).Start();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("A sus marcas, listos, fuera!!!");
            Console.WriteLine();

            new Thread(() => RunTeam()).Start();
            countdown.Wait();

            Console.WriteLine();
            Console.WriteLine("Estamos listos para la premiacion");
            for (int i = 0; i < places.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {places[i]}");
            }
        }
    }
}
