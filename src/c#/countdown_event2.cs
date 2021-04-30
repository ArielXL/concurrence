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
        public static string[] students = { "Alain", "Alberto", "Jose Jorge",
            "Alejandro", "David", "Christian", "Enmanuel", "Andres", "Hector" };
        public static Random random;
        public static MyCountdownEvent my_countdown;

        public static void Arrive(int student)
        {
            Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 5)));
            Console.WriteLine($"El alumno {students[student]} llega al aula.");
            my_countdown.Signal();
        }

        public static void ArrivePeople()
        {
            for (int i = 0; i < students.Length; i++)
            {
                new Thread(() => Arrive(i)).Start();
                Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 5)));
            }
        }

        static void Main(string[] args)
        {
            random = new Random();
            my_countdown = new MyCountdownEvent(students.Length);
            Console.WriteLine("Son las 7:55 am. Va a empezar la clase.");
            Console.WriteLine();

            new Thread(() => ArrivePeople()).Start();
            my_countdown.Wait();

            Console.WriteLine();
            Console.WriteLine("8:00 am!!! Empieza la clase.");
        }
    }
}
