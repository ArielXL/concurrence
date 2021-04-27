import time
import queue
import threading

class SleepyBarber:

    def __init__(self, count_chairs=5, time_cut=3, 
                                    time_client=2, max_clients=20):
        self.count_chairs = count_chairs
        self.time_cut = time_cut
        self.time_client = time_client
        self.max_clients = max_clients
        self.count_sleepy_barber = 0
        self.count_in_client = 0
        self.count_out_client = 0
        self.clients = queue.Queue(self.count_chairs)
        self.id = 0
        self.block = threading.Semaphore(1)

        self.start_threads()
        self.print_results()

    def print_results(self):
        '''
        Imprime los resultados de la simulacion.
        '''
        print(f'\nCantidad de sillas : {self.count_chairs}')
        print(f'Tiempo que se demora el barbero pelando : {self.time_cut} segundos')
        print(f'Tiempo que demoran llegar los clientes : {self.time_client} segundos')
        print(f'Cantidad de veces que el barbero se duerme : {self.count_sleepy_barber}')
        print(f'Cantidad de clientes que pudieron entrar : {self.count_in_client}')
        print(f'Cantidad de clientes que no pudieron entrar : {self.count_out_client}')
        print(f'Total de clientes : {self.id}')
        print(f'Por ciento de los clientes pelados : {round(self.count_in_client / self.id * 100, 2)} %')

    def generate_clients(self):
        '''
        Funcion encargada de generar los clientes 
        que llegan a la barberia.
        '''
        while True:

            if self.id == self.max_clients:
                return

            self.id += 1
            self.block.acquire()

            if self.clients.full():
                self.count_out_client += 1
                print(f'El cliente {self.id} no pudo entrar porque la barberia esta llena.')
            else:
                self.clients.put(self.id)
                self.count_in_client += 1
                print(f'El cliente {self.id} llega a la barberia.')

            self.block.release()
            time.sleep(self.time_client)

    def barber(self):
        '''
        Funcion encargada de simular las acciones del barbero.
        '''
        while True:
            
            if self.id == self.max_clients and self.clients.empty():
                return

            self.block.acquire()

            if not self.clients.empty():
                id_client = self.clients.get()
                self.block.release()

                print(f'El barbero comenzo a cortarle el cabello al cliente {id_client}.')
                time.sleep(self.time_cut)
                print(f'El barbero termino de cortarle el cabello al cliente {id_client}.')

                if self.id == self.max_clients and self.clients.empty():
                    return
                elif self.clients.empty():
                    self.count_sleepy_barber += 1
                    print('El barbero esta durmiendo.')
            else:
                self.count_sleepy_barber += 1
                print('El barbero esta durmiendo.')
                self.block.release()

            time.sleep(1)

    def start_threads(self):
        '''
        Comienza a correr los hilos.
        '''
        thread_generate_clients = threading.Thread(target=self.generate_clients)
        thread_barber = threading.Thread(target=self.barber)

        thread_generate_clients.start()
        thread_barber.start()
        thread_generate_clients.join()
        thread_barber.join()

def main():

    SleepyBarber(5, 3, 2, 20)
    # SleepyBarber(10, 3, 2, 50)
    # SleepyBarber(15, 1, 1, 60)
    # SleepyBarber(15, 5, 2, 60)
    # SleepyBarber(15, 2, 5, 60)
    # SleepyBarber(20, 2, 1, 100)

if __name__ == '__main__':
    main()