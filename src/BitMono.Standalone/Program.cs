using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

internal class Program
{
    private static Task Main(string[] args)
    {
        if (Type.GetType("Mono.Runtime") != null)
        {
            Console.WriteLine("Mono!");
        }
        else
        {
            Console.WriteLine("Something other!");
        }

        Console.Write("Enter path to the file: ");
        var file = Console.ReadLine();

        Console.WriteLine(file);

        Assembly.Load(File.ReadAllBytes(file));

        Console.WriteLine("Dll loaded!");
        return Task.CompletedTask;
    }
}