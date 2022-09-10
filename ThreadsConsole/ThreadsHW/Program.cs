using Bogus;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;

namespace ThreadsHW
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Faker<Dragon> faker = new Faker<Dragon>()
                .RuleFor(x => x.Name, f => f.Person.FullName)
                .RuleFor(x => x.Image, f => f.Image.LoremFlickrUrl());
            Thread[] threads = new Thread[1000];

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                var dragon = faker.Generate();
                threads[i] = new Thread(DownloadImage);
                threads[i].Start(dragon);
            }
            for (int i = 0; i < 1000; i++)
            {
                threads[i].Join();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine(" Threaded Download Photos RunTime " + elapsedTime);
        }

        static void DownloadImage(object dragon)
        {
            try
            {
                Dragon tmp = (Dragon)dragon;
                using (WebClient client = new WebClient())
                {
                    string fileName = Path.GetRandomFileName();
                    string path = Directory.GetCurrentDirectory() + $"\\Images\\{fileName}.png";
                    client.DownloadFile(new Uri(tmp.Image), path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
// Downloading 1000 images without threads takes 00:04:13.36
// Downloading 1000 images with threads takes less than 1 second without waiting their ending
// Downloading 1000 images with threads takes 20-30 seconds with waiting their end (takes more time because of the internet)

/*
    Зробити генерацію 1000 драконів.
    Потрібно зробити програму, щоб у окремому потоці фото зберігалися на ПК в папку.
    Протестувати швидкість роботи із потоками та без потоків.
    Кожна окреме фото оремий потік.
*/