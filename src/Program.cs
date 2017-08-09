using System;
using System.IO;

namespace AA
{
    class Program
    {
        static IAnalyzer dllAnalyzer = new ReflectionAnalyzer();

        static void Main(string[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Usage: AA SearchPath OutputPath");
            var sourcePath = args[0].Trim();
            var outputPath = args[1].Trim();
            ProcessAllAssemblies(sourcePath, outputPath);
            Console.WriteLine("Done.");
        }

        static void ProcessAllAssemblies(string sourcePath, string outputPath)
        {
            if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException($"Directory {sourcePath} does not exist");
            var allDlls = Directory.EnumerateFiles(sourcePath, "*.dll", SearchOption.AllDirectories);
            foreach (var dll in allDlls)
            {
                dllAnalyzer.Analyze(dll, outputPath);
            }
        }
    }
}
