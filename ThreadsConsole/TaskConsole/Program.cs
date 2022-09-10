using System;

namespace TaskConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Продвінутий Thread
            //Task Працює в окремому потоці і програма завершує свою роботу не дочекавшись завершення роботи потока
            Task task = new Task(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(500);
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    Console.WriteLine("Hello my task run " + threadId);
                }
            });
            task.Start();
            task.Wait(); // Чекає завершення Task
            Console.WriteLine("Main program has ended: " + Thread.CurrentThread.ManagedThreadId);

            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine("Main Thread");
            //    Thread.Sleep(500);
            //}
            

        }
    }
}
