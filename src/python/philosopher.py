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
    left_fork = 4
    right_fork = 5
    finished_eat = 6

class Philosopher:

    def __init__(self, count_philosopher=5, max_failed=3):
        self.count_philosopher = count_philosopher
        self.max_failed = max_failed

        self.forks = [ threading.Semaphore(1) for _ in range(self.count_philosopher) ]
        self.used_fork = [ False for _ in range(self.count_philosopher) ]
        self.state_philosopher = [ StatesPhilosopher.thinking 
                                    for _ in range(self.count_philosopher) ]
        self.start_threads()

    def get_left_fork(self, id_philosopher):
        '''
        Devuelve el tenedor izquierdo del filosofo especificado.
        '''
        return id_philosopher
    
    def get_right_fork(self, id_philosopher):
        '''
        Devuelve el tenedor derecho del filosofo especificado.
        '''
        return (id_philosopher + 1) % self.count_philosopher

    def take_left_fork(self, id_philosopher):
        '''
        Intenta tomar el tenedor izquierdo del filosofo especificado.
        '''
        id_left_fork = self.get_left_fork(id_philosopher)
        if not self.used_fork[id_left_fork]:
            self.forks[id_left_fork].acquire()
            self.used_fork[id_left_fork] = True
            print(f'[<=] Filosofo {id_philosopher} cogio el tenedor izquierdo.')
            self.state_philosopher[id_left_fork] = StatesPhilosopher.left_fork

    def take_right_fork(self, id_philosopher):
        '''
        Intenta tomar el tenedor derecho del filosofo especificado.
        '''
        id_right_fork = self.get_right_fork(id_philosopher)
        if not self.used_fork[id_right_fork]:
            self.forks[id_right_fork].acquire()
            self.used_fork[id_right_fork] = True
            print(f'[=>] Filosofo {id_philosopher} cogio el tenedor derecho.')
            self.state_philosopher[id_right_fork] = StatesPhilosopher.right_fork

    def release_left_fork(self, id_philosopher):
        '''
        Libera el tenedor izquierdo del filosofo especificado.
        '''
        id_left_fork = self.get_left_fork(id_philosopher)
        if self.used_fork[id_left_fork]:
            self.forks[id_left_fork].release()
            self.used_fork[id_left_fork] = False
            print(f'[<-] Filosofo {id_philosopher} solto el tenedor izquierdo.')

    def release_right_fork(self, id_philosopher):
        '''
        Libera el tenedor derecho del filosofo especificado.
        '''
        id_right_fork = self.get_right_fork(id_philosopher)
        if self.used_fork[id_right_fork]:
            self.forks[id_right_fork].release()
            self.used_fork[id_right_fork] = False
            print(f'[<-] Filosofo {id_philosopher} solto el tenedor derecho.')

    def eat(self, id_philosopher):
        '''
        Funcion que va ejecutar cada filosofo o hilo.
        '''
        while True:

            if random.randint(0, 1) == 0:

                self.state_philosopher[id_philosopher] = StatesPhilosopher.hungry
                print(f'[@] Filosofo {id_philosopher} hambiento.')

                if not self.used_fork[self.get_left_fork(id_philosopher)]:
                    self.take_left_fork(id_philosopher)
                    failed = 0
                    while True:
                        if failed == self.max_failed:
                            failed = 0
                            self.release_left_fork(id_philosopher)
                            break
                        elif not self.used_fork[self.get_right_fork(id_philosopher)]:
                            failed = 0
                            self.take_right_fork(id_philosopher)
                            print(f'[+] Filosofo {id_philosopher} comiendo.')
                            self.state_philosopher[id_philosopher] = StatesPhilosopher.eating
                            time.sleep(random.randint(3, 5))
                            self.state_philosopher[id_philosopher] = StatesPhilosopher.finished_eat
                            print(f'[!] Filosofo {id_philosopher} termino de comer.')
                            self.release_left_fork(id_philosopher)
                            self.release_right_fork(id_philosopher)
                            break
                        else:
                            failed += 1
                            print(f'[{failed}] Filosofo {id_philosopher} no pudo coger el tenedor derecho porque estaba ocupado.')
                        time.sleep(random.randint(1, 3))

                    self.state_philosopher[id_philosopher] = StatesPhilosopher.thinking
                    print(f'[*] Filosofo {id_philosopher} pensando.')
                    time.sleep(random.randint(2, 5))
                elif not self.used_fork[self.get_right_fork(id_philosopher)]:
                    self.take_right_fork(id_philosopher)
                    failed = 0
                    while True:
                        if failed == self.max_failed:
                            failed = 0
                            self.release_right_fork(id_philosopher)
                            break
                        elif not self.used_fork[self.get_left_fork(id_philosopher)]:
                            failed = 0
                            self.take_left_fork(id_philosopher)
                            print(f'[+] Filosofo {id_philosopher} comiendo.')
                            self.state_philosopher[id_philosopher] = StatesPhilosopher.eating
                            time.sleep(random.randint(3, 5))
                            self.state_philosopher[id_philosopher] = StatesPhilosopher.finished_eat
                            print(f'[!] Filosofo {id_philosopher} termino de comer.')
                            self.release_left_fork(id_philosopher)
                            self.release_right_fork(id_philosopher)
                            break
                        else:
                            failed += 1
                            print(f'[{failed}] Filosofo {id_philosopher} no pudo coger el tenedor izquierdo porque estaba ocupado.')
                        time.sleep(random.randint(1, 3))

                    self.state_philosopher[id_philosopher] = StatesPhilosopher.thinking
                    print(f'[*] Filosofo {id_philosopher} pensando.')
                    time.sleep(random.randint(2, 5))

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