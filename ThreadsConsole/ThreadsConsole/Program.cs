using Bogus;
using System;
using System.Diagnostics;
using System.Globalization;

namespace ThreadsConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //THREADS !!!
            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.WriteLine("Cores' number in the system {0}", Environment.ProcessorCount); // number of cores(ядер) in the system
            Console.WriteLine("Main thread id: {0}", Thread.CurrentThread.ManagedThreadId); // id of the main thread
            Thread big_girl = new Thread(sendGirl); // потік має вказувати на якийсь метод
            big_girl.Start();
            var dragon = new Faker<Dragon>()
                .RuleFor(x => x.Name, f => f.Person.FullName)
                .RuleFor(x => x.Image, f => f.Image.LoremFlickrUrl());

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var list = new List<Dragon>();
            for (int i = 0; i < 100000; i++)
            {
                //Thread.Sleep(500); // stop for a half a second
                //Console.WriteLine("Hello! {0}", i); 
                list.Add(dragon.Generate());
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime MainThread " + elapsedTime);

            big_girl.Join(); // waits the end of thread work till go next.
            Console.WriteLine("Bye-bye. Thanks for participating :)");
            
        }

        static void sendGirl()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var dragon = new Faker<Dragon>()
                .RuleFor(x => x.Name, f => f.Person.FullName)
                .RuleFor(x => x.Image, f => f.Image.LoremFlickrUrl());

            Console.WriteLine("Thread id: {0}", Thread.CurrentThread.ManagedThreadId);
            var list = new List<Dragon>();
            for (int i = 0; i < 100000; i++)
            {
                //Thread.Sleep(500); // stop for a half a second
                //Console.WriteLine("Hello! {0}", i); 
                list.Add(dragon.Generate());
            }
            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime Send Girl " + elapsedTime);
        }
    }
}