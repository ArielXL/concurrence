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
        /// Obtiene el n�mero de la fase actual de la barrera.
        /// </summary>
        /// <returns>
        /// Devuelve el n�mero de la fase actual de la barrera.
        /// </returns>
        public int ParticipantCount { get; private set; }

        /// <summary>
        /// Obtiene el n�mero total de participantes de la barrera.
        /// </summary>
        /// <returns>
        /// Devuelve el n�mero total de participantes de la barrera.
        /// </returns>
        public long CurrentPhaseNumber { get; private set; }

        /// <summary>
        /// Obtiene el n�mero de participantes de la barrera que no a�n no se han 
        /// se�alado en la fase actual.
        /// </summary>
        /// <returns>
        /// Devuelve el n�mero de participantes de la barrera que no a�n no se han 
        /// se�alado en la fase actual.
        /// </returns>
        public int ParticipantsRemaining { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase MyBarrier.
        /// </summary>
        /// <param name="participantCount">
        /// N�mero de subprocesos que participan.
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
        /// N�mero de fase de la barrera en la que primero participar�n los nuevos 
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
        /// Se�ala que un participante ha alcanzado la barrera y espera a que todos los 
        /// dem�s participantes alcancen tambi�n la barrera.
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
        public static Barrier my_barrier;

        public static void Play(string name, string message, int time)
        {
            for (int i = 1; i <= 3; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(time));
                Console.WriteLine("{0} empieza a {1}", name, message);

                Thread.Sleep(TimeSpan.FromSeconds(time));
                Console.WriteLine("{0} finaliza de {1}", name, message);

                my_barrier.SignalAndWait();
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            my_barrier = new Barrier(3);

            Thread thread1 = new Thread(() => Play("El guitarrista",
                "tocar su guitarra.", 2));
            Thread thread2 = new Thread(() => Play("El vocalista",
                "cantar su canci�n.", 3));
            Thread thread3 = new Thread(() => Play("El pianista",
                "tocar el piano.", 4));

            thread1.Start();
            thread2.Start();
            thread3.Start();

            thread1.Join();
            thread2.Join();
            thread3.Join();
        }
    }
}
