import time
import queue
import random
import threading

class SleepyBarber:

    def __init__(self, count_chairs=5):
        self.count_chairs = count_chairs
        self.clients = queue.Queue(self.count_chairs)
        self.id = 0
        self.block = threading.Semaphore(1)

        self.start_threads()

    def generate_clients(self):
        '''
        Funcion encargada de generar los clientes 
        que llegan a la barberia.
        '''
        
        while True:
            self.id += 1
            self.block.acquire()

            if self.clients.full():
                print(f'El cliente {self.id} no pudo entrar porque la barberia esta llena.')
            else:
                self.clients.put(self.id)
                print(f'El cliente {self.id} llega a la barberia.')

            self.block.release()
            time.sleep(random.randint(2, 4))

    def barber(self):
        '''
        Funcion encargada de simular las acciones del barbero.
        '''

        while True:
            self.block.acquire()

            if not self.clients.empty():
                id_client = self.clients.get()
                self.block.release()

                print(f'El barbero comenzo a cortarle el cabello al cliente {id_client}.')
                time.sleep(random.randint(1, 3))
                print(f'El barbero termino de cortarle el cabello al cliente {id_client}.')

                if self.clients.empty():
                    print('El barbero esta durmiendo.')
            else:
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

    SleepyBarber(5)

if __name__ == '__main__':
    main()