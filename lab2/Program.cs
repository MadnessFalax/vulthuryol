using System.ComponentModel.DataAnnotations;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var file = new StreamReader("text.txt");
            var scanner = new Scanner(file);

            var token = scanner.NextToken();
            while(token.Type != TokenType.EOF) 
            {
                token.printToken();
                token = scanner.NextToken();
            }

            return;

        }
    }
}