using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace MutexThreadsApp
{
    internal class Program
    {
        static Mutex fileMutex = new Mutex();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Thread t1 = new Thread(GenerateNumbers);
            Thread t2 = new Thread(FilterPrimes);
            Thread t3 = new Thread(FilterEndsWith7);
            Thread t4 = new Thread(AnalyzeFiles);

            Console.WriteLine("Запуск потоків\n");

            t1.Start();
            t1.Join();

            t2.Start();
            t2.Join();

            t3.Start();
            t3.Join();

            t4.Start();
            t4.Join();

            Console.WriteLine("\nВсі потоки завершено!");
            Console.ReadKey();
        }

        static void GenerateNumbers()
        {
            fileMutex.WaitOne();
            try
            {
                Random rnd = new Random();
                int[] numbers = Enumerable.Range(0, 100).Select(_ => rnd.Next(1, 10000)).ToArray();
                File.WriteAllLines("numbers.txt", numbers.Select(n => n.ToString()));
                Console.WriteLine("Згенеровано 100 чисел і записано у numbers.txt");
            }
            finally
            {
                fileMutex.ReleaseMutex();
            }
        }

        static void FilterPrimes()
        {
            fileMutex.WaitOne();
            try
            {
                var numbers = File.ReadAllLines("numbers.txt").Select(int.Parse);
                var primes = numbers.Where(IsPrime).ToArray();
                File.WriteAllLines("primes.txt", primes.Select(p => p.ToString()));
                Console.WriteLine("Створено primes.txt з простими числами");
            }
            finally
            {
                fileMutex.ReleaseMutex();
            }
        }

        static void FilterEndsWith7()
        {
            fileMutex.WaitOne();
            try
            {
                var primes = File.ReadAllLines("primes.txt").Select(int.Parse);
                var ends7 = primes.Where(n => n % 10 == 7).ToArray();
                File.WriteAllLines("ends7.txt", ends7.Select(n => n.ToString()));
                Console.WriteLine("Створено ends7.txt (числа, що закінчуються на 7)");
            }
            finally
            {
                fileMutex.ReleaseMutex();
            }
        }

        static void AnalyzeFiles()
        {
            fileMutex.WaitOne();
            try
            {
                Console.WriteLine("\nАналіз файлів:");

                foreach (string file in new[] { "numbers.txt", "primes.txt", "ends7.txt" })
                {
                    if (!File.Exists(file))
                    {
                        Console.WriteLine($"{file} — файл відсутній!");
                        continue;
                    }

                    var content = File.ReadAllLines(file);
                    var size = new FileInfo(file).Length;

                    Console.WriteLine($"\n{file}:");
                    Console.WriteLine($"Кількість чисел: {content.Length}");
                    Console.WriteLine($"Розмір: {size} байт");
                    Console.WriteLine($"Вміст: {string.Join(", ", content)}");
                }
            }
            finally
            {
                fileMutex.ReleaseMutex();
            }
        }

        static bool IsPrime(int n)
        {
            if (n < 2) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;
            for (int i = 3; i * i <= n; i += 2)
                if (n % i == 0) return false;
            return true;
        }
    }
}
