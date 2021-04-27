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
        self.forks = [ threading.Semaphore(1) 
                                    for _ in range(self.count_philosopher) ]
        self.state_philosopher = [ StatesPhilosopher.thinking 
                                    for _ in range(self.count_philosopher) ]
        self.start_threads()

    def take_forks(self, id_philosopher):
        '''
        Intenta tomar los tenedores adyacentes 
        al filosofo especificado.
        '''
        self.forks[id_philosopher].acquire()
        self.forks[(id_philosopher + 1) % self.count_philosopher].acquire()

    def release_forks(self, id_philosopher):
        '''
        Libera los tenedores adyacentes del filosofo especificado.
        '''
        self.forks[id_philosopher].release()
        self.forks[(id_philosopher + 1) % self.count_philosopher].release()

    def eat(self, id_philosopher):
        '''
        Funcion que va ejecutar cada filosofo o hilo.
        '''
        while True:

            if random.randint(0, 2) == 0:
                self.state_philosopher[id_philosopher] = StatesPhilosopher.thinking
                print(f'[*] Filosofo {id_philosopher} pensando.')
                time.sleep(random.randint(2, 5))
            else:
                self.state_philosopher[id_philosopher] = StatesPhilosopher.hungry
                print(f'[@] Filosofo {id_philosopher} hambiento.')
                
                self.take_forks(id_philosopher)

                self.state_philosopher[id_philosopher] = StatesPhilosopher.eating
                print(f'[+] Filosofo {id_philosopher} comiendo.')
                time.sleep(random.randint(3, 5))
                self.state_philosopher[id_philosopher] = StatesPhilosopher.finished_eat
                print(f'[!] Filosofo {id_philosopher} termino de comer.')

                self.release_forks(id_philosopher)

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