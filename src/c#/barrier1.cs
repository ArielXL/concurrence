using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Barriers
{
    public class MyBarrier
    {
        private Semaphore block;
        private Semaphore barrier;

        /// <summary>
        /// Obtiene el número de la fase actual de la barrera.
        /// </summary>
        /// <returns>
        /// Devuelve el número de la fase actual de la barrera.
        /// </returns>
        public int ParticipantCount { get; private set; }

        /// <summary>
        /// Obtiene el número total de participantes de la barrera.
        /// </summary>
        /// <returns>
        /// Devuelve el número total de participantes de la barrera.
        /// </returns>
        public long CurrentPhaseNumber { get; private set; }

        /// <summary>
        /// Obtiene el número de participantes de la barrera que no aún no se han 
        /// señalado en la fase actual.
        /// </summary>
        /// <returns>
        /// Devuelve el número de participantes de la barrera que no aún no se han 
        /// señalado en la fase actual.
        /// </returns>
        public int ParticipantsRemaining { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase MyBarrier.
        /// </summary>
        /// <param name="participantCount">
        /// Número de subprocesos que participan.
        /// </param>
        public MyBarrier(int participantCount)
        {
            block = new Semaphore(1, 1);
            barrier = new Semaphore(0, participantCount - 1);

            CurrentPhaseNumber = 1;
            ParticipantCount = participantCount;
            ParticipantsRemaining = participantCount;
        }

        /// <summary>
        /// Notifica que va a haber un participante adicional.
        /// </summary>
        /// <returns>
        /// Número de fase de la barrera en la que primero participarán los nuevos 
        /// participantes.
        /// </returns>
        public long AddParticipant()
        {
            block.WaitOne();
            ParticipantCount++;
            ParticipantsRemaining++;
            block.Release();
            return CurrentPhaseNumber;
        }

        /// <summary>
        /// Notifica que va a haber un participante menos.
        /// </summary>
        public void RemoveParticipant()
        {
            block.WaitOne();
            if (ParticipantCount == 0)
                throw new InvalidOperationException();

            ParticipantCount--;
            ParticipantsRemaining--;
            block.Release();
        }

        /// <summary>
        /// Señala que un participante ha alcanzado la barrera y espera a que todos los 
        /// demás participantes alcancen también la barrera.
        /// </summary>
        public void SignalAndWait()
        {
            block.WaitOne();
            if (ParticipantsRemaining > 1)
            {
                ParticipantsRemaining--;
                block.Release();
                barrier.WaitOne();
            }
            else
            {
                barrier.Release(ParticipantCount - 1);
                CurrentPhaseNumber++;
                ParticipantsRemaining = ParticipantCount;
                block.Release();
            }
        }
    }

    class Program
    {
        public class Caribes
        {
            private int winner_faculties { get; set; }
            private int[] points_faculties { get; set; }
            private Thread[] threads_faculties { get; set; }
            private MyBarrier barrier { get; set; }
            private Semaphore block { get; set; }

            public string[] Faculties { get; private set; }
            public string[] Sports { get; private set; }

            public Caribes(string[] faculties, string[] sports)
            {
                Faculties = (string[])faculties.Clone();
                Sports = (string[])sports.Clone();

                winner_faculties = 0;
                points_faculties = new int[faculties.Length];
                threads_faculties = new Thread[faculties.Length];
                barrier = new MyBarrier(faculties.Length);
                block = new Semaphore(1, 1);
            }

            private void PlaySport(int faculty)
            {
                Random random = new Random();

                for (int i = 0; i < Sports.Length; i++)
                {
                    winner_faculties = 0;
                    while (true)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 4)));

                        if (random.Next(0, 3) == 0)
                        {
                            block.WaitOne();
                            int points = 10 - winner_faculties;
                            points_faculties[faculty] += points;
                            Console.WriteLine($"La facultad {Faculties[faculty]} ha terminado con el deporte {Sports[i]} con {points} puntos.");
                            winner_faculties++;
                            block.Release();

                            barrier.SignalAndWait();
                            break;
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    Console.WriteLine();
                }
            }

            public void Run()
            {
                for (int i = 0; i < Faculties.Length; i++)
                {
                    int idx = i;
                    threads_faculties[idx] = new Thread(() => PlaySport(idx));
                }

                for (int i = 0; i < threads_faculties.Length; i++)
                {
                    threads_faculties[i].Start();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                for (int i = 0; i < threads_faculties.Length; i++)
                {
                    threads_faculties[i].Join();
                }
            }

            public void PrintResults()
            {
                List<Tuple<int, string>> faculties_points = new List<Tuple<int, string>>();

                for (int i = 0; i < points_faculties.Length; i++)
                {
                    faculties_points.Add(new Tuple<int, string>(points_faculties[i], Faculties[i]));
                }

                faculties_points.Sort();
                Console.WriteLine("Empieza la premiacion");

                for (int i = faculties_points.Count - 1, j = 0; i >= 0; i--, j++)
                {
                    Console.WriteLine($"{j + 1}. Facultad {faculties_points[i].Item2} con {faculties_points[i].Item1} puntos");
                }
            }
        }

        static void Main(string[] args)
        {
            string[] faculties = { "MATCOM", "ECONOMIA", "DERECHO", "TURISMO", "CONTABILIDAD" };
            string[] sports = { "Futbol", "Voleibol", "Ajedrez", "Natacion", "Beisbol", "Atletismo" };

            Console.WriteLine("Comienzan los juegos Caribes!!!");
            Console.WriteLine();

            Caribes caribes = new Caribes(faculties, sports);
            caribes.Run();

            Console.WriteLine();
            caribes.PrintResults();
        }
    }
}
