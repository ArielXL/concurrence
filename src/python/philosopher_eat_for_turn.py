import enum
import time
import random
import threading

class StatesPhilosopher(enum.Enum):
    '''
    Representa los posibles estados de los filosofos.
    '''
    thinking = 1
    hungry = 2
    eating = 3
    finished_eat = 4

class Philosopher:

    def __init__(self, count_philosopher=5):
        self.count_philosopher = count_philosopher
        self.mutex = threading.Semaphore(1)
        self.state_philosopher = [ StatesPhilosopher.thinking 
                                    for _ in range(self.count_philosopher) ]
        self.start_threads()

    def eat(self, id_philosopher):
        '''
        Funcion que va ejecutar cada filosofo o hilo.
        '''
        while True:

            if random.randint(0, 2) == 0:
                self.mutex.acquire()

                self.state_philosopher[id_philosopher] = StatesPhilosopher.hungry
                print(f'[@] Filosofo {id_philosopher} hambiento.')
                
                self.state_philosopher[id_philosopher] = StatesPhilosopher.eating
                print(f'[+] Filosofo {id_philosopher} comiendo.')
                time.sleep(random.randint(3, 5))
                self.state_philosopher[id_philosopher] = StatesPhilosopher.finished_eat
                print(f'[!] Filosofo {id_philosopher} termino de comer.')
                
                self.mutex.release()
            else:
                self.state_philosopher[id_philosopher] = StatesPhilosopher.thinking
                print(f'[*] Filosofo {id_philosopher} pensando.')
                time.sleep(random.randint(2, 5))

            time.sleep(1)

    def start_threads(self):
        '''
        Comienza a correr los hilos correspondiente 
        a cada filosofo.
        '''
        threads = [ threading.Thread(target=self.eat, args=(id,)) 
                        for id in range(self.count_philosopher) ]

        for thread in threads:
            thread.start()

        for thread in threads:
            thread.join()

def main():
    
    Philosopher()

if __name__ == '__main__':
    main()