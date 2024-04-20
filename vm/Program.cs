namespace vm
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadAllText("target-code.txt");
            VirtualMachine vm = new VirtualMachine(input);
            vm.Run();
        }
    }
}
