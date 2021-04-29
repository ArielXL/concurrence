using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Semaphores
{
    public class Almacen
    {
        public int[] Datos { get; private set; }
        public int IndiceProductor { get; private set; }
        public int IndiceConsumidor { get; private set; }
        
        private Semaphore bloqueo { get; set; }     // exclusion mutua
        private Semaphore vacios { get; set; }      // contador de elementos vacios
        private Semaphore llenos { get; set; }      // contador de elementos llenos

        public Almacen(int cantidad)
        {
            Datos = new int[cantidad];
            IndiceProductor = 0;
            IndiceConsumidor = 0;

            bloqueo = new Semaphore(1, 1);
            vacios = new Semaphore(cantidad, cantidad);
            llenos = new Semaphore(0, cantidad);
        }

        public void Producir(int elemento)
        {
            vacios.WaitOne();       // decrementar el contador de elementos vacios
            bloqueo.WaitOne();      // entrada de la seccion critica
            Datos[IndiceProductor] = elemento;
            IndiceProductor = (IndiceProductor + 1) % Datos.Length;
            Thread.Sleep(TimeSpan.FromSeconds(2));
            bloqueo.Release();      // salida de la seccion critica
            llenos.Release();       // incrementar el contador de elementos llenos
        }

        public int Consumir()
        {
            llenos.WaitOne();       // decrementar el contador de elementos llenos
            bloqueo.WaitOne();      // entrada de la seccion critica
            int elemento = Datos[IndiceConsumidor];
            IndiceConsumidor = (IndiceConsumidor + 1) % Datos.Length;
            Thread.Sleep(TimeSpan.FromSeconds(2));
            bloqueo.Release();      // salida de la seccion critica
            vacios.Release();       // incrementar el contador de elementos vacios
            return elemento;
        }
    }

    class Program
    {
        public static void Productor(Almacen almacen)
        {
            Random random = new Random();

            while (true)
            {
                int elemento = random.Next(0, 100);
                almacen.Producir(elemento);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Se produce el numero {elemento}.");
                Console.ResetColor();
                Thread.Sleep(TimeSpan.FromSeconds(random.Next(0, 2)));
            }
        }

        public static void Consumidor(Almacen almacen)
        {
            Random random = new Random();

            while (true)
            {
                int elemento = almacen.Consumir();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Se consume el numero {elemento}.");
                Console.ResetColor();
                Thread.Sleep(TimeSpan.FromSeconds(random.Next(8, 10)));
            }
        }

        static void Main(string[] args)
        {
            Almacen almacen = new Almacen(10);

            Thread hiloProductor = new Thread(() => Productor(almacen));
            Thread hiloConsumidor1 = new Thread(() => Consumidor(almacen));
            Thread hiloConsumidor2 = new Thread(() => Consumidor(almacen));

            hiloProductor.Start();
            hiloConsumidor1.Start();
            hiloConsumidor2.Start();
            hiloProductor.Join();
            hiloConsumidor1.Join();
            hiloConsumidor2.Join();
        }
    }
}
