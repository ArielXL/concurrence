using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Monitors
{
    class SimpleMonitor
    {
        private Semaphore wait;
        private Semaphore block;
        private Semaphore monitor;
        private int waitingThreads;

        public int ActThread { get; private set; }

        public SimpleMonitor()
        {
            block = new Semaphore(1, 1);
            monitor = new Semaphore(1, 1);
            wait = new Semaphore(0, int.MaxValue);
            waitingThreads = 0;

            ActThread = -1;
        }

        public void Enter()
        {
            monitor.WaitOne();
            ActThread = Thread.CurrentThread.ManagedThreadId;
        }

        public void Exit()
        {
            monitor.Release();
            ActThread = -1;
        }

        public bool Wait()
        {
            Exit();
            block.WaitOne();
            waitingThreads++;
            block.Release();
            wait.WaitOne();
            Enter();
            return true;
        }

        public void Pulse()
        {
            block.WaitOne();
            if (waitingThreads > 0)
                waitingThreads--;
            block.Release();
            wait.Release();
        }

        public void PulseAll()
        {
            wait.Release(waitingThreads);
            block.WaitOne();
            waitingThreads = 0;
            block.Release();
        }
    }

    public static class MyMonitor
    {
        private static Dictionary<object, SimpleMonitor> dictionary =
            new Dictionary<object, SimpleMonitor>();
        private static Semaphore monitor = new Semaphore(1, 1);

        /// <summary>
        /// Determina si el subproceso actual mantiene el bloqueo en el objeto especificado.
        /// </summary>
        /// <param name="obj">
        /// Objeto que se va a probar.
        /// </param>
        /// <returns>
        /// Devuelve true si el subproceso actual tiene el bloqueo obj, en caso contrario, 
        /// devuelve false.
        /// </returns>
        public static bool IsEntered(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!dictionary.ContainsKey(obj))
                throw new SynchronizationLockException();

            return dictionary[obj].ActThread == Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Adquiere un bloqueo exclusivo en el objeto especificado.
        /// </summary>
        /// <param name="obj">
        /// Objeto en el que se va a adquirir el bloqueo de monitor.
        /// </param>
        public static void Enter(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            monitor.WaitOne();
            if (!dictionary.ContainsKey(obj))
                dictionary.Add(obj, new SimpleMonitor());
            monitor.Release();
            dictionary[obj].Enter();
        }

        /// <summary>
        /// Libera un bloqueo exclusivo en el objeto especificado.
        /// </summary>
        /// <param name="obj">
        /// Objeto en el que se va a liberar el bloqueo.
        /// </param>
        public static void Exit(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!dictionary.ContainsKey(obj))
                throw new SynchronizationLockException();

            dictionary[obj].Exit();
        }

        /// <summary>
        /// Libera el bloqueo en un objeto y bloquea el subproceso actual hasta que vuelve 
        /// a adquirir el bloqueo.
        /// </summary>
        /// <param name="obj">
        /// Objeto en el que se va a esperar.
        /// </param>
        /// <returns>
        /// Devuelve true si la llamada fue devuelta porque el llamador volvió a adquirir 
        /// el bloqueo para el objeto especificado. Este método no devuelve ningún 
        /// resultado si el bloqueo no vuelve a adquirirse.
        /// </returns>
        public static bool Wait(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!dictionary.ContainsKey(obj) || Thread.CurrentThread.ManagedThreadId != dictionary[obj].ActThread)
                throw new SynchronizationLockException();

            return dictionary[obj].Wait();
        }

        /// <summary>
        /// Notifica un cambio de estado del objeto bloqueado al subproceso que se 
        /// encuentra en la cola de espera.
        /// </summary>
        /// <param name="obj">
        /// Objeto que está esperando un subproceso.
        /// </param>
        public static void Pulse(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!dictionary.ContainsKey(obj))
                throw new SynchronizationLockException();

            dictionary[obj].Pulse();
        }

        /// <summary>
        /// Notifica un cambio de estado del objeto a todos los subprocesos que se 
        /// encuentran en espera.
        /// </summary>
        /// <param name="obj">
        /// Objeto que envía el pulso.
        /// </param>
        public static void PulseAll(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!dictionary.ContainsKey(obj))
                throw new SynchronizationLockException();

            dictionary[obj].PulseAll();
        }
    }

    class Program
    {
        public class Cuenta
        {
            public string Titular { get; private set; }
            public float Saldo { get; protected set; }

            private object obj { get; set; }

            public Cuenta(string titular, float saldoInicial)
            {
                obj = new object();
                Titular = (string)titular.Clone();
                if (saldoInicial < 0)
                    throw new Exception("Hay que abrir una cuenta con saldo mayor que cero.");
                Saldo = saldoInicial;
            }

            public void Deposita(float cantidad)
            {
                if (cantidad <= 0)
                    throw new Exception("Cantidad a depositar debe ser mayor que cero.");
                MyMonitor.Enter(obj);
                Saldo += cantidad;
                MyMonitor.Exit(obj);
            }

            public virtual void Extrae(float cantidad)
            {
                if (cantidad <= 0)
                    throw new Exception("Cantidad a extraer debe ser mayor que cero.");
                MyMonitor.Enter(obj);
                Saldo -= cantidad;
                MyMonitor.Exit(obj);
            }
        }

        public static void DepositaUno(Cuenta cuenta)
        {
            for (int i = 0; i < 1000000; i++)
            {
                cuenta.Deposita(1);
            }
        }

        public static void ExtraeUno(Cuenta cuenta)
        {
            for (int i = 0; i < 1000000; i++)
            {
                cuenta.Extrae(1);
            }
        }

        static void Main(string[] args)
        {
            Cuenta cuentaAriel = new Cuenta("Ariel", 1000);

            Thread hiloDeposita = new Thread(() => DepositaUno(cuentaAriel));
            Thread hiloExtrae = new Thread(() => ExtraeUno(cuentaAriel));

            hiloDeposita.Start();
            hiloExtrae.Start();
            hiloDeposita.Join();
            hiloExtrae.Join();

            Console.WriteLine(cuentaAriel.Saldo);
        }
    }
}
