using System;
using System.Collections.Generic;
using System.Threading;

class Player
{
    public int Id { get; }
    public int StartMoney { get; }
    public int Money { get; set; }
    public bool Finished { get; set; }

    public Player(int id, int startMoney)
    {
        Id = id;
        StartMoney = startMoney;
        Money = startMoney;
        Finished = false;
    }
}

class Program
{
    static SemaphoreSlim table = new SemaphoreSlim(5);
    static Random random = new Random();
    static List<Player> players = new List<Player>();
    static object consoleLock = new object();
    static int finishedCount = 0;

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        int playerCount = random.Next(20, 101);
        Console.WriteLine($"🎰🎰🎰Починається день у казино🎰🎰🎰. Гравців: {playerCount}\n");

        for (int i = 0; i < playerCount; i++)
        {
            players.Add(new Player(i + 1, random.Next(100, 1001)));
        }

        List<Thread> threads = new List<Thread>();
        foreach (var player in players)
        {
            Thread t = new Thread(() => Play(player));
            threads.Add(t);
            t.Start();
        }
        foreach (var t in threads)
            t.Join();

        Console.WriteLine("\nКінець дня. Підсумки:\n");
        foreach (var p in players)
        {
            Console.WriteLine($"Гравець {p.Id,-3} | Початкова сума: {p.StartMoney,5} | Кінцева сума: {p.Money,5}");
        }
    }

    static void Play(Player player)
    {
        while (!player.Finished)
        {
            table.Wait();

            if (player.Money <= 0)
            {
                FinishPlayer(player);
                table.Release();
                return;
            }

            int bet = random.Next(1, player.Money / 2 + 1);
            int chosenNumber = random.Next(0, 35);
            int winningNumber = random.Next(0, 35);

            bool win = chosenNumber == winningNumber;
            if (win)
                player.Money += bet * 10;
            else
                player.Money -= bet;

            lock (consoleLock)
            {
                Console.WriteLine($"Гравець {player.Id,3}: ставка {bet,4} на {chosenNumber,2} → {(win ? "ВИГРАВ" : "програв")}. Баланс: {player.Money,5}");
            }

            if (random.Next(0, 100) < 5 || player.Money <= 0)
            {
                FinishPlayer(player);
            }

            table.Release();
            Thread.Sleep(random.Next(100, 500));
        }
    }

    static void FinishPlayer(Player player)
    {
        if (!player.Finished)
        {
            player.Finished = true;
            Interlocked.Increment(ref finishedCount);
            lock (consoleLock)
            {
                Console.WriteLine($"Гравець {player.Id} залишив стіл. (Баланс: {player.Money})");
            }
        }
    }
}
