using System;

namespace Lab5
{
    class Program
    {
        static void Main(string[] args)
        {
            var N = int.Parse(Console.ReadLine());
            for(int i=0;i<N;i++)
            {
                var line = Console.ReadLine();
                Scanner scanner = new Scanner(line);
                Parser parser = new Parser(scanner);
                if (parser.error)
                {
                    Console.WriteLine("ERROR");
                }
                else
                {
                    Console.WriteLine(parser.Result);
                }
            }

            return;
        }
    }
}
