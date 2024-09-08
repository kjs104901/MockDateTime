namespace MockDateTime.Test;

class Program
{
    static void Main(string[] args)
    {
        // launch 4 threads and print the current time
        for (int i = 0; i < 4; i++)
        {
            new Thread(() =>
            {
                Console.WriteLine($"{i} {DateTime.Now}");
                Thread.Sleep(3000);
                Console.WriteLine($"{i} {DateTime.Now}");
            }).Start();
        }
        
        MockTimeProvider.Init();
        
        Console.WriteLine($"Main {DateTime.Now}");
    }
}